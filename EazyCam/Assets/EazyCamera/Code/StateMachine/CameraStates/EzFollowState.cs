using UnityEngine;
using System.Collections;
using System;

public class EzFollowState : EzCameraState
{
    public bool SnapBehindTarget { get; set; }

    //private float m_snapAngle = 2.5f;
    private Quaternion m_defaultRotation = Quaternion.identity;

    public EzFollowState(EzCamera camera, EzCameraSettings settings)
        : base(camera, settings) 
    {
        m_defaultRotation = m_cameraTransform.rotation;
        SnapBehindTarget = true;
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
            if (SnapBehindTarget)
            {
                UpdateCameraPosition();
            }
            else
            {
                m_controlledCamera.UpdatePosition();
            }
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

    private void UpdateCameraPosition()
    {
        // Update the rotation

        // Update the position of the camera to reflect any rotation changes
        m_controlledCamera.Settings.OffsetDistance = Mathf.MoveTowards(m_controlledCamera.Settings.OffsetDistance, m_controlledCamera.Settings.DesiredDistance, Time.deltaTime * m_controlledCamera.Settings.ZoomSpeed);
        Vector3 m_relativePosition = (m_cameraTarget.position + (Vector3.up * m_controlledCamera.Settings.OffsetHeight)) + (m_cameraTransform.rotation * (-m_cameraTarget.forward * m_controlledCamera.Settings.OffsetDistance)) + (m_cameraTransform.right * m_controlledCamera.Settings.LateralOffset);
        m_cameraTransform.position = Vector3.MoveTowards(m_cameraTransform.position, m_relativePosition, m_controlledCamera.Settings.RotateSpeed * Time.deltaTime);
    }
}
