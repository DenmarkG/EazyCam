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
