using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {
    [Header("Movement Status")]
    [SerializeField, Range(0, 1)] private float stamina = 1f;

    //public float CurrentStamina => stamina;

    //[Header("Stamina")]
    //[SerializeField] private float maxStamina = 1f;
    //[SerializeField] private float staminaDrainRate = 0.2f;
    //[SerializeField] private float staminaRegenRate = 0.1f;
    //[SerializeField] private Slider staminaSlider;

    [Header("References")] 
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform playerCameraRoot;

    private static Player player => Player.Instance;
    private static InputManager inputManager => InputManager.Instance;

    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    
    private Vector2 moveDirection;
    private Vector3 velocity;

    private bool cantFunction;

    private float walkingSpeed;
    private float sprintSpeed;
    private float crouchSpeed;

    private const float GRAVITY = -9.81f;
    private const float STANDING_HEIGHT = 2.2f;
    private const float CROUCHING_HEIGHT = 0.5f;

    private void Start() {
        // If there is no InputManager available at the start, disallow functionality and print a warning in console
        if (inputManager == null) {
            cantFunction = true;
            Debug.Log("<color=red>[PlayerMovement]</color> InputManager was not found, moving will not work!");

            return;
        }
        
        // Retrieve the different input actions from InputManager so they can be cached
        moveAction = inputManager.GetAction("Player", "Move");
        sprintAction = inputManager.GetAction("Player", "Sprint");
        crouchAction = inputManager.GetAction("Player", "Crouch");
        
        // Set the different speeds to their values from settings.json, which are stored on the player instance
        walkingSpeed = player.walkSpeed;
        sprintSpeed = player.sprintSpeed;
        crouchSpeed = player.crouchSpeed;
    }

    private void Update() {
        // If input is manually disabled or functionality is broken then don't do anything
        if (player.disableInput || cantFunction) return;
        
        HandleMovement();
    }
    
    private void HandleMovement() {
        var input = moveAction.ReadValue<Vector2>();
        var move = transform.right * input.x + transform.forward * input.y;
        var currentSpeed = walkingSpeed; // Temporary until sprinting and crouching checks are in

        // Move the character controller in the proper Vector3 direction at the currentSpeed
        characterController.Move(move * (currentSpeed * Time.deltaTime));
        
        // If the player is not on the ground, set the up-down velocity to -2 so they go down
        if (characterController.isGrounded && velocity.y < 0f) velocity.y = -2f;
        
        // Set the velocity to the gravity value
        // Then move the character controller by the velocity alongside the previous Vector3 movement
        velocity.y += GRAVITY * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    /*#region Stamina
    private void HandleStamina() {
        var im = InputManager.Instance;

        if (playerAccessor.isSprinting && !playerAccessor.infiniteStamina && !playerAccessor.isCrouching && playerAccessor.isMoving && !sprintLocked) {
            float drainRate = staminaDrainRate * (1f + playerAccessor.staminaDepletionModifier);
            stamina = Mathf.Max(stamina - drainRate * Time.deltaTime, 0f);
            if (stamina <= 0f) { stamina = 0f; sprintLocked = true; }
        }
        else if (stamina < maxStamina) {
            stamina = Mathf.Min(stamina + staminaRegenRate * Time.deltaTime, maxStamina);
            //if (stamina > 0f && im != null && !im.IsSprinting) sprintLocked = false;
        }

        //if (im != null && !im.IsSprinting) sprintLocked = false;
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
        StartHeightTween(characterController.height, crouchingHeight);
    }

    private void TryStand() {
        if (!playerAccessor.isCrouching || !CanStand()) return;
        playerAccessor.isCrouching = false;
        StartHeightTween(characterController.height, standingHeight);
    }

    private void StartHeightTween(float from, float to) {
        Tween.Custom(from, to, 0.1f, val => characterController.height = val);
    }

    private bool CanStand() {
        Vector3 bottom = transform.position + Vector3.up * characterController.radius;
        Vector3 top = transform.position + Vector3.up * (standingHeight - characterController.radius);
        int mask = ~(1 << LayerMask.NameToLayer("Player"));
        return !Physics.CheckCapsule(bottom, top, characterController.radius - 0.05f, mask, QueryTriggerInteraction.Ignore);
    }
    #endregion*/
}