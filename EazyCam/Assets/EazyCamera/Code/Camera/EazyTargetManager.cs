using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EazyCamera
{
    using EazyCamera.Events;

    public interface ITargetable
    {
        Vector3 LookAtPosition { get; }
        bool IsActive { get; }
        void OnFocusReceived();
        void OnFocusLost();
    }

    public class EazyTargetManager : IEventListener
    {
        private List<ITargetable> _targetsInRange = new List<ITargetable>();
        private EazyCam _controlledCamera = null;

        private ITargetable _currentTarget = null;

        public bool IsEnabled { get; private set; }

        public EazyTargetManager(EazyCam cam, EnabledState defaultState = EnabledState.Enabled)
        {
            _controlledCamera = cam;
            SetEnabled(defaultState);
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
                Debug.Log($"{target} is in range");
            }
        }

        public void OnExitFocusRange(EventData data)
        {
            if (data is ExitFocusRangeData rangeExitData)
            {
                RemoveTargetInRange(rangeExitData.Target);
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

                Debug.Log($"{target} is in no longer range");
            }
        }

        public void BeginTargetLock()
        {
            //
        }

        public void EndTargetLock()
        {
            //
        }

        public void ToggleLockOn()
        {
            if (_targetsInRange.Count > 0)
            {
                ITargetable nearestTarget = FindNearestTarget();
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
                    _controlledCamera.CameraSettings.TargetLockIcon.SetActive(true);
                    _controlledCamera.CameraSettings.TargetLockIcon.transform.position = _currentTarget.LookAtPosition;
                }
            }
        }

        private ITargetable FindNearestTarget()
        {
            if (_targetsInRange.Count > 0)
            {
                // if two targets, toggle between them
                if (_targetsInRange.Count == 2)
                {
                    return _currentTarget == _targetsInRange[0] ? _targetsInRange[1] : _targetsInRange[0]; ;
                }

                // if more than two targets:
                // Find the target nearest to the direction we want to move 
                ITargetable nearestTarget = _currentTarget;
                ITargetable nextTarget = null;
                float currentNearestDistance = float.MaxValue;

                Transform cameraTransform = _controlledCamera.CameraTransform;

                for (int i = 0; i < _targetsInRange.Count; ++i)
                {
                    nextTarget = _targetsInRange[i];
                    if (nextTarget == _currentTarget)
                    {
                        continue;
                    }

                    Vector3 relativeDirection = nextTarget.LookAtPosition - cameraTransform.position;
                    float distance = relativeDirection.sqrMagnitude;
                    if (distance < currentNearestDistance)
                    {
                        nearestTarget = nextTarget;
                        currentNearestDistance = distance;
                    }
                }

                return nearestTarget;
            }

            return null;
        }

        public void BindEvents()
        {
            EazyEventManager.BindToEvent(EazyEventKeys.OnEnterFocasableRange, OnEnterFocusRange);
            EazyEventManager.BindToEvent(EazyEventKeys.OnExitFocasableRange, OnExitFocusRange);
        }

        public void UnbindEvents()
        {
            EazyEventManager.UnbindFromEvent(EazyEventKeys.OnEnterFocasableRange, OnEnterFocusRange);
            EazyEventManager.UnbindFromEvent(EazyEventKeys.OnExitFocasableRange, OnExitFocusRange);
        }
    }
}
