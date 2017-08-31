using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Add a sphere trigger collider to this, 
 * as objects enter and leave, update the list of nearby targettable objects
 * 
 */


public class EzLockOnState : EzCameraState
{
    [SerializeField] private float m_maxTargetDistance = 8f;

    private bool m_isLockedOn = false;
    private EzLockOnTarget m_currentTarget = null;
    private Vector3 m_lockonMidpoint = Vector3.zero;

    private List<EzLockOnTarget> m_nearbyTargets = null;

    public int LayerMask { get { return m_layermask; } }
    private int m_layermask = 0;
    [SerializeField] private int m_targetObjectLayer = 9;

    public EzLockOnState(EzCamera camera, EzCameraSettings stateCameraSettings = null) 
        : base(camera, stateCameraSettings)
    {
        m_nearbyTargets = new List<EzLockOnTarget>();
    }

    public override void EnterState()
    {
        Debug.Log("entered lock on mode");
        m_layermask = 1 << m_targetObjectLayer;
        //m_layermask = ~m_layermask;

        // Do a sphere check to get the nearby targettables 
        Collider[] nearbyObjects = Physics.OverlapSphere(m_controlledCamera.transform.position, m_maxTargetDistance/*, m_layermask*/);
        EzLockOnTarget targetToAdd = null;
        for (int i = 0; i < nearbyObjects.Length; ++i)
        {
            targetToAdd = nearbyObjects[i].gameObject.GetComponent<EzLockOnTarget>();
            if (targetToAdd != null)
            {
                targetToAdd.TargetIcon.SetActive(true);
                m_nearbyTargets.Add(targetToAdd);
            }
        }

        Debug.Log("Added " + m_nearbyTargets.Count + " targets");
    }

    public override void ExitState()
    {
        Debug.Log("entered lock on mode");
    }

    public override void HandleInput()
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
}
