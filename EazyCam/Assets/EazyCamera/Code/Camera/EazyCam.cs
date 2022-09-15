using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EazyCam
{
    [ExecuteInEditMode]
    public class EazyCam : MonoBehaviour
    {
        [System.Serializable]
        public struct Settings
        {
            public Vector3 Offset;
        }

        [SerializeField] private Settings _settings = new Settings() { Offset = new Vector3(0f, 3f, -5f) };
        [SerializeField] private Transform _target = null;

        public Transform CameraTransform => _transform;
        private Transform _transform = null;



        private void Awake()
        {
            _transform = this.transform;
        }

        private void LateUpdate()
        {
            _transform.position = GetFollowPosition();
        }

        private Vector3 GetFollowPosition()
        {
            return _target.position + (_settings.Offset.x * _transform.right) + (_settings.Offset.y * Vector3.up) + (_transform.rotation * (_settings.Offset.z * _transform.forward));
        }
    }
}

