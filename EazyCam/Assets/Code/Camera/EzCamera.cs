using UnityEngine;
using System.Collections;


#if UNITY_EDITOR
using System.Collections.Generic;

[ExecuteInEditMode]
#endif
public class EzCamera : MonoBehaviour 
{
    // Values to be set in the inspector
	[SerializeField] private EzCameraSettings m_settings = null;
    public EzCameraSettings Settings { get { return m_settings; } }

    [SerializeField] private Transform m_target = null;
    public Transform Target { get { return m_target; } }
    [SerializeField] private int m_playerLayer = 8;
    private int m_layermask = 0;
    private Transform m_transform = null;

    private Quaternion m_destRot = Quaternion.identity;

    private Vector3 m_relativePosition = Vector3.zero;

    // Camera Occlusion Variables
    private Camera m_camera = null;

    private Vector3[] m_nearClipPlanePoints = new Vector3[4];
    private Vector3[] m_originalClipPlanePoints = new Vector3[4];
    private Vector3 m_pointBehindCamera = new Vector3();

    private float m_nearPlaneDistance = 0f;
    private float m_aspectHalfHeight = 0f;
    private float m_aspectHalfWidth = 0f;

    private bool m_isOccluded = false;

    // State Machine and default state
    private EzStateMachine m_stateMachine = null;
    [SerializeField] private EzCameraState.State m_defaultState = EzCameraState.State.FOLLOW;
    public EzLockOnState.State DefaultState { get { return m_defaultState; } }

    // State for a stationary camera that rotates to look at a target but does not follow it
    private EzStationaryState m_stationaryState = null;
    public EzStationaryState StationaryState
    {
        get { return m_stationaryState; }
        set { m_stationaryState = value; }
    }

    // State for orbiting around a target
    private EzOrbitState m_orbitState = null;
    public EzOrbitState OrbitState
    {
        get { return m_orbitState; }
        set { m_orbitState = value; }
    }

    // State for tracking a target object's position around the environment
    private EzFollowState m_followState = null;
    public EzFollowState FollowState
    {
        get { return m_followState; }
        set { m_followState = value; }
    }

    /// <summary>
    /// Set the value to true if you want the camera to be able to track an object while still following the player
    /// </summary>
    private EzLockOnState m_lockOnState = null;
    public EzLockOnState LockOnState
    {
        get { return m_lockOnState; }
        set { m_lockOnState = value; }
    }
        
    [SerializeField] private bool m_allowLockOn = true;
    public bool AllowTargeting { get { return m_allowLockOn; } }
    public void SetAllowTargeting(bool allowTargeting)
    {
        m_allowLockOn = allowTargeting;
        if (m_allowLockOn)
        {
            if (m_lockOnState == null)
            {
                m_lockOnState = this.GetOrAddComponent<EzLockOnState>();
                m_lockOnState.Init(this, m_settings);

#if UNITY_EDITOR
                m_runtimeStatesAdded.Add(m_lockOnState);
#endif
            }
        }
    }

    private void Start()
    {
        m_transform = this.transform;
        m_camera = this.GetComponent<Camera>();
        m_nearPlaneDistance = m_camera.nearClipPlane;

        // reset the offset distance be 1/3 of the distance from the min to max
        m_settings.OffsetDistance = (m_settings.MaxDistance - m_settings.MinDistance) / 3f;
        m_settings.DesiredDistance = m_settings.OffsetDistance;
        m_settings.StoreDefaultValues();

        m_relativePosition = (m_target.position + (Vector3.up * m_settings.OffsetHeight)) + (m_transform.rotation * (Vector3.forward * -m_settings.OffsetDistance)) + (m_transform.right * m_settings.LateralOffset);
        m_transform.position = m_relativePosition;

        UpdateNearClipPlanePoints();

        m_layermask = 1 << m_playerLayer;
        m_layermask = ~m_layermask;

        if (m_allowLockOn)
        {
            m_lockOnState = this.GetOrAddComponent<EzLockOnState>();
            m_lockOnState.Init(this, m_settings);
        }

        m_stateMachine = new EzStateMachine();
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            if (m_stateMachine.CurrentState != null)
            {
                m_stateMachine.CurrentState.Init(this, m_settings);
            }
        }
#endif
        SetState(m_defaultState);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
#endif
            if (m_stateMachine != null)
            {
                HandleInput();
                m_stateMachine.UpdateState();
            }
#if UNITY_EDITOR
        }
