using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public struct FootstepData { public Texture texture; public AudioClip[] walkingFootstepAudio; public AudioClip[] runningFootstepAudio; }

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float gravity = 9.81f;

    [Header("Look Settings")]
    [SerializeField] private float lookSpeed = 2f;
    [SerializeField] private float maxLookX = 90f;
    [SerializeField] private float minLookX = -90f;

    [Header("Footstep Settings")]
    [SerializeField] private FootstepHandler footstepHandler;

    [Header("Blinking Settings")]
    public bool isBlinking = false;
    [SerializeField] private Image blinkOverlay;
    [SerializeField] private float blinkDrainRate = 0.2f;
    [SerializeField] private Slider blinkSlider;
    [SerializeField] private float blinkDepletionModifier = 0f;

    [Header("Stamina Settings")]
    [SerializeField, Range(0, 1)] private float stamina = 1f;
    [SerializeField] private float maxStamina = 1f;
    [SerializeField] private float staminaDrainRate = 0.2f;
    [SerializeField] private float staminaRegenRate = 0.1f;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private float staminaDepletionModifier = 0f;

    [Header("Other Settings")]
    public bool enabledNoclip = false;
    public bool infiniteBlink = false;
    public bool infiniteStamina = false;
    public bool isMoving;
    public bool isSprinting;

    private CharacterController characterController;
    private Vector3 moveDirection;
    private float rotationX;
    private bool isBlinkingOverlayActive;
    private float blinkTimer;
    private float blinkOverlayDuration = 0.2f;
    private float blinkOverlayHoldDuration = 0.2f;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        footstepHandler.Initialize(characterController);
        UpdateCursorState();
        staminaSlider.value = stamina;
        blinkTimer = 1f;
        blinkSlider.value = blinkTimer;
        blinkOverlay.enabled = false;
    }

    private void Update()
    {
        if (GameManager.Instance.disablePlayerInputs) return;

        HandleNoclip();
        HandleMovement();
        HandleLook();
        HandleStamina();
        HandleBlinking();
        footstepHandler.UpdateFootsteps(isMoving, isSprinting);
    }

    private void HandleNoclip()
    {
        if (enabledNoclip)
        {
            characterController.enabled = false;
            float moveSpeed = isSprinting ? 10f : 5f;
            float vertical = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
            float horizontal = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
            float upDown = 0f;

            if (Input.GetKey(GameManager.Instance.noclipUp)) upDown = moveSpeed * Time.deltaTime;
            if (Input.GetKey(GameManager.Instance.noclipDown)) upDown = -moveSpeed * Time.deltaTime;

            transform.Translate(new Vector3(horizontal, upDown, vertical));
        }
        else
        {
            characterController.enabled = true;
        }
    }

    private void HandleMovement()
    {
        if (enabledNoclip) return;

        if (characterController.isGrounded)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            if (moveX != 0 || moveZ != 0)
            {
                moveDirection = transform.TransformDirection(new Vector3(moveX, 0, moveZ)) * DetermineCurrentSpeed();
                isMoving = true;
            }
            else
            {
                moveDirection.x = 0;
                moveDirection.z = 0;
                isMoving = false;
            }

            isSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving && (infiniteStamina || stamina > 0);
        }

        moveDirection.y -= gravity * Time.deltaTime;
        characterController.Move(moveDirection * Time.deltaTime);
    }


    private float DetermineCurrentSpeed()
    {
        if (isSprinting) return sprintSpeed;
        if (Input.GetKey(KeyCode.LeftControl)) return crouchSpeed;
        return walkSpeed;
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        rotationX = Mathf.Clamp(rotationX - mouseY, minLookX, maxLookX);
        transform.Rotate(0, mouseX, 0);
        Quaternion currentRotation = Camera.main.transform.localRotation;
        Camera.main.transform.localRotation = Quaternion.Euler(rotationX, 0, currentRotation.eulerAngles.z);
    }

    private void HandleStamina()
    {
        if (isSprinting && !infiniteStamina)
        {
            float adjustedDrainRate = staminaDrainRate * (1 + staminaDepletionModifier);
            stamina = Mathf.Max(stamina - adjustedDrainRate * Time.deltaTime, 0f);
            if (stamina == 0f) isSprinting = false;
        }
        else if (stamina < maxStamina)
        {
            stamina = Mathf.Min(stamina + staminaRegenRate * Time.deltaTime, maxStamina);
        }

        staminaSlider.value = stamina / maxStamina;
    }

    private void HandleBlinking()
    {
        if (infiniteBlink)
        {
            blinkTimer = 1f;
        }
        else if (!isBlinkingOverlayActive)
        {
            float modifiedDrainRate = blinkDrainRate * (1 + blinkDepletionModifier);
            blinkTimer = Mathf.Max(blinkTimer - modifiedDrainRate * Time.deltaTime, 0f);
            if (blinkTimer == 0) TriggerBlink();
        }

        if (Input.GetKey(KeyCode.Space) && !infiniteBlink)
        {
            if (!isBlinkingOverlayActive)
            {
                blinkOverlay.enabled = true;
                isBlinking = true;
                isBlinkingOverlayActive = true;
                blinkTimer = 1f;
                CancelInvoke(nameof(EndBlink));
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space) && isBlinkingOverlayActive)
        {
            Invoke(nameof(EndBlink), blinkOverlayHoldDuration);
        }

        blinkSlider.value = blinkTimer;
    }

    private void TriggerBlink()
    {
        blinkOverlay.enabled = true;
        isBlinking = true;
        isBlinkingOverlayActive = true;
        blinkTimer = 1f;
        Invoke(nameof(EndBlink), blinkOverlayDuration);
    }

    private void EndBlink()
    {
        blinkOverlay.enabled = false;
        isBlinking = false;
        isBlinkingOverlayActive = false;
    }

    private void UpdateCursorState()
    {
        bool disablePlayerInputs = GameManager.Instance.disablePlayerInputs;
        Cursor.lockState = disablePlayerInputs ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = disablePlayerInputs;
    }
}