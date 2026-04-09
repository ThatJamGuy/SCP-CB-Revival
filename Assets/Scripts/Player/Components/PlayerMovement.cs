using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour {
    [Header("Movement Status")]
    [SerializeField, Range(0, 1)] private float stamina = 1f;

    public float CurrentStamina => stamina;
    
    public bool IsActuallySprinting { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float noclipSpeed = 10f;
    [SerializeField] private float crouchSpeed = 2f;

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
    private bool sprintLocked;

    private const float gravity = -9.81f;
    private const float standingHeight = 2.2f;
    private const float crouchingHeight = 0.5f;

    #region Unity Methods
    private void Awake() {
        controller = GetComponent<CharacterController>();
    }

    private void Start() {
        DevConsole.Instance.Add("noclip", () => ToggleNoclip());
    }

    private void Update() {
        if (playerAccessor == null || !playerAccessor.allowInput) return;

        var im = InputManager.Instance;
        if (im != null && im.IsCrouchTriggered) AttemptToggleCrouch();

        if (noclip) HandleNoclip();
        else HandleMovement();
    }
    #endregion

    #region Movement
    private void HandleMovement() {
        if (controller == null) return;

        HandleStamina();

        grounded = controller.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -2f;

        float currentSpeed = playerAccessor.isCrouching ? crouchSpeed : walkSpeed;
        bool wantsToSprint = playerAccessor.isSprinting;
        bool canSprint = !playerAccessor.isCrouching && (playerAccessor.infiniteStamina || stamina > 0f) && playerAccessor.isMoving && !sprintLocked;

        if (wantsToSprint && canSprint) currentSpeed = sprintSpeed;
        
        IsActuallySprinting = wantsToSprint && canSprint;

        Vector3 move = GetMoveDirection(true) * currentSpeed;
        velocity.y += gravity * Time.deltaTime;
        controller.Move((move + velocity) * Time.deltaTime);
    }

    private void HandleNoclip() {
        transform.position += GetMoveDirection(false) * noclipSpeed * Time.deltaTime;
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

        Vector3 move = forward.normalized * input.y + right.normalized * input.x;
        if (move.sqrMagnitude > 1f) move.Normalize();
        return move;
    }
    #endregion

    #region Stamina
    private void HandleStamina() {
        var im = InputManager.Instance;

        if (playerAccessor.isSprinting && !playerAccessor.infiniteStamina && !playerAccessor.isCrouching && playerAccessor.isMoving && !sprintLocked) {
            float drainRate = staminaDrainRate * (1f + playerAccessor.staminaDepletionModifier);
            stamina = Mathf.Max(stamina - drainRate * Time.deltaTime, 0f);
            if (stamina <= 0f) { stamina = 0f; sprintLocked = true; }
        }
        else if (stamina < maxStamina) {
            stamina = Mathf.Min(stamina + staminaRegenRate * Time.deltaTime, maxStamina);
            if (stamina > 0f && im != null && !im.IsSprinting) sprintLocked = false;
        }

        if (im != null && !im.IsSprinting) sprintLocked = false;
        if (staminaSlider) staminaSlider.value = stamina / maxStamina;
    }
    #endregion

    #region Crouch
    private void AttemptToggleCrouch() {
        if (playerAccessor.isCrouching) TryStand();
        else StartCrouch();
    }

    private void StartCrouch() {
        if (playerAccessor.isCrouching) return;
        playerAccessor.isCrouching = true;
        StartHeightTween(controller.height, crouchingHeight);
    }

    private void TryStand() {
        if (!playerAccessor.isCrouching || !CanStand()) return;
        playerAccessor.isCrouching = false;
        StartHeightTween(controller.height, standingHeight);
    }

    private void StartHeightTween(float from, float to) {
        Tween.Custom(from, to, 0.1f, val => controller.height = val);
    }

    private bool CanStand() {
        Vector3 bottom = transform.position + Vector3.up * controller.radius;
        Vector3 top = transform.position + Vector3.up * (standingHeight - controller.radius);
        int mask = ~(1 << LayerMask.NameToLayer("Player"));
        return !Physics.CheckCapsule(bottom, top, controller.radius - 0.05f, mask, QueryTriggerInteraction.Ignore);
    }
    #endregion

    #region Utility
    private void ToggleNoclip() {
        noclip = !noclip;
        if (controller) controller.enabled = !noclip;
        velocity = Vector3.zero;
        Debug.Log($"Toggling noclip mode to: {noclip}");
    }
    #endregion
}