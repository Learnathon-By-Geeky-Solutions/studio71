using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Singleton;

namespace SingletonManagers
{
    public class InputHandler : SingletonPersistent<InputHandler>
    {
        public delegate void OnActionEvent();

        private InputAction MoveInput;
        public Vector2 MoveDirection { get; private set; }
        public Vector2 MousePosition { get; private set; }
        public bool GrenadeThrowStart { get; private set; }

        private event Action<bool> OnAttack;
        private event OnActionEvent OnReload;
        private event OnActionEvent OnPrimaryWeapon;
        private event OnActionEvent OnSecondaryWeapon;
        private event OnActionEvent OnCrouch;
        private event OnActionEvent OnGrenade;
        private event Action<bool> OnSprint;
        private event OnActionEvent OnInteract;

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

        // Subscription Methods
        public void SubscribeAttack(Action<bool> callback) => OnAttack += callback;
        public void SubscribeReload(OnActionEvent callback) => OnReload += callback;
        public void SubscribePrimaryWeapon(OnActionEvent callback) => OnPrimaryWeapon += callback;
        public void SubscribeSecondaryWeapon(OnActionEvent callback) => OnSecondaryWeapon += callback;
        public void SubscribeCrouch(OnActionEvent callback) => OnCrouch += callback;
        public void SubscribeGrenade(OnActionEvent callback) => OnGrenade += callback;
        public void SubscribeSprint(Action<bool> callback) => OnSprint += callback;
        public void SubscribeInteract(OnActionEvent callback) => OnInteract += callback;

        // Unsubscription Methods
        public void UnsubscribeAttack(Action<bool> callback) => OnAttack -= callback;
        public void UnsubscribeReload(OnActionEvent callback) => OnReload -= callback;
        public void UnsubscribePrimaryWeapon(OnActionEvent callback) => OnPrimaryWeapon -= callback;
        public void UnsubscribeSecondaryWeapon(OnActionEvent callback) => OnSecondaryWeapon -= callback;
        public void UnsubscribeCrouch(OnActionEvent callback) => OnCrouch -= callback;
        public void UnsubscribeGrenade(OnActionEvent callback) => OnGrenade -= callback;
        public void UnsubscribeSprint(Action<bool> callback) => OnSprint -= callback;
        public void UnsubscribeInteract(OnActionEvent callback) => OnInteract -= callback;
    }
}
