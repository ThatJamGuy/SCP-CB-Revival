using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class InputManager : MonoBehaviour {
    public static InputManager Instance { get; private set; }

    public PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    public InputAction interactAction { get; private set; }
    public InputAction inventoryAction { get; private set; }
    public InputAction consoleAction { get; private set; }
    public InputAction blinkAction { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        sprintAction = playerInput.actions["Sprint"];
        crouchAction = playerInput.actions.FindAction("Crouch", false);
        interactAction = playerInput.actions["Interact"];
        inventoryAction = playerInput.actions["Inventory"];
        consoleAction = playerInput.actions["Console"];
        blinkAction = playerInput.actions["Blink"];
    }

    private void OnEnable() {
        blinkAction.Enable();
    }

    private void OnDisable() {
        blinkAction.Disable();
    }

    public Vector2 Move => moveAction.ReadValue<Vector2>();
    public Vector2 Look => lookAction.ReadValue<Vector2>();
    public bool IsMoving => Move.magnitude > 0;
    public bool IsSprinting => sprintAction.ReadValue<float>() > 0;
    public bool IsCrouchTriggered => crouchAction != null && crouchAction.triggered;
    public bool IsBlinkHeld => blinkAction.IsPressed();
}