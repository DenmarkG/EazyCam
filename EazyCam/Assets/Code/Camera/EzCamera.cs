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
    private Transform m_transform = null;

    private Vector3 m_relativePosition = Vector3.zero;

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
    [SerializeField] private bool m_allowOrbit = false;
    public bool AllowOrbit { get { return m_allowOrbit; } }
    public void SetAllowOrbit(bool allowOrbit)
    {
        m_allowOrbit = allowOrbit;
        if (m_allowOrbit)
        {
            if (m_orbitState == null)
            {
                m_orbitState = this.GetOrAddComponent<EzOrbitState>();
                m_orbitState.Init(this, m_settings);
            }
        }
    }

    // State for tracking a target object's position around the environment
    private EzFollowState m_followState = null;
    public EzFollowState FollowState
    {
        get { return m_followState; }
        set { m_followState = value; }
    }

    [SerializeField] private bool m_allowFollow = false;
    public bool AllowFollow { get { return m_allowFollow; } }
    public void SetAllowFollow(bool allowFollow)
    {
        m_allowFollow = allowFollow;
        if (m_allowFollow)
        {
            if (m_followState == null)
            {
                m_followState = this.GetOrAddComponent<EzFollowState>();
                m_followState.Init(this, m_settings);
            }
        }
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
            }
        }
    }

    [SerializeField] private bool m_allowZoom = true;
    private float m_zoomDelta = 0f;
    private const float ZOOM_DEAD_ZONE = .01f;


    [SerializeField] private bool m_checkForCollisions = true;
    private EzCameraCollider m_cameraCollilder = null;

    private void Start()
    {
        m_transform = this.transform;

        // reset the offset distance be 1/3 of the distance from the min to max
        m_settings.OffsetDistance = (m_settings.MaxDistance - m_settings.MinDistance) / 3f;
        m_settings.DesiredDistance = m_settings.OffsetDistance;
        m_settings.StoreDefaultValues();

        m_relativePosition = (m_target.position + (Vector3.up * m_settings.OffsetHeight)) + (m_transform.rotation * (Vector3.forward * -m_settings.OffsetDistance)) + (m_transform.right * m_settings.LateralOffset);
        m_transform.position = m_relativePosition;

        if (m_allowLockOn)
        {
            m_lockOnState = this.GetOrAddComponent<EzLockOnState>();
            m_lockOnState.Init(this, m_settings);
        }

        if (m_allowFollow && m_defaultState != EzCameraState.State.FOLLOW)
        {
            m_followState = this.GetOrAddComponent<EzFollowState>();
            m_followState.Init(this, m_settings);
        }

        if (m_allowOrbit && m_defaultState != EzCameraState.State.ORBIT)
        {
            m_orbitState = this.GetOrAddComponent<EzOrbitState>();
            m_orbitState.Init(this, m_settings);
        }

        if (m_checkForCollisions)
        {
            m_cameraCollilder = this.GetOrAddComponent<EzCameraCollider>();
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

        if (m_defaultState == EzCameraState.State.LOCKON)
        {
            Debug.LogWarning("Camera cannot start in a locked on stated. Please change the default value in the inspector. Switching to Stationary State");
            SetState(EzCameraState.State.STATIONARY);
        }
        else
        {
            SetState(m_defaultState);
        }
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
                if (m_allowZoom && Mathf.Abs(m_zoomDelta) > ZOOM_DEAD_ZONE)
                {
                    ZoomCamera(m_zoomDelta);
                }
                m_stateMachine.LateUpdateState();
            }
        }
    }

    private void OnApplicationQuit()
    {
        m_settings.ResetCameraSettings();
    }

    private void HandleInput()
    {
        // Zoom the camera using the middle mouse button + drag
        if (Input.GetMouseButton(2))
        {
            m_zoomDelta = Input.GetAxis(ExtensionMethods.MOUSEY);
        }
        else
        {
            m_zoomDelta = 0;
        }
    }

    public void UpdatePosition()
    {
        // Update the position of the camera to reflect any rotation changes
        float moveSpeed = 0f;
        if (m_checkForCollisions)
        {
            moveSpeed = (m_cameraCollilder.IsOccluded ? m_settings.ZoomSpeed : m_settings.ResetSpeed);
        }
        else
        {
            moveSpeed = m_settings.ZoomSpeed;
        }
        m_settings.OffsetDistance = Mathf.MoveTowards(m_settings.OffsetDistance, m_settings.DesiredDistance, Time.deltaTime * moveSpeed);
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
        if (!m_checkForCollisions || (m_checkForCollisions && !m_cameraCollilder.IsOccluded))
        {
            float step = Time.deltaTime * m_settings.ZoomSpeed * zDelta;
            m_settings.DesiredDistance = Mathf.Clamp(m_settings.OffsetDistance + step, m_settings.MinDistance, m_settings.MaxDistance);
        }
    }

    public void SetCameraTarget(Transform target)
    {
        m_target = target;
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
                }
                
                m_allowFollow = true;
                m_stateMachine.SetCurrentState(m_followState);
                break;
            case EzCameraState.State.ORBIT:
                if (m_orbitState == null)
                {
                    m_orbitState = this.GetOrAddComponent<EzOrbitState>();
                    m_orbitState.Init(this, m_settings);
                }

                m_allowOrbit = true;
                m_stateMachine.SetCurrentState(m_orbitState);
                break;
            case EzCameraState.State.LOCKON:
                if (m_lockOnState == null)
                {
                    m_lockOnState = this.GetOrAddComponent<EzLockOnState>();
                    m_lockOnState.Init(this, m_settings);
                }

                m_allowLockOn = true;
                m_stateMachine.SetCurrentState(m_lockOnState);
                break;
            case EzCameraState.State.STATIONARY:
            default:
                if (m_stationaryState == null)
                {
                    m_stationaryState = this.GetOrAddComponent<EzStationaryState>();
                    m_stationaryState.Init(this, m_settings);
                }

                m_stateMachine.SetCurrentState(m_stationaryState);
                break;
        }
    }

    
    public bool IsOccluded
    {
        get
        {
            if (!m_checkForCollisions)
            {
                return true;
            }

            return m_cameraCollilder.IsOccluded;
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
}
