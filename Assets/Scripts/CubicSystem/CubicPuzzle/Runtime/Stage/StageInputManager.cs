using UnityEngine;


namespace CubicSystem.CubicPuzzle
{
    public class StageInputManager :MonoBehaviour
    {
        public delegate void PressDown();
        public event PressDown OnPressDown;

        public delegate void PressUp();
        public event PressUp OnPressUp;

        public delegate void Performed();
        public event Performed OnPerformed;

        public StageInputActions InputActions { get; private set; }

        private void Awake()
        {
            InputActions = new StageInputActions();

            InputActions.Touch.TouchPress.started += TouchPressDown;
            InputActions.Touch.TouchPress.canceled += TouchPressUp;
            InputActions.Touch.TouchPosition.performed += TouchPerformed;
        }

        private void OnEnable()
        {
            InputActions.Enable();
        }

        private void OnDisable()
        {
            InputActions.Disable();
        }


        private void TouchPressDown(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            OnPressDown?.Invoke();
        }

        private void TouchPressUp(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            OnPressUp?.Invoke();
        }

        private void TouchPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            OnPerformed?.Invoke();
        }

        public Vector2 GetTouchPoisition()
        {
            return InputActions.Touch.TouchPosition.ReadValue<Vector2>();
        }
    }
}
