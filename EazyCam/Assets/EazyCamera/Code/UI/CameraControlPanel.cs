using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EazyCamera
{
    public class CameraControlPanel : MonoBehaviour
    {
        public event System.Action OnClosed = () => { };

        [SerializeField] private EazyCam _sceneCamera = null;
        private EazyCam.Settings _activeSettings;
        private EazyCam.Settings _defaultSettings;

        private void Start()
        {
            bool setupIsInvalid = _sceneCamera == null;
            if (setupIsInvalid)
            {
                Debug.LogWarning("[CameraControlPanel] Disabling Control Panel as it's setup is invalid");
                this.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            _activeSettings = _sceneCamera.CameraSettings;
            _defaultSettings = _activeSettings;
        }

        public void CancelChanges()
        {
            Close();
        }

        public void ApplyChanges()
        {
            _sceneCamera.OverrideSettings(_activeSettings);
            Close();
        }

        private void Close()
        {
            _sceneCamera.OverrideSettings(_defaultSettings);
            OnClosed();
        }

        public void SetCameraZoomDistanceNormalized(float normalizedDistance)
        {
            float actualDistance = _activeSettings.ZoomRange.Lerp(normalizedDistance);
            _sceneCamera.SetZoomDistance(actualDistance);
        }

        public void SetCollisionsEnabled(bool enabled)
        {
            _activeSettings.EnableCollision = enabled;
            _sceneCamera.SetCollisionEnabled(enabled ? EnabledState.Enabled : EnabledState.Disabled);
        }
    }
}

