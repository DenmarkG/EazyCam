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
            [Range(.1f, 1f)] public float SnapFactor;
            [Range(0f, 10f)] public float MaxLagDistance;

            // Rotation
            public float RotationSpeed;
            public FloatRange HorizontalRoation;
            public FloatRange VerticalRotation;
            public AnimationCurve EaseCurve;
        }

        [SerializeField] private Settings _settings = new Settings()
        {
            Offset = new Vector3(0f, 3f, -5f),
            MoveSpeed = 5f,
            SnapFactor = .75f,
            MaxLagDistance = 1f,
            RotationSpeed = 30f,
            HorizontalRoation = new FloatRange(-360f, 360f),
            VerticalRotation = new FloatRange(-89f, 89f),
            EaseCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f),
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
            if (_settings.MaxLagDistance > 0f && _settings.SnapFactor < 1f)
            {
                Vector3 travelDirection = _target.position - _focalPoint;
                float travelDistance = travelDirection.sqrMagnitude;
                float maxDistance = _settings.MaxLagDistance.Squared();

                if (travelDistance > maxDistance)
                {
                    _focalPoint = _target.position - (travelDirection.normalized * _settings.MaxLagDistance);
                }
                else if (travelDistance < DeadZone.Squared())
                {
                    _focalPoint = _target.position;
                }

                float dt = Time.deltaTime;

                float pointOnCurve = 1 - Mathf.Clamp01(travelDistance / maxDistance);
                float speed = _settings.MoveSpeed * _settings.EaseCurve.Evaluate(pointOnCurve);

                Debug.Log($"t = {pointOnCurve}; eval = {_settings.EaseCurve.Evaluate(pointOnCurve)}; speed = {speed}");

                float step = _settings.SnapFactor * dt * speed;

                _focalPoint = Vector3.MoveTowards(_focalPoint, _target.position, step);
            }
            else
            {
                _focalPoint = _target.position;
            }

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

