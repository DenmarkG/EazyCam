using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EazyCamera
{
    using EazyCamera.Events;
    using Util = EazyCameraUtility;

    public class EazyController : MonoBehaviour
    {
        [SerializeField] private EazyCam _controlledCamera = null;

        private EazyInputHandler _inputHandler = EazyInputHandler.Create();

        private void Start()
        {
            Debug.Assert(_controlledCamera != null, "Attempting to use a controller on a GameOjbect without an EazyCam component");
            _inputHandler.Init(this);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _inputHandler.HandleInput(this, dt);
        }

        public void SetControlledCamera(EazyCam cam)
        {
            _controlledCamera = cam;
        }

        private void ToggleLockOn()
        {
            _controlledCamera.ToggleLockOn();
        }

        private void CycleTargets()
        {
            _controlledCamera.CycleTargets();
        }

        private void CycleRight()
        {
            _controlledCamera.CycleTargetsRight();
        }

        private void CycleLeft()
        {
            _controlledCamera.CycleTargetsLeft();
        }

        private void ToggleUi()
        {
            EazyEventManager.TriggerEvent(EazyEventKey.OnUiToggled);
        }

        public void OnValidate()
        {
            
        }

        #region InputHandlers
        private abstract class EazyInputHandler
        {
            public abstract void Init(EazyController controller);
            public abstract void HandleInput(EazyController controller, float dt);
            public abstract void Validate(EazyController controller);

            public static EazyInputHandler Create()
            {
#if ENABLE_INPUT_SYSTEM
                return new EazyCameraInputHandler();
#else
                return new EazyLegacyInputHandler();
#endif
            }
        }

        private class EazyLegacyCameraInputHandler : EazyInputHandler
        {
            public override void HandleInput(EazyController controller, float dt)
            {
                float scrollDelta = Input.mouseScrollDelta.y;
                if (scrollDelta > Constants.DeadZone || scrollDelta < -Constants.DeadZone)
                {
                    controller._controlledCamera.IncreaseZoomDistance(scrollDelta, dt);
                }

                float horz = Input.GetAxis(Util.MouseX);
                float vert = Input.GetAxis(Util.MouseY);
                controller._controlledCamera.IncreaseRotation(horz, vert, dt);

                if (Input.GetKeyDown(KeyCode.R))
                {
                    controller._controlledCamera.ResetPositionAndRotation();
                }

                if (Input.GetKeyUp(KeyCode.T))
                {
                    controller.ToggleLockOn();
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    controller.CycleTargets();
                }

                if (Input.GetKeyDown(KeyCode.Q))
                {
                    controller.CycleLeft();
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    controller.CycleRight();
                }

                if (Input.GetKeyDown(KeyCode.U))
                {
                    controller.ToggleUi();
                }
            }

            public override void Init(EazyController controller)
            {
                //
            }

            public override void Validate(EazyController controller)
            {
                //
            }
        }

        private class EazyCameraInputHandler : EazyInputHandler
        {
            private InputAction _toggleLockOn = new InputAction();
            private InputAction _cycleTargets = new InputAction();
            private InputAction _cycleRight = new InputAction();
            private InputAction _cycleLeft = new InputAction();
            private InputAction _zoom = new InputAction();
            private InputAction _orbit = new InputAction();

            private Vector2 _rotation = new Vector2();

            public override void HandleInput(EazyController controller, float dt)
            {
                controller._controlledCamera.IncreaseRotation(_rotation.x, _rotation.y, dt);
            }

            public override void Init(EazyController controller)
            {
                Validate(controller);

                _orbit.performed += OnOrbit;
                _orbit.canceled += OnOrbit;
                _orbit.Enable();
            }

            public override void Validate(EazyController controller)
            {
                if (_orbit.bindings.Count == 0)
                {
                    _orbit.AddBinding(Mouse.current.delta);
                    _orbit.AddBinding(Gamepad.current.rightStick);
                }
            }

            private void OnOrbit(InputAction.CallbackContext cxt)
            {
                _rotation = cxt.ReadValue<Vector2>();
            }
        }
        #endregion // InputHandlers
    }


}
