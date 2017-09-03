using UnityEngine;
using System.Collections;
using System;

public class EzFollowState : EzCameraState
{
    [SerializeField] private float m_snapAngle = 2.5f;
    private Quaternion m_defaultRotation = Quaternion.identity;

    protected override void AddStateToCamera()
    {
        EzCamera ezCam = this.GetComponent<EzCamera>();
        if (ezCam != null)
        {
            ezCam.FollowState = this;
            Init(ezCam, ezCam.Settings);
        }
    }

    public override void Init(EzCamera camera, EzCameraSettings stateCameraSettings = null)
    {
        base.Init(camera, stateCameraSettings);
        m_defaultRotation = m_cameraTransform.rotation;
    }
    //
    public override void EnterState()
    {
        //
    }

    public override void ExitState()
    {
        //
    }


    public override void LateUpdateState()
    {
        if (Quaternion.Angle(m_cameraTransform.rotation, m_defaultRotation) > m_snapAngle)
        {
            float step = m_stateSettings.RotateSpeed * Time.deltaTime;
            m_cameraTransform.rotation = Quaternion.Lerp(m_cameraTransform.rotation, m_defaultRotation, step);
        }
        else
        {
            m_cameraTransform.rotation = m_defaultRotation;
        }

        m_controlledCamera.UpdatePosition();
    }

    public override void UpdateState()
    {
        //
    }

    public override void UpdateStateFixed()
    {
        //
    }

    
    public override void HandleInput()
    {
        //
    }
}
