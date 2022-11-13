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
            OnClosed();
        }
    }
}

