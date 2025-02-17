using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : SingletonPersistent<InputHandler>
{
    public delegate void OnActionEvent();
    public delegate void OnActionEventBool(bool value);

    private InputAction MoveInput;
    public Vector2 MoveDirection { get; private set; }
    public Vector2 MousePosition { get; private set; }

    public event OnActionEventBool OnAttack;
    public event OnActionEvent OnReload;
    public event OnActionEvent OnPrimaryWeapon;
    public event OnActionEvent OnSecondaryWeapon;
    public event OnActionEvent OnCrouch;
    public event OnActionEventBool OnSprint;

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
        if (!context.performed)
        {
            OnReload?.Invoke();
        }
    }
}
