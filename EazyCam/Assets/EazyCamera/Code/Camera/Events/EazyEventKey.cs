using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EazyCamera.Events
{
    [System.Serializable]
    public struct EazyEventKey : System.IEquatable<EazyEventKey>
    {
        public string Key { get; }

        public EazyEventKey(string key)
        {
            Key = key;
        }

        public static implicit operator string(EazyEventKey key) => key.Key;

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public bool Equals(EazyEventKey other)
        {
            return Key == other.Key;
        }

        public static readonly EazyEventKey OnEnterFocasableRange = new EazyEventKey("OnEnterFocasableRange");
        public static readonly EazyEventKey OnExitFocasableRange = new EazyEventKey("OnExitFocasableRange");
        public static readonly EazyEventKey OnMangerEnabled = new EazyEventKey("OnManagerEnabled");
        public static readonly EazyEventKey OnManagerDisabled = new EazyEventKey("OnManagerDisabled");
    }
}
