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
            public float MoveSpeed;
            [Range(0f, 1f)] public float LagFactor;
        }

        [SerializeField] private Settings _settings = new Settings() { Offset = new Vector3(0f, 3f, -5f) };
        [SerializeField] private Transform _target = null;

        public Transform CameraTransform => _transform;
        private Transform _transform = null;

        private const float DeadZone = .01f;
        private const float SnapDistanceSq = DeadZone * DeadZone;

        private void Awake()
        {
            _transform = this.transform;
        }

        private void LateUpdate()
        {
            Vector3 followPos = GetFollowPosition();
            Vector3 travelDirection = followPos - _transform.position;
            float travelDistance = travelDirection.sqrMagnitude;

            float dt = Time.deltaTime;
            float step = dt * (1 - _settings.LagFactor) * _settings.MoveSpeed;

            if (travelDistance > SnapDistanceSq)
            {
                followPos = Vector3.Lerp(_transform.position, followPos, step);
            }

            _transform.position = followPos;
        }

        private Vector3 GetFollowPosition()
        {
            return _target.position + (_settings.Offset.x * _transform.right) + (_settings.Offset.y * Vector3.up) + (_transform.rotation * (_settings.Offset.z * _transform.forward));
        }
    }
}

