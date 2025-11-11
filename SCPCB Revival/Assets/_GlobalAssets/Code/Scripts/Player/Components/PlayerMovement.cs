using SickDev.CommandSystem;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {
    [Header("Movement Status")]
    [SerializeField, Range(0, 1)] private float stamina = 1f;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float noclipSpeed = 10f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 1f;
    [SerializeField] private float staminaDrainRate = 0.2f;
    [SerializeField] private float staminaRegenRate = 0.1f;
    [SerializeField] private Slider staminaSlider;

    [Header("References")]
    [SerializeField] private Transform playerCameraRoot;

    private PlayerAccessor playerAccessor => PlayerAccessor.instance;
    private CharacterController controller;
    private Vector3 velocity;
    private bool grounded;
    private bool noclip;

    private bool sprintLocked = false;

    private const float gravity = -9.81f;

    #region Default Methods
    private void OnEnable() {
        DevConsole.singleton.AddCommand(new ActionCommand(ToggleNoclip) { className = "Player" });
    }

    private void Awake() {
        controller = GetComponent<CharacterController>();
    }

    private void Update() {
        if (playerAccessor == null || !playerAccessor.allowInput) return;

        if (noclip) HandleNoclip();
        else HandleMovement();
    }
    #endregion

    #region Private Methods
    private void HandleMovement() {
        if (controller == null) return;
        HandleStamina();

        grounded = controller.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -2f;

        float currentSpeed = walkSpeed;

        bool wantsToSprint = playerAccessor.isSprinting;
        bool canSprint = !playerAccessor.isCrouching && (playerAccessor.infiniteStamina || stamina > 0f) && playerAccessor.isMoving && !sprintLocked;
        if (wantsToSprint && canSprint) currentSpeed = sprintSpeed;

        Vector3 move = GetMoveDirection(flattenY: true) * currentSpeed;

        velocity.y += gravity * Time.deltaTime;

        Vector3 displacement = (move + velocity) * Time.deltaTime;
        controller.Move(displacement);
    }

    private void HandleStamina() {
        var inputManager = InputManager.Instance;

        if (playerAccessor.isSprinting && !playerAccessor.infiniteStamina && !playerAccessor.isCrouching && playerAccessor.isMoving && !sprintLocked) {
            float drainRate = staminaDrainRate * (1f + playerAccessor.staminaDepletionModifier);
            stamina = Mathf.Max(stamina - drainRate * Time.deltaTime, 0f);

            if (stamina <= 0f) {
                stamina = 0f;
                sprintLocked = true;
            }
        }
        else if (stamina < maxStamina) {
            stamina = Mathf.Min(stamina + staminaRegenRate * Time.deltaTime, maxStamina);

            if (stamina > 0f && inputManager != null && !inputManager.IsSprinting) {
                sprintLocked = false;
            }
        }

        if (inputManager != null && !inputManager.IsSprinting) {
            sprintLocked = false;
        }

        if (staminaSlider) staminaSlider.value = stamina / maxStamina;
    }

    private void HandleNoclip() {
        transform.position += GetMoveDirection(flattenY: false) * noclipSpeed * Time.deltaTime;
    }

    private Vector3 GetMoveDirection(bool flattenY) {
        var im = InputManager.Instance;
        Vector2 input = im != null ? im.Move : Vector2.zero;
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

    private void ToggleNoclip() {
        noclip = !noclip;
        if (controller) controller.enabled = !noclip;
        velocity = Vector3.zero;
        Debug.Log($"Toggling noclip mode to: {noclip}");
    }
    #endregion
}