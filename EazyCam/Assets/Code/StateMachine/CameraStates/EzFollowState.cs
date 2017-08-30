using UnityEngine;
using System.Collections;
using System;

public class EzFollowState : EzCameraState
{
    public EzFollowState(EzCamera camera, EzCameraSettings stateCameraSettings = null) 
        : base(camera, stateCameraSettings)
    {
        //
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
