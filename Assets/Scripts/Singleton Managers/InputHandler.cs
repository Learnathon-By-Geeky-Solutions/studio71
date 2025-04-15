using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Singleton;

namespace SingletonManagers
{
    public class InputHandler : SingletonPersistent<InputHandler>
    {
        private InputAction MoveInput;
        public Vector2 MoveDirection { get; private set; }
        public Vector2 MousePosition { get; private set; }
        public bool GrenadeThrowStart { get; private set; }

        public event Action<bool> OnAttack;
        public event Action OnReload;
        public event Action OnPrimaryWeapon;
        public event Action OnSecondaryWeapon;
        public event Action OnCrouch;
        public event Action OnGrenade;
        public event Action<bool> OnSprint;
        public event Action OnInteract;

        private void Start()
        {
            MoveInput = GetComponent<PlayerInput>().actions.FindAction("Move");
            if (MoveInput == null)
            {
                Debug.LogWarning($"Input System is missing on {gameObject.name}");
            }
        }

        private void Update()
        {
            MoveAction();
            LookAction();
        }

        private void MoveAction()
        {
            MoveDirection = MoveInput.ReadValue<Vector2>();
        }

        private void LookAction()
        {
            if (Mouse.current == null) return;
            MousePosition = Mouse.current.position.ReadValue();
        }

        public void CrouchAction(InputAction.CallbackContext context)
        {
            if (context.performed) OnCrouch?.Invoke();
        }

        public void SprintAction(InputAction.CallbackContext context)
        {
            OnSprint?.Invoke(context.performed);
        }

        public void AttackAction(InputAction.CallbackContext context)
        {
            OnAttack?.Invoke(context.performed);
        }

        public void PrimaryWeaponAction(InputAction.CallbackContext context)
        {
            if (context.performed) OnPrimaryWeapon?.Invoke();
        }

        public void SecondaryWeaponAction(InputAction.CallbackContext context)
        {
            if (context.performed) OnSecondaryWeapon?.Invoke();
        }

        public void ReloadAction(InputAction.CallbackContext context)
        {
            if (context.performed) OnReload?.Invoke();
        }

        public void GrenadeAction(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                GrenadeThrowStart = true;
            }
            else if (context.canceled)
            {
                StartCoroutine(DelayedAction(1.7f, () => { GrenadeThrowStart = false; }));
                OnGrenade?.Invoke();
            }
        }

        public void InteractAction(InputAction.CallbackContext context)
        {
            if (context.performed) OnInteract?.Invoke();
        }

        private static IEnumerator DelayedAction(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
    }
}
