using UnityEngine;

namespace EazyCamera
{
    using Util = EazyCameraUtility;

    public class EazyController : MonoBehaviour
    {
        [SerializeField] private EazyCam _controlledCamera = null;

        private void Start()
        {
            Debug.Assert(_controlledCamera != null, "Attempting to use a controller on a GameOjbect without an EazyCam component");
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            float scrollDelta = Input.mouseScrollDelta.y;
            if (scrollDelta > Constants.DeadZone || scrollDelta < -Constants.DeadZone)
            {
                _controlledCamera.IncreaseZoomDistance(scrollDelta, dt);
            }

            float horz = Input.GetAxis(Util.MouseX);
            float vert = Input.GetAxis(Util.MouseY);
            _controlledCamera.IncreaseRotation(horz, vert, dt);

            if (Input.GetKeyDown(KeyCode.R))
            {
                _controlledCamera.ResetPositionAndRotation();
            }


            if (Input.GetKeyUp(KeyCode.Space))
            {
                ToggleLockOn();
            }
        }

        public void SetControlledCamera(EazyCam cam)
        {
            _controlledCamera = cam;
        }

        private void BeginLockOn()
        {
            //
        }

        private void EndLockOn()
        {
            //
        }

        private void ToggleLockOn()
        {
            _controlledCamera.ToggleLockOn();
        }
    }
}
