using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public event Action OnJump;
    public event Action OnInteract;

    private PlayerInputActions inputActions;

    private System.Action<UnityEngine.InputSystem.InputAction.CallbackContext> jumpAction;
    private System.Action<UnityEngine.InputSystem.InputAction.CallbackContext> interactAction;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        jumpAction = ctx => OnJump?.Invoke();
        interactAction = ctx => OnInteract?.Invoke();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Jump.performed += jumpAction;
        inputActions.Player.Interact.performed += interactAction;
    }

    public Vector2 GetMoveVector()
    {
        return inputActions.Player.Move.ReadValue<Vector2>();
    }
    
    public bool IsPrimaryPressed()
    {
        return inputActions.Player.Primary.IsPressed();
    }

    public bool IsSecondaryPressed() {
        return inputActions.Player.Secondary.IsPressed();
    }

    public Vector2 GetMousePosition()
    {
        return inputActions.Player.MousePosition.ReadValue<Vector2>();
    }

    private void OnDisable()
    {
        inputActions.Player.Jump.performed -= jumpAction;
        inputActions.Player.Interact.performed -= interactAction;
        inputActions.Disable();
    }
}