#endif
    }

    private void LateUpdate()
    {
        if (m_target != null) // prevent updating if the target is null
        {
            if (m_stateMachine != null)
            {
                m_stateMachine.LateUpdateState();
            }


            UpdateNearClipPlanePoints();
#if UNITY_EDITOR
            DrawNearPlane();
#endif
            if (m_isOccluded)
            {
                CheckOriginalPlanePoints();
#if UNITY_EDITOR
                DrawOriginalPlane();
#endif
            }

            CheckCameraPlanePoints();
        }
    }

    private void OnApplicationQuit()
    {
        m_settings.ResetCameraSettings();

#if UNITY_EDITOR
        if (m_runtimeStatesAdded.Count > 0)
        {
            RemoveRuntimeStates();
        }
#endif

    }

    private void HandleInput()
    {
        //if (m_orbitState != null && Input.GetMouseButtonDown(0) && !IsLockedOn)
        //{
        //    SetState(EzCameraState.State.ORBIT);
        //}
        //else if (m_allowLockOn && Input.GetKeyDown(KeyCode.Space))
        //{
        //    SetState(EzCameraState.State.LOCKON);
        //}
        //else if (Input.GetKeyUp(KeyCode.Space))
        //{
        //    SetState(EzCameraState.State.FOLLOW);
        //}

        // Zoom the camera using the middle mouse button + drag
        if (Input.GetMouseButton(2))
        {
            ZoomCamera(Input.GetAxis(ExtensionMethods.MOUSEY));
        }
    }

    public void UpdatePosition()
    {
        // Update the position of the camera to reflect any rotation changes
        m_settings.OffsetDistance = Mathf.MoveTowards(m_settings.OffsetDistance, m_settings.DesiredDistance, Time.deltaTime * (m_isOccluded ? m_settings.ZoomSpeed : m_settings.ResetSpeed));
        m_relativePosition = (m_target.position + (Vector3.up * m_settings.OffsetHeight)) + (m_transform.rotation * (Vector3.forward * -m_settings.OffsetDistance)) + (m_transform.right * m_settings.LateralOffset);
        this.transform.position = m_relativePosition;
    }

    public void SmoothLookAt()
    {
        Vector3 relativePlayerPosition = m_target.position - m_transform.position + m_transform.right * m_settings.LateralOffset;

        Vector3 destDir = Vector3.ProjectOnPlane(relativePlayerPosition, m_transform.up);
        Quaternion lookAtRotation = Quaternion.LookRotation(destDir, Vector3.up);
        m_transform.rotation = Quaternion.Lerp(m_transform.rotation, lookAtRotation, m_settings.RotateSpeed * Time.deltaTime);
    }

    public void ZoomCamera(float zDelta)
    {
        // clamp the value to the min/max ranges
        if (!m_isOccluded)
        {
            float step = Time.deltaTime * m_settings.ZoomSpeed * zDelta;
            m_settings.DesiredDistance = Mathf.Clamp(m_settings.OffsetDistance + step, m_settings.MinDistance, m_settings.MaxDistance);
        }
    }

    public void SetCameraTarget(Transform target)
    {
        m_target = target;
    }

    //
    // Camera Occlusion Functions
    //

    private void UpdateNearClipPlanePoints()
    {
        Vector3 nearPlaneCenter = m_transform.position + m_transform.forward * m_nearPlaneDistance;
        m_pointBehindCamera = m_transform.position - m_transform.forward * m_nearPlaneDistance;

        float halfFOV = Mathf.Deg2Rad * (m_camera.fieldOfView / 2);
        m_aspectHalfHeight = Mathf.Tan(halfFOV) * m_nearPlaneDistance;
        m_aspectHalfWidth = m_aspectHalfHeight * m_camera.aspect;

        m_nearClipPlanePoints[0] = nearPlaneCenter + m_transform.rotation * new Vector3(-m_aspectHalfWidth, m_aspectHalfHeight);
        m_nearClipPlanePoints[1] = nearPlaneCenter + m_transform.rotation * new Vector3(m_aspectHalfWidth, m_aspectHalfHeight);
        m_nearClipPlanePoints[2] = nearPlaneCenter + m_transform.rotation * new Vector3(m_aspectHalfWidth , -m_aspectHalfHeight);
        m_nearClipPlanePoints[3] = nearPlaneCenter + m_transform.rotation * new Vector3(-m_aspectHalfWidth, -m_aspectHalfHeight);
    }

    private void UpdateOriginalClipPlanePoints()
    {
        Vector3 originalCameraPosition = (m_target.position + (Vector3.up * m_settings.OffsetHeight)) + (m_transform.rotation * (Vector3.forward * -m_settings.ResetPositionDistance)) + (m_transform.right * m_settings.LateralOffset);
        Vector3 originalPlaneCenter = originalCameraPosition + m_transform.forward * m_nearPlaneDistance;

        //Vector3 rearPlaneCenter = m_transform.position - m_transform.forward * m_nearPlaneDistance;

        float halfFOV = Mathf.Deg2Rad * (m_camera.fieldOfView / 2);
        m_aspectHalfHeight = Mathf.Tan(halfFOV) * m_nearPlaneDistance;
        m_aspectHalfWidth = m_aspectHalfHeight * m_camera.aspect;

        m_originalClipPlanePoints[0] = originalPlaneCenter + m_transform.rotation * new Vector3(-m_aspectHalfWidth, m_aspectHalfHeight);
        m_originalClipPlanePoints[1] = originalPlaneCenter + m_transform.rotation * new Vector3(m_aspectHalfWidth, m_aspectHalfHeight);
        m_originalClipPlanePoints[2] = originalPlaneCenter + m_transform.rotation * new Vector3(m_aspectHalfWidth, -m_aspectHalfHeight);
        m_originalClipPlanePoints[3] = originalPlaneCenter + m_transform.rotation * new Vector3(-m_aspectHalfWidth, -m_aspectHalfHeight);
    }

    #region Editor Only Functions
