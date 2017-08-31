using UnityEngine;
using System.Collections;

/// <summary>
/// This is the main class for controlling both camera and player. It is recommended to attach this to the player or camera in the scene, but not necessary
/// </summary>
public class EzController : MonoBehaviour 
{
	[SerializeField] private EzCamera m_camera = null;
    [SerializeField] private EzMotor m_controlledPlayer = null;

    protected const string HORIZONTAL = "Horizontal";
    protected const string VERITCAL = "Vertical";
    protected const string MOUSEX = "Mouse X";
    protected const string MOUSEY = "Mouse Y";
    protected const string MOUSE_WHEEL = "Mouse ScrollWheel";

    private void Start()
    {
        // if either the player or camera are null, attempt to find them
        SetUpControlledPlayer();
        SetUpCamera();
    }

    private void Update()
    {
        if (m_controlledPlayer != null && m_camera != null)
        {
            HandleInput();
        }
    }

    private void SetUpControlledPlayer()
    {
        if (m_controlledPlayer == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                m_controlledPlayer = playerObj.GetComponent<EzMotor>();
            }
        }
    }

    private void SetUpCamera()
    {
        if (m_camera == null)
        {
            m_camera = Camera.main.GetComponent<EzCamera>();
            if (m_camera == null)
            {
                m_camera = Camera.main.gameObject.AddComponent<EzCamera>();
            }
        }
    }

    private void HandleInput()
    {
        // Update player movement first
        // cache the inputs
        float horz = Input.GetAxis(HORIZONTAL);
        float vert = Input.GetAxis(VERITCAL);
        
        // Convert movement to camera space
        Transform camTransform = m_camera.transform;
        float moveX = (horz * camTransform.right.x) + (vert * camTransform.forward.x);
        float moveZ = (horz * camTransform.right.z) + (vert * camTransform.forward.z);

        // Move the Player
        m_controlledPlayer.MovePlayer(moveX, moveZ, Input.GetKey(KeyCode.LeftShift));

        // Update camera rotation if necessary
        if (Input.GetMouseButtonUp(0))
        {
            m_camera.SetState(EzCameraState.State.FOLLOW);
        }
        else if (Input.GetMouseButtonDown(0) && !m_camera.IsLockedOn)
        {
            m_camera.SetState(EzCameraState.State.ORBIT);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            m_camera.SetState(EzCameraState.State.LOCKON);
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            m_camera.SetState(EzCameraState.State.FOLLOW);
        }


        if (Input.GetMouseButton(2))
        {
            vert = Input.GetAxis(MOUSEY);
            m_camera.ZoomCamera(vert);
        }
    }
}
