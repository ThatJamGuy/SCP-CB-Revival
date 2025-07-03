using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using NaughtyAttributes;

public class PlayerBase : MonoBehaviour {
    [Header("Input")]
    public InputActionAsset playerControls;

    [Header("Player Status")]
    [ProgressBar("Stamina", "maxStamina", EColor.Green)] public float stamina = 1f;
    public bool isMoving;
    public bool isSprinting;
    public bool isCrouching;

    [Header("Modifier Settings")]
    public float staminaDepletionModifier = 0f;

    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 1f;

    [Header("Stamina")]
    public float maxStamina = 1f;
    public float staminaDrainRate = 0.2f;
    public float staminaRegenRate = 0.1f;
    
    [SerializeField] Slider staminaSlider;

    [Header("User Cheats")]
    public bool infiniteStamina;

    private CharacterController characterController;
    private PlayerFootsteps playerFootsteps;
    private InputAction moveAction, sprintAction, crouchAction;

    private Vector2 moveInput;
    private float currentSpeed;
    private float terminalVelocity = -53f;
    private float verticalVelocity;

    private const float gravity = -10.81f;

    #region EnableAndDisable
    private void OnEnable() {
        playerControls.Enable();
        var actionMap = playerControls.FindActionMap("Player", true);

        moveAction = actionMap.FindAction("Move", true);
        sprintAction = actionMap.FindAction("Sprint", true);
        crouchAction = actionMap.FindAction("Crouch", true);

        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += _ => moveInput = Vector2.zero;

        sprintAction.performed += _ => isSprinting = true;
        sprintAction.canceled += _ => isSprinting = false;

        crouchAction.performed += _ => isCrouching = true;
        crouchAction.canceled += _ => isCrouching = false;
    }

    private void OnDisable() {
        moveAction.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled -= _ => moveInput = Vector2.zero;

        sprintAction.performed -= _ => isSprinting = true;
        sprintAction.canceled -= _ => isSprinting = false;

        crouchAction.performed -= _ => isCrouching = true;
        crouchAction.canceled -= _ => isCrouching = false;

        playerControls.Disable();
    }
    #endregion

    #region Default Methods
    private void Start() {
       characterController = GetComponent<CharacterController>();
        playerFootsteps = GetComponentInChildren<PlayerFootsteps>();
    }

    private void Update() {
        Move();
        HandleStamina();

        isMoving = moveInput.sqrMagnitude > 0.01f;
    }
    #endregion

    #region Private Methods
    private void Move() {
        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (isCrouching) currentSpeed = crouchSpeed;
        else if (isSprinting && stamina > 0f) currentSpeed = sprintSpeed;
        else currentSpeed = walkSpeed;

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        move = transform.TransformDirection(move) * currentSpeed;

        verticalVelocity += gravity * Time.deltaTime;
        verticalVelocity = Mathf.Max(verticalVelocity, terminalVelocity);

        move.y = verticalVelocity;

        characterController.Move(move * Time.deltaTime);

        playerFootsteps.UpdateFootsteps();
    }

    private void HandleStamina() {
        if (isSprinting && !infiniteStamina && !isCrouching) {
            float drainRate = staminaDrainRate * (1f + staminaDepletionModifier);
            stamina = Mathf.Max(stamina - drainRate * Time.deltaTime, 0f);
            if (stamina == 0f) isSprinting = false;
        }
        else if (stamina < maxStamina) {
            stamina = Mathf.Min(stamina + staminaRegenRate * Time.deltaTime, maxStamina);
        }

        if (staminaSlider) staminaSlider.value = stamina / maxStamina;
    }
    #endregion
}