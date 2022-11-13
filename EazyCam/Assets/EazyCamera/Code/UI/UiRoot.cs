using UnityEngine;
using UnityEngine.UI;

namespace EazyCamera
{
    public class UiRoot : MonoBehaviour
    {
        [SerializeField] private GameObject _collapsedView = null;
        [SerializeField] private CameraControlPanel _fullView = null;

        private EnabledState _panelState = EnabledState.Disabled;

        private void Start()
        {
            bool setupIsInvalid = _collapsedView == null || _fullView == null;
            if (setupIsInvalid)
            {
                Debug.LogWarning("[UiRoot] Disabling Control Panel as it's setup is invalid");
                this.gameObject.SetActive(false);
            }
            else
            {
                _fullView.OnClosed += HideFullPanel;
                _fullView.gameObject.SetActive(false);

                _collapsedView.SetActive(true);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleView();
            }
        }

        private void ToggleView()
        {
            if (_panelState == EnabledState.Disabled)
            {
                ShowFullPanel();
            }
            else
            {
                HideFullPanel();
            }
        }

        private void ShowFullPanel()
        {
            _collapsedView.SetActive(false);
            _fullView.gameObject.SetActive(true);
            _panelState = EnabledState.Enabled;
            Cursor.lockState = CursorLockMode.None;
        }

        private void HideFullPanel()
        {
            _collapsedView.SetActive(true);
            _fullView.gameObject.SetActive(false);
            _panelState = EnabledState.Disabled;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
