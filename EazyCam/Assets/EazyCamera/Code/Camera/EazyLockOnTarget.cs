using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EazyCamera
{
    using EazyCamera.Events;
    using Util = EazyCameraUtility;

    public class EazyLockOnTarget : MonoBehaviour, IEventSource, ITargetable
    {
        public Vector3 LookAtPosition => _lookTarget?.position ?? this.transform.position;
        [SerializeField] private Transform _lookTarget = null;

        public bool IsActive { get; private set; }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.root.CompareTag(Util.PlayerTag))
            {
                BroadcastEvent(EazyEventKeys.OnEnterFocasableRange, new EnterFocusRangeData(this));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.root.CompareTag(Util.PlayerTag))
            {
                BroadcastEvent(EazyEventKeys.OnExitFocasableRange, new ExitFocusRangeData(this));
            }
        }

        public void BroadcastEvent(string key, EventData data)
        {
            EazyEventManager.TriggerEvent(key, data);
        }

        public void SetActive(EnabledState state)
        {
            IsActive = state == EnabledState.Enabled;
        }
    }
}
