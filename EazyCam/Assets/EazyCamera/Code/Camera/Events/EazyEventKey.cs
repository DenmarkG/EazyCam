using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EazyCamera.Events
{
    [System.Serializable]
    public class EazyEventKey
    {
        public string Key { get; }

#if UNITY_EDITOR
        private static readonly HashSet<string> _allKeys = new HashSet<string>();
#endif // UNITY_EDITOR

        public EazyEventKey(string key)
        {
            Key = key;

#if UNITY_EDITOR
            _allKeys.Add(key);
#endif // UNITY_EDITOR
        }


        public static HashSet<string> GetAll()
        {
            return _allKeys;
        }

        public static readonly EazyEventKey OnEnterFocasableRange = new EazyEventKey("OnEnterFocasableRange");
        public static readonly EazyEventKey OnExitFocasableRange = new EazyEventKey("OnExitFocasableRange");

        public static implicit operator string(EazyEventKey key) => key.Key;
    }
}
