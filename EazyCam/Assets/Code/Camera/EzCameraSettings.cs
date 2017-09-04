using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "CameraSettings")]
public class EzCameraSettings : ScriptableObject 
{
    public float OffsetHeight = 1.5f;
    public float LateralOffset = 0f;
    public float MaxLateralOffset = 5f;
    public float OffsetDistance = 4.5f;
    public float MaxDistance = 15f;
    public float MinDistance = 1f;
    public float ZoomSpeed = 10f;
    public float ResetSpeed = 5f;

    public float RotateSpeed = 15f;

    public float MaxRotX = 90f;
    public float MinRotX = -90f;

    public float DesiredDistance { get; set; }
    public float ResetPositionDistance { get; set; }
    public float HitDistance { get; set; }

    private float m_defaultHeight = 0f;
    private float m_defaultLateralOffset = 0f;
    private float m_defaualtDistance = 0f;

    public void StoreDefaultValues()
    {
        m_defaultHeight = OffsetHeight;
        m_defaultLateralOffset = LateralOffset;
        m_defaualtDistance = OffsetDistance;
    }

    public void ResetCameraSettings()
    {
        OffsetHeight = m_defaultHeight;
        LateralOffset = m_defaultLateralOffset;
        OffsetDistance = m_defaualtDistance;
        DesiredDistance = m_defaualtDistance;
    }
}
