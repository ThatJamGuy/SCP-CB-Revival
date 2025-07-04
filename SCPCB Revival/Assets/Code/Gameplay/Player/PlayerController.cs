using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {

    #region Public Variables
    [Header("Movement Settings")]
    public InputActionAsset playerControls;
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 1f;
    public float crouchSmoothSpeed = 8f;
    public float gravity = 9.81f;

    [Header("Look Settings")]
    public Camera playerCamera;
    public float lookSpeed = 0.3f;
    public float maxLookX = 80f;
    public float minLookX = -80f;

    [Header("Blink")]
    public bool infiniteBlink;
    public float blinkDrainRate = 0.05f;
    public float blinkDepletionModifier = 0f;
    public float blinkOverlayDuration = 0.2f;
    public float blinkOverlayHoldDuration = 0.2f;
    public Image blinkOverlay;
    public Slider blinkSlider;

    [Header("Stamina")]
    public bool infiniteStamina;
    public float maxStamina = 1f;
    public float staminaDrainRate = 0.2f;
    public float staminaRegenRate = 0.1f;
    public float staminaDepletionModifier = 0f;
    [SerializeField, Range(0, 1)] float stamina = 1f;
    [SerializeField] Slider staminaSlider;

    [Header("States")]
    public bool isMoving;
    public bool isSprinting;
    public bool isCrouching;
    public bool isBlinking;
    #endregion

    #region Private Variables
    private CharacterController characterController;
    private Vector3 moveDirection;
    private float rotationX;
    private bool isBlinkingOverlayActive;
    private float blinkTimer;

    private Vector3 targetScale;
    private Vector3 crouchScale = new Vector3(1, 0.65f, 1);
    private Vector3 standScale = new Vector3(1, 1.1f, 1);
    #endregion

    private void OnEnable() {
        playerControls.Enable();
    }

    private void OnDisable() {
        playerControls.Disable();
    }

    private void Start() {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        staminaSlider.value = stamina;
        blinkTimer = 1f;
        blinkSlider.value = blinkTimer;
        blinkOverlay.enabled = false;
    }

    private void Update() {
        //if (GameManager.Instance.disablePlayerInputs) return;

        HandleMovement();
        HandleLook();
        HandleStamina();
        HandleBlinking();
        HandleCrouch();
    }

    private void HandleCrouch() {
        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            isCrouching = !isCrouching;

            if (isCrouching) {
                isSprinting = false;
            }
        }

        targetScale = isCrouching ? crouchScale : standScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * crouchSmoothSpeed);
    }

    private void HandleMovement() {
        if (characterController.isGrounded) {
            Vector2 moveInput = playerControls.FindAction("Move").ReadValue<Vector2>();

            if (moveInput.x != 0 || moveInput.y != 0) {
                moveDirection = transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y)) * DetermineCurrentSpeed();
                isMoving = true;
            }
            else {
                moveDirection.x = 0;
                moveDirection.z = 0;
                isMoving = false;
            }

            isSprinting = playerControls.FindAction("Sprint").IsPressed()
                          && isMoving
                          && !isCrouching
                          && (infiniteStamina || stamina > 0);
        }

        moveDirection.y -= gravity * Time.deltaTime;
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private float DetermineCurrentSpeed() {
        if (isCrouching)
            return crouchSpeed;

        if (isSprinting)
            return sprintSpeed;

        return walkSpeed;
    }

    private void HandleLook() {
        Vector2 lookInput = playerControls.FindAction("Look").ReadValue<Vector2>() * lookSpeed;

        rotationX = Mathf.Clamp(rotationX - lookInput.y, minLookX, maxLookX);
        transform.Rotate(0, lookInput.x, 0);
        Quaternion currentRotation = playerCamera.transform.localRotation;
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, currentRotation.eulerAngles.z);
    }

    private void HandleStamina() {
        if (isSprinting && !infiniteStamina && !isCrouching) {
            float adjustedDrainRate = staminaDrainRate * (1 + staminaDepletionModifier);
            stamina = Mathf.Max(stamina - adjustedDrainRate * Time.deltaTime, 0f);
            if (stamina == 0f) isSprinting = false;
        }
        else if (stamina < maxStamina) {
            stamina = Mathf.Min(stamina + staminaRegenRate * Time.deltaTime, maxStamina);
        }

        staminaSlider.value = stamina / maxStamina;
    }

    private void HandleBlinking() {
        if (infiniteBlink) {
            blinkTimer = 1f;
        }
        else if (!isBlinkingOverlayActive) {
            float modifiedDrainRate = blinkDrainRate * (1 + blinkDepletionModifier);
            blinkTimer = Mathf.Max(blinkTimer - modifiedDrainRate * Time.deltaTime, 0f);
            if (blinkTimer == 0) TriggerBlink();
        }

        if (Input.GetKey(KeyCode.Space) && !infiniteBlink) {
            if (!isBlinkingOverlayActive) {
                blinkOverlay.enabled = true;
                isBlinking = true;
                isBlinkingOverlayActive = true;
                blinkTimer = 1f;
                CancelInvoke(nameof(EndBlink));
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space) && isBlinkingOverlayActive) {
            Invoke(nameof(EndBlink), blinkOverlayHoldDuration);
        }

        blinkSlider.value = blinkTimer;
    }

    private void TriggerBlink() {
        blinkOverlay.enabled = true;
        isBlinking = true;
        isBlinkingOverlayActive = true;
        blinkTimer = 1f;
        Invoke(nameof(EndBlink), blinkOverlayDuration);
    }

    private void EndBlink() {
        blinkOverlay.enabled = false;
        isBlinking = false;
        isBlinkingOverlayActive = false;
    }
}