#if UNITY_EDITOR
    private void DrawNearPlane()
    {
        Debug.DrawLine(m_nearClipPlanePoints[0], m_nearClipPlanePoints[1], Color.red);
        Debug.DrawLine(m_nearClipPlanePoints[1], m_nearClipPlanePoints[2], Color.red);
        Debug.DrawLine(m_nearClipPlanePoints[2], m_nearClipPlanePoints[3], Color.red);
        Debug.DrawLine(m_nearClipPlanePoints[3], m_nearClipPlanePoints[0], Color.red);
    }

    private void DrawOriginalPlane()
    {
        Debug.DrawLine(m_originalClipPlanePoints[0], m_originalClipPlanePoints[1], Color.cyan);
        Debug.DrawLine(m_originalClipPlanePoints[1], m_originalClipPlanePoints[2], Color.cyan);
        Debug.DrawLine(m_originalClipPlanePoints[2], m_originalClipPlanePoints[3], Color.cyan);
        Debug.DrawLine(m_originalClipPlanePoints[3], m_originalClipPlanePoints[0], Color.cyan);
    }
#endif
    #endregion

    private void CheckCameraPlanePoints()
    {
#if UNITY_EDITOR
        Color lineColor = Color.black;
#endif
        RaycastHit hit;
        float hitDistance = 0;

        for (int i = 0; i < m_nearClipPlanePoints.Length; ++i)
        {

            if (Physics.Linecast(m_target.position, m_nearClipPlanePoints[i], out hit, m_layermask))
            {
                if (hit.collider.gameObject.transform.root != m_target.root)
                {
                    if (hit.distance > hitDistance)
                    {
                        hitDistance = hit.distance;

                        if (!m_isOccluded) // Only store the original position on the original hit
                        {
                            //m_settings.ResetPositionDistance = m_settings.OffsetDistance;
                            m_settings.ResetPositionDistance = m_settings.DesiredDistance;
                        }

                        m_isOccluded = true;
                        m_settings.DesiredDistance = hitDistance - m_nearPlaneDistance;

#if UNITY_EDITOR
                        lineColor = Color.red;
                        Debug.Log("camera is occluded");
#else
                        return;
#endif

                        
                    }
                }
            }

#if UNITY_EDITOR
            Debug.DrawLine(m_nearClipPlanePoints[i], m_target.position, lineColor);
#endif
        }

        if (!m_isOccluded)
        {
            if (Physics.Linecast(m_target.position, m_pointBehindCamera, out hit, m_layermask))
            {
#if UNITY_EDITOR
                lineColor = Color.red;
                Debug.Log("camera is occluded");
#endif
                m_isOccluded = true;
                m_settings.ResetPositionDistance = m_settings.OffsetDistance;
                m_settings.DesiredDistance = hit.distance + m_nearPlaneDistance;
            }
        }
        
    }

    private void CheckOriginalPlanePoints()
    {
        UpdateOriginalClipPlanePoints();

        bool objectWasHit = false;
        RaycastHit hit;
        float closestHitDistance = float.MaxValue;

        for (int i = 0; i < m_originalClipPlanePoints.Length; ++i)
        {
#if UNITY_EDITOR
            Color lineColor = Color.blue;
#endif
            if (Physics.Linecast(m_target.position, m_originalClipPlanePoints[i], out hit, m_layermask))
            {
                lineColor = Color.red;
                objectWasHit = true;

                if (hit.distance < closestHitDistance)
                {
                    closestHitDistance = hit.distance;
                }
            }

            Debug.DrawLine(m_target.position, m_originalClipPlanePoints[i], lineColor);
        }

        if (!objectWasHit)
        {
            m_settings.DesiredDistance = m_settings.ResetPositionDistance;
            m_isOccluded = false;
        }
        else
        {
            if (closestHitDistance > m_settings.DesiredDistance)
            {
                m_settings.DesiredDistance = closestHitDistance;
            }
        }
    }

    public void SetState(EzCameraState.State nextState)
    {
        switch (nextState)
        {
            case EzCameraState.State.FOLLOW:
                if (m_followState == null)
                {
                    m_followState = this.GetOrAddComponent<EzFollowState>();
                    m_followState.Init(this, m_settings);

#if UNITY_EDITOR
                    m_runtimeStatesAdded.Add(m_followState);
#endif
                }

                m_stateMachine.SetCurrentState(m_followState);
                break;
            case EzCameraState.State.ORBIT:
                if (m_orbitState == null)
                {
                    m_orbitState = this.GetOrAddComponent<EzOrbitState>();
                    m_orbitState.Init(this, m_settings);

#if UNITY_EDITOR
                    m_runtimeStatesAdded.Add(m_orbitState);
#endif
                }

                m_stateMachine.SetCurrentState(m_orbitState);
                break;
            case EzCameraState.State.LOCKON:
                if (m_lockOnState == null)
                {
                    m_lockOnState = this.GetOrAddComponent<EzLockOnState>();
                    m_lockOnState.Init(this, m_settings);

#if UNITY_EDITOR
                    m_runtimeStatesAdded.Add(m_lockOnState);
#endif
                }

                m_stateMachine.SetCurrentState(m_lockOnState);
                break;
            case EzCameraState.State.STATIONARY:
            default:
                if (m_stationaryState == null)
                {
                    m_stationaryState = this.GetOrAddComponent<EzStationaryState>();
                    m_stationaryState.Init(this, m_settings);

#if UNITY_EDITOR
                    m_runtimeStatesAdded.Add(m_stationaryState);
#endif
                }

                m_stateMachine.SetCurrentState(m_stationaryState);
                break;
        }
    }

    
    public bool IsInOrbit 
    { 
        get 
        {
            if (m_orbitState != null)
            {
                return m_stateMachine.CurrentState == m_orbitState;
            }

            return false;
        } 
    }

    public bool IsLockedOn 
    { 
        get 
        {
            if (m_lockOnState != null)
            {
                return m_stateMachine.CurrentState == m_lockOnState;
            }

            return false;
        } 
    }

#if UNITY_EDITOR
    private List<EzCameraState> m_runtimeStatesAdded = new List<EzCameraState>();

    private void RemoveRuntimeStates()
    {
        for (int i = 0; i < m_runtimeStatesAdded.Count; ++i)
        {
            DestroyImmediate(m_runtimeStatesAdded[i]);
        }
    }

#endif
}
