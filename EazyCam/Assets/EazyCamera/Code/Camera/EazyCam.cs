using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EazyCam
{
    using Util = EazyCameraUtility;

    [ExecuteInEditMode]
    public class EazyCam : MonoBehaviour
    {
        [System.Serializable]
        public struct Settings
        {
            public Vector3 Offset;

            // Movement
            public float MoveSpeed;
            [Range(0f, 1f)] public float LagFactor;

            // Rotation
            public float RotationSpeed;
            public FloatRange HorizontalRoation;
            public FloatRange VerticalRotation;
        }

        [SerializeField] private Settings _settings = new Settings()
        {
            Offset = new Vector3(0f, 3f, -5f),
            MoveSpeed = 15f,
            LagFactor = .75f,
            RotationSpeed = 30f,
            HorizontalRoation = new FloatRange(-360f, 360f),
            VerticalRotation = new FloatRange(-89f, 89f)
        };

        [SerializeField] private Transform _target = null;

        private Vector3 _rotation = new Vector3();

        public Transform CameraTransform => _transform;
        private Transform _transform = null;

        private const float DeadZone = .01f;
        private const float SnapDistanceSq = DeadZone * DeadZone;

        private void Awake()
        {
            _transform = this.transform;
        }

        private void Start()
        {
            _transform.position = GetFollowPosition();
            _rotation = transform.rotation.eulerAngles;
        }

        private void LateUpdate()
        {
            UpdateRotation();
            UpdatePosition();
        }

        private void UpdatePosition()
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

        private void UpdateRotation()
        {
            float horz = Input.GetAxis(Util.MouseX);
            float vert = Input.GetAxis(Util.MouseY);


            // cache the step and update the roation from input
            float step = Time.deltaTime * _settings.RotationSpeed;
            _rotation.y += horz * step;
            _rotation.x += vert * step;
            _rotation.x = Mathf.Clamp(_rotation.x, _settings.HorizontalRoation.Min, _settings.HorizontalRoation.Max);

            // compose the quaternions from Euler vectors to get the new rotation
            Quaternion addRot = Quaternion.Euler(0f, _rotation.y, 0f);
            Quaternion destRot = addRot * Quaternion.Euler(_rotation.x, 0f, 0f); // Not commutative

            _transform.rotation = destRot;
        }

        private Vector3 GetFollowPosition()
        {
            return _target.position + (_settings.Offset.x * _transform.right) + (_settings.Offset.y * Vector3.up) + (_transform.rotation * (_settings.Offset.z * _transform.forward));
        }
    }
}

