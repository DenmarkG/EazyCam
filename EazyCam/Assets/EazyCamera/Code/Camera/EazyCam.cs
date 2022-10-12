using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EazyCam
{
    using Util = EazyCameraUtility;
    
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class EazyCam : MonoBehaviour
    {
        [System.Serializable]
        public struct Settings
        {
            public float Distance;
            public float DefaultDistance;

            // Movement
            public float MoveSpeed;
            [Range(.1f, 1f)] public float SnapFactor;
            [Range(0f, 10f)] public float MaxLagDistance;

            // Rotation
            public float RotationSpeed;
            public FloatRange VerticalRotation;
            public AnimationCurve EaseCurve;

            // Optional Components
            public bool EnableCollision;

            public bool EnableZoom;
            public float ZoomDistance;
            public FloatRange ZoomRange;
        }

        public Settings CameraSettings => _settings;
        [SerializeField] private Settings _settings = new Settings()
        {
            Distance = -5f,
            DefaultDistance = -5f,
            MoveSpeed = 5f,
            SnapFactor = .75f,
            MaxLagDistance = 1f,
            RotationSpeed = 30f,
            VerticalRotation = new FloatRange(-89f, 89f),
            EaseCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f),
            EnableCollision = true,
            EnableZoom = true,
            ZoomDistance = -5,
            ZoomRange = new FloatRange(-1f, -10f),
        };

        [SerializeField] private Transform _target = null;
        public Transform TargetRoot { get; private set; }

        public Vector3 FocalPoint => _focalPoint;
        private Vector3 _focalPoint = new Vector3();

        public Camera AttachedCamera { get; private set; }
        
        public Transform CameraTransform => _transform;
        private Transform _transform = null;

        private Vector2 _rotation = new Vector2();

        private EazyCollider _collider = null;

        private const float DeadZone = .001f;

        private void Awake()
        {
            _transform = this.transform;
            
            Debug.Assert(_target != null, "Target should not be null on an EazyCam component");
            TargetRoot = _target.root;

            AttachedCamera = this.GetComponent<Camera>();



            if (_settings.EnableCollision)
            {
                _collider = new EazyCollider(this);
            }
        }

        private void Start()
        {
            Vector3 initialPosition = _target.position + (_target.forward * _settings.Distance);
            Quaternion lookDirection = Quaternion.LookRotation(_target.position - initialPosition);
            _rotation = lookDirection.eulerAngles;

            _transform.SetPositionAndRotation(initialPosition, lookDirection);

            _focalPoint = _target.position;

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
            Vector3 position = _focalPoint + ((rotation * Vector3.forward) * _settings.Distance);

            _transform.SetPositionAndRotation(position, Quaternion.LookRotation(_target.position - position));

            DebugDrawFocualCross(_focalPoint);

            if (_collider != null)
            {
                _collider.Tick();
            }

            Debug.DrawLine(_focalPoint, position, Color.black);
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
                    _focalPoint = Vector3.MoveTowards(_focalPoint, _target.position - (travelDirection.normalized * _settings.MaxLagDistance), _settings.MoveSpeed * Time.deltaTime);
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

            if (_rotation.y > 360f)
            {
                _rotation.y -= 360f;
            }
            else if (_rotation.y < -360f)
            {
                _rotation.y += 360f;
            }

            _rotation.x += vert * step;
            _rotation.x = Mathf.Clamp(_rotation.x, _settings.VerticalRotation.Min, _settings.VerticalRotation.Max);
        }

        public Vector3 GetDefaultPosition()
        {
            return _focalPoint + ((CalculateRotationFromVector(_rotation) * Vector3.forward) * _settings.DefaultDistance);
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

        public void SetDistance(float distance)
        {
            _settings.Distance = distance;
        }

        public void SetZoomDistance(float zoomDistance)
        {
            _settings.ZoomDistance = Util.Clamp(zoomDistance, _settings.ZoomRange);
        }

        public float GetDistance()
        {
            return _settings.Distance;
        }

        public void ResetDistance()
        {
            _settings.Distance = _settings.DefaultDistance;
        }
    }
}

