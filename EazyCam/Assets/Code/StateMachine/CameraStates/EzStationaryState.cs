using UnityEngine;
using System.Collections;
using System;

public class EzStationaryState : EzCameraState
{
    protected override void AddStateToCamera()
    {
        EzCamera ezCam = this.GetComponent<EzCamera>();
        if (ezCam != null)
        {
            ezCam.StationaryState = this;
            Init(ezCam, ezCam.Settings);
        }
    }

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
        m_controlledCamera.SmoothLookAt();

        //m_relativePosition = (m_target.position + (Vector3.up * m_settings.OffsetHeight)) + (m_transform.rotation * (Vector3.forward * -m_settings.OffsetDistance)) + (m_transform.right * m_settings.LateralOffset);
        //m_cameraTransform.rotation = Quaternion.LookRotation(m_cameraTarget.transform.position - m_cameraTransform.position);
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
