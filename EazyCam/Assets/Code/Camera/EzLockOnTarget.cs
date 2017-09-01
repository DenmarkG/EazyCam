using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class EzLockOnTarget : MonoBehaviour
{
    // need AABB and  transform
    //public GameObject TargetIcon { get { return m_targetIcon; } }
    [SerializeField] private GameObject m_targetIcon = null;
    [SerializeField] private Color32 m_inactiveColor = new Color32(127,  127, 127, 127);
    [SerializeField] private Color32 m_activeColor = new Color32(255, 0,  0, 255);


    private SphereCollider m_collider = null;

    private void Awake()
    {
        SetIconActive(false);
        m_collider = this.GetComponent<SphereCollider>();
        m_collider.isTrigger = true;
    }

    private void Start()
    {
        // Set the physics layer at runtime
    }

    public void SetIconActive(bool isActive = true)
    {
        if (m_targetIcon != null)
        {
            //m_targetIcon.SetActive(isActive);
            m_targetIcon.GetComponent<Renderer>().material.color = (isActive) ? m_activeColor : m_inactiveColor;
        }
    }
}
