using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EazyCamera
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

            [Header("Movement")]
            public float MoveSpeed;
            [Range(.1f, 1f)] public float SnapFactor;
            [Range(0f, 10f)] public float MaxLagDistance;

            [Header("Rotation")]
            public float RotationSpeed;
            public FloatRange VerticalRotation;
            public AnimationCurve EaseCurve;

            public bool InvertY;

            [Header("Optional Components")]
            // Collision
            public bool EnableCollision;

            // Zoom
            public bool EnableZoom;
            public float ZoomDistance;
            public FloatRange ZoomRange;

            // Targeting
            public bool EnableTargetLock;
            public GameObject TargetImage;
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
            ZoomRange = new FloatRange(-10f, -1f),
            EnableTargetLock = true,
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
        private EazyTargetManager _targetManager = null;

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

            if (_settings.EnableTargetLock)
            {
                _targetManager = new EazyTargetManager(this);
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
                else if (travelDistance < Constants.DeadZone.Squared())
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

        public void SetRotation(float horzRot, float vertRot)
        {
            _rotation.x = vertRot;
            _rotation.y = horzRot;

            ClampVerticalRotation();
            ClampHorizontalRotation();
        }

        public void IncreaseRotation(float horzRotDelta, float vertRotDelta, float deltaTime)
        {
            float step = deltaTime * _settings.RotationSpeed;
            _rotation.y += horzRotDelta * step;
            ClampVerticalRotation();


            _rotation.x += vertRotDelta * step * (_settings.InvertY ? 1f : -1f);
            ClampHorizontalRotation();
        }

        private void ClampVerticalRotation()
        {
            if (_rotation.y > 360f)
            {
                _rotation.y -= 360f;
            }
            else if (_rotation.y < -360f)
            {
                _rotation.y += 360f;
            }
        }

        private void ClampHorizontalRotation()
        {
            _rotation.x = Mathf.Clamp(_rotation.x, _settings.VerticalRotation.Min, _settings.VerticalRotation.Max);
        }

        public Vector3 GetDefaultPosition()
        {
            float distance = _settings.EnableZoom ? _settings.ZoomDistance : _settings.Distance;
            return _focalPoint + ((CalculateRotationFromVector(_rotation) * Vector3.forward) * distance);
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
            if (_settings.EnableZoom)
            {
                // #DG: account for camera in front (take abs of both values and pick the smaller)
                _settings.Distance = Mathf.Max(distance, _settings.ZoomDistance);
            }
            else
            {
                _settings.Distance = distance;
            }
        }

        public void SetZoomDistance(float zoomDistance)
        {
            _settings.ZoomDistance = Util.ClampToRange(zoomDistance, _settings.ZoomRange);
            SetDistance(_settings.ZoomDistance);
        }

        public void IncreaseZoomDistance(float inputDelta, float deltaTime)
        {
            if (_settings.EnableZoom)
            {
                inputDelta *= _settings.MoveSpeed * deltaTime;
                inputDelta += _settings.ZoomDistance;
                _settings.ZoomDistance = Util.ClampToRange(inputDelta, _settings.ZoomRange);
                SetDistance(_settings.ZoomDistance);
            }
        }

        public float GetDistance()
        {
            if (_settings.EnableZoom)
            {
                return Mathf.Max(_settings.Distance, _settings.ZoomDistance);
            }

            return _settings.Distance;
        }

        public void ResetToUnoccludedDistance()
        {
            _settings.Distance = _settings.EnableZoom ? _settings.ZoomDistance : _settings.DefaultDistance;
        }

        public void ResetToDefaultDistance()
        {
            _settings.Distance = _settings.DefaultDistance;
        }

        public void SetZoomEnabled(EnabledState state)
        {
            _settings.EnableZoom = state == EnabledState.Enabled;
        }

        public void SetCollisionEnabled(EnabledState state)
        {
            _settings.EnableCollision = state == EnabledState.Enabled;
            if (_settings.EnableCollision)
            {
                if (_collider == null)
                {
                    _collider = new EazyCollider(this);
                }
            }
            else
            {
                if (_collider != null)
                {
                    ResetToUnoccludedDistance();
                }

                _collider = null;
            }
        }

        public void ResetPositionAndRotation()
        {
            _rotation = new Vector2();
            _focalPoint = _target.position;
            ResetToDefaultDistance();
        }

        // Targeting
        public void SetTargetingEnabled(EnabledState state)
        {
            _settings.EnableTargetLock = state == EnabledState.Enabled;
            if (_settings.EnableCollision)
            {
                if (_targetManager == null)
                {
                    _targetManager = new EazyTargetManager(this);
                }
            }
            else
            {
                if (_targetManager != null)
                {
                    _targetManager.ClearTargetsInRange();
                }

                _targetManager = null;
            }
        }

        public void AddTargetInRange(ITargetable target)
        {
            if (_settings.EnableTargetLock && _targetManager != null)
            {
                _targetManager.AddTargetInRange(target);
            }
        }

        public void RemoveTargetInRange(ITargetable target)
        {
            if (_settings.EnableTargetLock && _targetManager != null)
            {
                _targetManager.AddTargetInRange(target);
            }
        }
    }
}

