using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EazyCamera
{
    public interface ITargetable
    {
        Vector3 LookAtPosition { get; }
        bool IsActive { get; }
        void SetActive(EnabledState state);
    }

    public class EazyTargetManager
    {
        private List<ITargetable> _targetsInRange = new List<ITargetable>();
        private EazyCam _controlledCamera = null;

        public bool IsEnabled { get; private set; } = true;

        public EazyTargetManager(EazyCam cam)
        {
            _controlledCamera = cam;
        }

        public void SetEnabled(EnabledState state)
        {
            IsEnabled = state == EnabledState.Enabled;
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

        public void AddTargetInRange(ITargetable target)
        {
            if (target != null)
            {
                _targetsInRange.Add(target);
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
            }
        }
    }
}
