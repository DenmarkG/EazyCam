using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
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

    public bool IsInOrbit { get; set; }

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
    //private float hitDistance = 0f;

    private bool m_isLockedOn = false;
    private GameObject m_lockonObject = null;
    private Vector3 m_lockonMidpoint = Vector3.zero;


    private EzStateMachine m_stateMachine = null;
    private EzStationaryState m_stationaryState = null;
    private EzOrbitState m_orbitState = null;
    private EzFollowState m_followState = null;

    // intialize necessarry fields
    private void Awake()
    {
        m_stationaryState = new EzStationaryState(this, m_settings);
        m_orbitState = new EzOrbitState(this, m_settings);
        m_followState = new EzFollowState(this, m_settings);
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

        // Init the state machine
        m_stationaryState = new EzStationaryState(this, m_settings);
        m_orbitState = new EzOrbitState(this, m_settings);
        m_followState = new EzFollowState(this, m_settings);
        m_stateMachine = new EzStateMachine();
        SetState(EzCameraState.State.FOLLOW);
    }

    private void Update()
    {
        if (m_stateMachine != null)
        {
            m_stateMachine.UpdateState();
        }
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

            UpdatePosition();
        }
    }

    private void OnApplicationQuit()
    {
        m_settings.ResetCameraSettings();
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
                            m_settings.ResetPositionDistance = m_settings.OffsetDistance;
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
                m_stateMachine.SetCurrentState(m_followState);
                break;
            case EzCameraState.State.ORBIT:
                m_stateMachine.SetCurrentState(m_orbitState);
                break;
            case EzCameraState.State.STATIONARY:
            default:
                m_stateMachine.SetCurrentState(m_stationaryState);
                break;
        }
    }
}
