using UnityEngine;
using System.Collections;
using System;

public class EzOrbitState : EzCameraState
{
    private float m_rotY = 0; // Camera's current rotation around the X axis (up/down)
    private float m_rotX = 0; // Camera's current rotation around the Y axis (left/right)

    private float m_horizontalInput = 0;
    private float m_verticalInput = 0;

    private bool m_isActive = false;

    Quaternion m_destRot = Quaternion.identity;

    protected override void AddStateToCamera()
    {
        EzCamera ezCam = this.GetComponent<EzCamera>();
        if (ezCam != null)
        {
            ezCam.OrbitState = this;
            Init(ezCam, ezCam.Settings);
        }
    }

    protected override void Update()
    {
        if (m_controlledCamera.OribtEnabled)
        {
            HandleInput();
        }
    }

    public override void EnterState()
    {
        m_rotX = m_cameraTransform.rotation.eulerAngles.x;
        m_rotX = Mathf.Clamp(m_rotX, m_stateSettings.MinRotX, m_stateSettings.MaxRotX);
        m_rotY = m_cameraTransform.rotation.eulerAngles.y;
    }

    public override void ExitState()
    {
        //
    }

    public override void UpdateState()
    {
        //
    }

    public override void LateUpdateState()
    {
        OrbitCamera(m_horizontalInput, m_verticalInput);
    }

    public override void UpdateStateFixed()
    {
        //
    }

    private void OrbitCamera(float horz, float vert)
    {
        // cache the step and update the roation from input
        float step = Time.deltaTime * m_stateSettings.RotateSpeed;
        m_rotY += horz * step; 
        m_rotX += vert * step;
        m_rotX = Mathf.Clamp(m_rotX, m_stateSettings.MinRotX, m_stateSettings.MaxRotX);
        
        // compose the quaternions from Euler vectors to get the new rotation
        Quaternion addRot = Quaternion.Euler(0f, m_rotY, 0f);
        m_destRot = addRot * Quaternion.Euler(m_rotX, 0f, 0f); // Not commutative

        m_cameraTransform.rotation = m_destRot;

#if UNITY_EDITOR
        Debug.DrawLine(m_cameraTransform.position, m_cameraTarget.transform.position, Color.red);
        Debug.DrawRay(m_cameraTransform.position, m_cameraTransform.forward, Color.blue);
#endif

        m_controlledCamera.UpdatePosition();
    }

    public override void HandleInput()
    {
        if (Input.GetMouseButtonUp(0))
        {
            m_isActive = false;
            m_controlledCamera.SetState(State.FOLLOW);
            return;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            m_isActive = true;
            m_controlledCamera.SetState(State.ORBIT);
        }

        if (m_isActive)
        {
            // cache the inputs
            float horz = Input.GetAxis(MOUSEX);
            float vert = Input.GetAxis(MOUSEY);

            m_horizontalInput = horz;
            m_verticalInput = vert;
        }
    }
}
