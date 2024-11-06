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

    [Header("Stamina Settings")]
    [SerializeField, Range(0, 1)] private float stamina = 1f;
    [SerializeField] private float maxStamina = 1f;
    [SerializeField] private float staminaDrainRate = 0.2f;
    [SerializeField] private float staminaRegenRate = 0.1f;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private float staminaDepletionModifier = 0f;
    [SerializeField] private bool infiniteStamina = false;

    private CharacterController characterController;
    private Vector3 moveDirection;
    private float rotationX;
    private bool isMoving;
    private bool isSprinting;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        footstepHandler.Initialize(characterController);
        UpdateCursorState();
        staminaSlider.value = stamina;
    }

    private void Update()
    {
        if (GameManager.Instance.disablePlayerInputs) return;

        HandleMovement();
        HandleLook();
        HandleStamina();
        footstepHandler.UpdateFootsteps(isMoving, isSprinting);
    }

    private void HandleMovement()
    {
        if (characterController.isGrounded)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            moveDirection = transform.TransformDirection(new Vector3(moveX, 0, moveZ)) * DetermineCurrentSpeed();
            isMoving = moveX != 0 || moveZ != 0;
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
        Camera.main.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
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

    private void UpdateCursorState()
    {
        bool disablePlayerInputs = GameManager.Instance.disablePlayerInputs;
        Cursor.lockState = disablePlayerInputs ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = disablePlayerInputs;
    }
}