using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class EzLockOnTarget : MonoBehaviour
{
    // need AABB and  transform
    public GameObject TargetIcon { get { return m_targetIcon; } }
    [SerializeField] private GameObject m_targetIcon = null;

    private void Awake()
    {
        if (m_targetIcon != null)
        {
            m_targetIcon.SetActive(false);
        }
    }
}
