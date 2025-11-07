using UnityEngine;
using SickDev.CommandSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {
    [Header("Movement Settings")]
    public bool movementEnabled = true;
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float noclipSpeed = 10f;

    [Header("References")]
    [SerializeField] Transform playerCameraRoot;

    CharacterController controller;
    Vector3 velocity;
    bool grounded;
    bool noclip;

    const float gravity = -9.81f;

    void OnEnable() {
        DevConsole.singleton.AddCommand(new ActionCommand(ToggleNoclip) { className = "Player" });
    }

    void Awake() {
        controller = GetComponent<CharacterController>();
    }

    void Update() {
        if (!movementEnabled || playerCameraRoot == null) return;
        if (noclip) HandleNoclip();
        else HandleMovement();
    }

    void HandleMovement() {
        grounded = controller.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -2f;

        Vector3 move = GetMoveDirection(flattenY: true) * walkSpeed;
        controller.Move(move * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleNoclip() {
        transform.position += GetMoveDirection(flattenY: false) * noclipSpeed * Time.deltaTime;
    }

    Vector3 GetMoveDirection(bool flattenY) {
        Vector2 input = InputManager.Instance.Move;
        Vector3 forward = playerCameraRoot.forward;
        Vector3 right = playerCameraRoot.right;

        if (flattenY) {
            forward.y = 0f;
            right.y = 0f;
        }

        Vector3 move = (forward.normalized * input.y + right.normalized * input.x);
        if (move.sqrMagnitude > 1f) move.Normalize();
        return move;
    }

    void ToggleNoclip() {
        noclip = !noclip;
        if (controller) controller.enabled = !enabled;
        velocity = Vector3.zero;
        Debug.Log($"Toggling noclip mode to: {noclip}");
    }
}