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
    public int LockOnTargetLayer { get { return m_targetObjectLayer; } }
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
            if (targetToAdd != null && !m_nearbyTargets.Contains(targetToAdd))
            {
                m_nearbyTargets.Add(targetToAdd);
            }
        }

        if (m_nearbyTargets.Count > 0)
        {
            SetInitialTarget();
        }

        Debug.Log("Added " + m_nearbyTargets.Count + " targets");
    }

    public override void UpdateState()
    {
        HandleInput();
    }

    public override void ExitState()
    {
        if (m_currentTarget != null)
        {
            m_currentTarget.SetIconActive(false);
            m_currentTarget = null;
        }

        m_nearbyTargets.Clear();
    }

    public override void LateUpdateState()
    {
        LockOnTarget();
        m_controlledCamera.UpdatePosition();
    }

    public override void UpdateStateFixed()
    {
        // Update the possible targets here
    }

    public override void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveToNextTarget(m_cameraTransform.right);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveToNextTarget(-m_cameraTransform.right);
        }
    }

    private void LockOnTarget()
    {
        if (m_currentTarget != null)
        {
            float step = Time.deltaTime * m_stateSettings.RotateSpeed;

            Vector3 relativePos = m_currentTarget.transform.position - m_cameraTransform.position;
            m_cameraTransform.rotation = Quaternion.LookRotation(relativePos);
        }
    }

    private void SetInitialTarget()
    {
        // Find the closest Target

        // for now set to the initial one in the list
        m_currentTarget = m_nearbyTargets[0];
        m_currentTarget.SetIconActive();
    }

    public void MoveToNextTarget(Vector3 direction)
    {
        // if one target early out
        if (m_nearbyTargets.Count <= 1)
        {
            return;
        }

        // if two targets, toggle between them
        if (m_nearbyTargets.Count == 2)
        {
            m_currentTarget = m_currentTarget == m_nearbyTargets[0] ? m_nearbyTargets[1] : m_nearbyTargets[0];
        }

        // if more than two targets:
        // Find the target nearest to the direction we want to move 
        EzLockOnTarget nearestTarget = m_currentTarget;
        EzLockOnTarget nextTarget = null;
        Vector3 relativeDirection = direction;
        float currentNearestDistance = float.MaxValue;
        float sqDstance = float.MaxValue;

        for (int i = 0; i < m_nearbyTargets.Count; ++i)
        {
            nextTarget = m_nearbyTargets[i];
            if (nextTarget == m_currentTarget)
            {
                continue;
            }

            relativeDirection = nextTarget.transform.position - m_cameraTransform.position;
            if (Vector3.Dot(relativeDirection, direction) > 0)
            {
                sqDstance = relativeDirection.sqrMagnitude;
                if (sqDstance < currentNearestDistance)
                {
                    nearestTarget = nextTarget;
                    currentNearestDistance = sqDstance;
                }
            }
        }

        m_currentTarget.SetIconActive(false);
        m_currentTarget = nearestTarget;
        m_currentTarget.SetIconActive(true);

        // priority goes to closest targets first
    }
}
