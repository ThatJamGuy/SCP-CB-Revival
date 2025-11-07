using UnityEngine;
using UnityEngine.InputSystem;
using SickDev.DevConsole;

public class InputManager : MonoBehaviour {
    public static InputManager Instance { get; private set; }

    public PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction lookAction;

    private void Awake() {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
    }

    public Vector2 Move => moveAction.ReadValue<Vector2>();
    public Vector2 Look => lookAction.ReadValue<Vector2>();
}