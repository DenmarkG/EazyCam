using UnityEngine;
using System.Collections;
using System;

public class EzFollowState : EzCameraState
{
    [SerializeField] private float m_snapAngle = 2.5f;
    [SerializeField] private bool m_snapBehindPlayer = true;
    private Quaternion m_defaultRotation = Quaternion.identity;

    public EzFollowState(EzCamera camera, EzCameraSettings settings)
        : base(camera, settings) 
    {
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
        if (m_controlledCamera.FollowEnabled)
        {
            if (m_snapBehindPlayer)
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

            }
            m_controlledCamera.UpdatePosition();
        }
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
