using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Singleton;

namespace SingletonManagers
{
    public class InputHandler : SingletonPersistent
    {
        public static InputHandler Instance => GetInstance<InputHandler>();
        public delegate void OnActionEvent();

        private InputAction MoveInput;
        public Vector2 MoveDirection { get; private set; }
        public Vector2 MousePosition { get; private set; }
        public bool GrenadeThrowStart { get; private set; }

        public event Action<bool> OnAttack;
        public event OnActionEvent OnReload;
        public event OnActionEvent OnPrimaryWeapon;
        public event OnActionEvent OnSecondaryWeapon;
        public event OnActionEvent OnCrouch;
        public event OnActionEvent OnGrenade;
        public event Action<bool> OnSprint;
        public event OnActionEvent OnInteract;

        private void Start()
        {
            MoveInput = gameObject.GetComponent<PlayerInput>().actions.FindAction("Move");
            if (MoveInput == null) { print($"Input System is missing on {gameObject.name}"); }
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
            if (context.performed)
            {
                OnCrouch?.Invoke();
            }
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
            if (context.performed)
            {
                OnPrimaryWeapon?.Invoke();
            }
        }

        public void SecondaryWeaponAction(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnSecondaryWeapon?.Invoke();
            }
        }

        public void ReloadAction(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnReload?.Invoke();
            }
        }

        public void GrenadeAction(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                GrenadeThrowStart = true;
            }
            else if (context.canceled)
            {
                StartCoroutine(DelayedAction(1.7f,
                    () => { GrenadeThrowStart = false; }));
                OnGrenade?.Invoke();
            }
        }

        public void InteractAction(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnInteract?.Invoke();
            }
        }

        private static IEnumerator DelayedAction(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
    }
}
