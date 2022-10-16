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

        public EazyTargetManager(EazyCam cam)
        {
            _controlledCamera = cam;
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
