using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour {
    public static InputManager Instance { get; private set; }

    public PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction sprintAction;

    private void Awake() {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        sprintAction = playerInput.actions["Sprint"];
    }

    public Vector2 Move => moveAction.ReadValue<Vector2>();
    public Vector2 Look => lookAction.ReadValue<Vector2>();
    public bool IsMoving => Move.magnitude > 0;
    public bool IsSprinting => sprintAction.ReadValue<float>() > 0;
}