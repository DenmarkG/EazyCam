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
            [Range(0f, 1000f)] public float MaxLagDistance;

            // Rotation
            public float RotationSpeed;
            public FloatRange HorizontalRoation;
            public FloatRange VerticalRotation;
        }

        [SerializeField] private Settings _settings = new Settings()
        {
            Offset = new Vector3(0f, 3f, -5f),
            MoveSpeed = 5f,
            LagFactor = .75f,
            MaxLagDistance = 1f,
            RotationSpeed = 30f,
            HorizontalRoation = new FloatRange(-360f, 360f),
            VerticalRotation = new FloatRange(-89f, 89f)
        };

        [SerializeField] private Transform _target = null;
        private Vector3 _focalPoint = new Vector3();

        private Vector3 _rotation = new Vector3();

        public Transform CameraTransform => _transform;
        private Transform _transform = null;

        private const float DeadZone = .001f;

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
            //UpdateRotation();
            UpdatePosition();

            DebugDrawFocualCross(_focalPoint);
        }

        private void UpdatePosition()
        {
            Vector3 travelDirection = _target.position - _focalPoint;
            float travelDistance = travelDirection.sqrMagnitude;
            float maxDistance = _settings.MaxLagDistance.Squared();

            float dt = Time.deltaTime;

            float step = 0f;
            if (travelDistance > maxDistance)
            {
                travelDistance = Mathf.Min(travelDistance, maxDistance);
                travelDistance -= dt;

                travelDistance = (maxDistance - travelDistance) / maxDistance;
            }

            if (travelDistance > DeadZone)
            {
                step = (1 - travelDistance) * dt * _settings.MoveSpeed;
                Debug.Log($"step = {step}; distance = {travelDistance}");
            }

            _focalPoint = Vector3.MoveTowards(_focalPoint, _target.position, step);
            _transform.position = _focalPoint + _settings.Offset;
        }

        private void UpdateRotation()
        {
            float horz = Input.GetAxis(Util.MouseX);
            float vert = Input.GetAxis(Util.MouseY);

            // cache the step and update the roation from input
            float step = Time.deltaTime * _settings.RotationSpeed;
            _rotation.y += horz * step;
            _rotation.y = Mathf.Clamp(_rotation.y, _settings.VerticalRotation.Min, _settings.VerticalRotation.Max);

            _rotation.x += vert * step;
            _rotation.x = Mathf.Clamp(_rotation.x, _settings.HorizontalRoation.Min, _settings.HorizontalRoation.Max);

            // compose the quaternions from Euler vectors to get the new rotation
            //Quaternion addRot = Quaternion.Euler(0f, _rotation.y, 0f);
            //Quaternion destRot = addRot * Quaternion.Euler(_rotation.x, 0f, 0f); // Not commutative

            //_transform.rotation = Quaternion.Euler(_rotation);
        }

        private Vector3 GetFollowPosition()
        {
            return _target.position + (_settings.Offset.x * _transform.right) + (_settings.Offset.y * Vector3.up) + (_transform.rotation * (_settings.Offset.z * _transform.forward));
        }

        private void DebugDrawFocualCross(Vector3 position)
        {
            Debug.DrawLine(position - (Vector3.up / 2f), position + (Vector3.up / 2f), Color.green);
            Debug.DrawLine(position - (Vector3.right / 2f), position + (Vector3.right / 2f), Color.red);
            Debug.DrawLine(position - (Vector3.forward / 2f), position + (Vector3.forward / 2f), Color.blue);
        }
    }
}

