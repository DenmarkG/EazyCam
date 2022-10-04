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

        private Vector2 _rotation = new Vector2();

        public Transform CameraTransform => _transform;
        private Transform _transform = null;

        private const float DeadZone = .001f;

        private void Awake()
        {
            _transform = this.transform;
        }

        private void Start()
        {
            // #DG: set the rotation from the look direction of the offest

            //Vector3 initialPosition = _target.position + _settings.Offset;
            //Quaternion lookDirection = Quaternion.LookRotation(_target.position - initialPosition);
            //_rotation = lookDirection.eulerAngles;

            //_transform.SetPositionAndRotation(initialPosition, lookDirection);

            _focalPoint = _target.position;

            Debug.Log($"OffsetMagnitude = {_settings.Offset.magnitude}");

            if (Application.isPlaying)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void LateUpdate()
        {
            UpdatePosition();
            UpdateRotation();

            Quaternion rotation = CalculateRotationFromVector(_rotation);
            Vector3 position = rotation * (_focalPoint + _settings.Offset);
            //Vector3 position = _focalPoint + (((Vector3.up * _settings.Offset.y)) + ((rotation * Vector3.forward) * _settings.Offset.z) + (_transform.right * _settings.Offset.x));

            _transform.SetPositionAndRotation(position, Quaternion.LookRotation(_target.position - position));

            DebugDrawFocualCross(_focalPoint);

            Debug.DrawLine(_focalPoint, position, Color.black);
            Debug.Log($"Distance = {(_target.position - _transform.position).magnitude}");
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

                float step = _settings.SnapFactor * dt * speed;

                _focalPoint = Vector3.MoveTowards(_focalPoint, _target.position, step);
            }
            else
            {
                _focalPoint = _target.position;
            }
        }

        private void UpdateRotation()
        {
            float horz = Input.GetAxis(Util.MouseX);
            float vert = Input.GetAxis(Util.MouseY);

            // cache the step and update the roation from input
            float step = Time.deltaTime * _settings.RotationSpeed;
            _rotation.y += horz * step;
            _rotation.y = Mathf.Clamp(_rotation.y, _settings.HorizontalRoation.Min, _settings.HorizontalRoation.Max);

            _rotation.x += vert * step;
            _rotation.x = Mathf.Clamp(_rotation.x, _settings.VerticalRotation.Min, _settings.VerticalRotation.Max);
        }

        private Quaternion CalculateRotationFromVector(Vector3 rotation)
        {
            Quaternion addRot = Quaternion.Euler(0f, rotation.y, 0f);
            return addRot * Quaternion.Euler(rotation.x, 0f, 0f); // Not commutative
        }

        private void DebugDrawFocualCross(Vector3 position)
        {
            Debug.DrawLine(position - (Vector3.up / 2f), position + (Vector3.up / 2f), Color.green);
            Debug.DrawLine(position - (Vector3.right / 2f), position + (Vector3.right / 2f), Color.red);
            Debug.DrawLine(position - (Vector3.forward / 2f), position + (Vector3.forward / 2f), Color.blue);
        }
    }
}

