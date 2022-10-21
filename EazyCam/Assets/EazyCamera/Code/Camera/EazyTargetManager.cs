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
        void SetActive(EnabledState state);
    }

    public class EazyTargetManager : IEventListener
    {
        private List<ITargetable> _targetsInRange = new List<ITargetable>();
        private EazyCam _controlledCamera = null;

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
                _targetsInRange[i].SetActive(EnabledState.Disabled);
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
                    target.SetActive(EnabledState.Disabled);
                }

                int index = _targetsInRange.IndexOf(target);
                if (index != -1)
                {
                    _targetsInRange.RemoveAt(index);
                }

                Debug.Log($"{target} is in no longer range");
            }
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
