using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EazyCamera
{
    using EazyCamera.Events;

    using TargetInfo = System.Tuple<ITargetable, int>;

    public interface ITargetable
    {
        Vector3 LookAtPosition { get; }
        Color TargetColor { get; }
        bool IsActive { get; }
        void OnFocusReceived();
        void OnFocusLost();
    }

    public class EazyTargetManager : IEventListener
    {
        private List<ITargetable> _targetsInRange = new List<ITargetable>();
        private EazyCam _controlledCamera = null;

        private ITargetable _currentTarget = null;
        private int _currentTargetIndex = -1;
        
        public bool IsEnabled { get; private set; } // Is the manager enabled and listening for events
        public bool IsActive { get; private set; } // Is the manager actively locked onto a target

        public EazyTargetManager(EazyCam cam, EnabledState defaultState = EnabledState.Enabled)
        {
            _controlledCamera = cam;
            SetEnabled(defaultState);
        }

        public void Tick(float dt)
        {
            if (IsActive)
            {
                // Not cached here to allow changing the reticle while targeting is active
                EazyTargetReticle reticle = _controlledCamera.CameraSettings.TargetLockIcon;
                if (reticle != null && _currentTarget != null)
                {
                    reticle.transform.position = _currentTarget.LookAtPosition;
                }
            }
        }

        public void SetEnabled(EnabledState state)
        {
            IsEnabled = state == EnabledState.Enabled;
            if (IsEnabled)
            {
                BindEvents();
            }
            else
            {
                UnbindEvents();
            }
        }

        public void ClearTargetsInRange()
        {
            int numTargets = _targetsInRange.Count;

            for (int i = 0; i < numTargets; ++i)
            {
                _targetsInRange[i].OnFocusReceived();
            }

            _targetsInRange.Clear();
        }

        public void OnEnterFocusRange(EventData data)
        {
            if (data is EnterFocusRangeData rangeEntryData)
            {
                AddTargetInRange(rangeEntryData.Target);
            }
        }

        public void AddTargetInRange(ITargetable target)
        {
            if (target != null)
            {
                _targetsInRange.Add(target);
            }
        }

        public void OnExitFocusRange(EventData data)
        {
            if (data is ExitFocusRangeData rangeExitData)
            {
                RemoveTargetInRange(rangeExitData.Target);

                if (rangeExitData.Target == _currentTarget)
                {
                    EndTargetLock();
                }
            }
        }

        public void RemoveTargetInRange(ITargetable target)
        {
            if (target != null)
            {
                if (target.IsActive)
                {
                    target.OnFocusLost();
                }

                int index = _targetsInRange.IndexOf(target);
                if (index != -1)
                {
                    _targetsInRange.RemoveAt(index);
                }
            }
        }

        public void BeginTargetLock()
        {
            IsActive = true;

            if (_targetsInRange.Count > 0)
            {
                TargetInfo info = FindNearestTarget();
                ITargetable nearestTarget = info.Item1;
                _currentTargetIndex = info.Item2;

                if (_currentTarget != null)
                {
                    if (_currentTarget != nearestTarget)
                    {
                        _currentTarget.OnFocusLost();
                    }
                }

                // #DG: prevent re-enabling target when these are the same
                _currentTarget = nearestTarget;

                if (_currentTarget != null)
                {
                    _currentTarget.OnFocusReceived();
                    EnableLockIcon();

                    _controlledCamera.SetLookTargetOverride(_currentTarget);
                    Debug.Log("Target locked");
                }
            }
        }

        public void EndTargetLock()
        {
            if (_currentTarget != null)
            {
                _currentTarget.OnFocusLost();
            }

            _controlledCamera.ClearLookTargetOverride();
            DisableLockIcon();

            IsActive = false;
        }

        public void ToggleLockOn()
        {
            if (IsActive)
            {
                EndTargetLock();
            }
            else
            {
                BeginTargetLock();
            }
        }

        private TargetInfo FindNearestTarget()
        {
            if (_targetsInRange.Count > 0)
            {
                if (_targetsInRange.Count == 1)
                {
                    return new TargetInfo(_targetsInRange[0], 0);
                }

                // if two targets, toggle between them
                if (_targetsInRange.Count == 2)
                {
                    if (_currentTarget == _targetsInRange[0])
                    {
                        return new TargetInfo(_targetsInRange[1], 1);
                    }
                    else
                    {
                        return new TargetInfo(_targetsInRange[0], 0);
                    }
                }

                // if more than two targets:
                // Find the target nearest to the direction we want to move 
                ITargetable nearestTarget = _currentTarget;
                ITargetable nextTarget = null;
                float currentNearestDistance = float.MaxValue;

                int nearestIndex = 0;

                for (int i = 0; i < _targetsInRange.Count; ++i)
                {
                    nextTarget = _targetsInRange[i];
                    if (nextTarget == _currentTarget)
                    {
                        continue;
                    }

                    Vector3 relativeDirection = nextTarget.LookAtPosition - _controlledCamera.FocalPoint;
                    float distance = relativeDirection.sqrMagnitude;
                    if (distance < currentNearestDistance)
                    {
                        nearestTarget = nextTarget;
                        currentNearestDistance = distance;
                        nearestIndex = i;
                    }
                }

                return new TargetInfo(nearestTarget, nearestIndex);
            }

            return null;
        }

        public void CycleTargets()
        {
            if (IsActive)
            {
                int numTargets = _targetsInRange.Count;
                if (_targetsInRange.Count <= 1)
                {
                    return;
                }

                _currentTargetIndex = (numTargets + (_currentTargetIndex + 1)) % numTargets;
                SetCurrentTarget(_targetsInRange[_currentTargetIndex], _currentTargetIndex);
            }
        }

        public void CycleTargets(Vector3 direction)
        {
            if (!IsActive)
            {
                return;
            }

            // if one target early out
            if (_targetsInRange.Count <= 1)
            {
                return;
            }

            ITargetable nearestTarget = null;
            int nearestTargetIndex = -1;

            // if two targets, toggle between them
            if (_targetsInRange.Count == 2)
            {
                if (_currentTarget == _targetsInRange[0])
                {
                    nearestTarget = _targetsInRange[1];
                    nearestTargetIndex = 1;
                }
                else
                {
                    _currentTarget = _targetsInRange[0];
                    nearestTargetIndex = 0;
                }
            }
            else
            {
                // if more than two targets:
                // Find the target nearest to the direction we want to move 
                nearestTarget = _currentTarget;
                ITargetable nextTarget = null;
                Vector3 relativeDirection = direction;
                float currentNearestDistance = float.MaxValue;
                float sqDstance = float.MaxValue;

                for (int i = 0; i < _targetsInRange.Count; ++i)
                {
                    nextTarget = _targetsInRange[i];
                    if (nextTarget == _currentTarget)
                    {
                        continue;
                    }

                    relativeDirection = nextTarget.LookAtPosition - _controlledCamera.CameraTransform.position;
                    if (Vector3.Dot(relativeDirection, direction) > 0)
                    {
                        //sqDstance = relativeDirection.sqrMagnitude;
                        sqDstance = (_currentTarget.LookAtPosition - nextTarget.LookAtPosition).sqrMagnitude;
                        if (sqDstance < currentNearestDistance)
                        {
                            nearestTarget = nextTarget;
                            currentNearestDistance = sqDstance;
                            nearestTargetIndex = i;
                        }
                    }
                }
            }

            SetCurrentTarget(nearestTarget, nearestTargetIndex);
        }

        private void EnableLockIcon()
        {
            EazyTargetReticle reticle = _controlledCamera.CameraSettings.TargetLockIcon;
            if (reticle != null)
            {
                reticle.Enable(_controlledCamera, _currentTarget.TargetColor);
                reticle.transform.position = _currentTarget.LookAtPosition;
            }
        }

        private void DisableLockIcon()
        {
            EazyTargetReticle reticle = _controlledCamera.CameraSettings.TargetLockIcon;
            if (reticle != null)
            {
                _controlledCamera.CameraSettings.TargetLockIcon.Disable();
            }
        }

        public void BindEvents()
        {
            EazyEventManager.BindToEvent(EazyEventKey.OnEnterFocasableRange, OnEnterFocusRange);
            EazyEventManager.BindToEvent(EazyEventKey.OnExitFocasableRange, OnExitFocusRange);
        }

        public void UnbindEvents()
        {
            EazyEventManager.UnbindFromEvent(EazyEventKey.OnEnterFocasableRange, OnEnterFocusRange);
            EazyEventManager.UnbindFromEvent(EazyEventKey.OnExitFocasableRange, OnExitFocusRange);
        }

        private void SetCurrentTarget(ITargetable target, int index)
        {
            if (_currentTarget != null)
            {
                _currentTarget.OnFocusLost();
            }

            _currentTarget = target;
            _currentTargetIndex = index;

            if (_currentTarget != null)
            {
                _currentTarget.OnFocusReceived();
                _controlledCamera.SetLookTargetOverride(_currentTarget);
            }
        }
    }
}
