using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Player component that handles all the movement logic. Also pulls a bunch of values from the Player class.
/// </summary>
public class PlayerMovement : MonoBehaviour {
    [Header("Movement Status")]
    [SerializeField, Range(0, 1)] private float currentStamina = 1f;

    [Header("Stamina Settings")]
    [SerializeField] private float staminaDrainRate = 0.2f;
    [SerializeField] private float staminaRegenRate = 0.1f;
    [SerializeField] private Slider temporaryStaminaSlider; // TODO: REMOVE ME!!!

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform playerCameraRoot;

    #region Private Variables
    private static Player player => Player.Instance;
    private static InputManager inputManager => InputManager.Instance;

    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction crouchAction;

    private Vector2 moveDirection;
    private Vector3 velocity;

    private bool cantFunction;
    private bool isSprinting;
    private bool isCrouching;
    private bool sprintLocked;

    private float currentSpeed;
    private float walkingSpeed;
    private float sprintSpeed;
    private float crouchSpeed;

    private const float GRAVITY = -9.81f;
    private const float STANDING_HEIGHT = 2.2f;
    private const float CROUCHING_HEIGHT = 0.5f;
    private const float MAX_STAMINA = 1;
    #endregion

    #region Unity Callbacks

    private void OnDisable() {
        // Cleanup to make sure we unsubscribe to the sprint and crouch actions
        if (sprintAction == null || crouchAction == null) return;
        sprintAction.started -= OnSprintStarted;
        sprintAction.canceled -= OnSprintCanceled;
        crouchAction.performed -= OnCrouchPerformed;
    }

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

        // Subscribe to the sprint and crouch actions so it can determine if the player is holding the right keys
        sprintAction.started += OnSprintStarted;
        sprintAction.canceled += OnSprintCanceled;
        crouchAction.performed += OnCrouchPerformed;

        // Set the different speeds to their values from settings.json, which are stored on the player instance
        walkingSpeed = player.walkSpeed;
        sprintSpeed = player.sprintSpeed;
        crouchSpeed = player.crouchSpeed;
    }

    private void Update() {
        // If input is manually disabled or functionality is broken then don't do anything
        if (player.disableInput || cantFunction) return;

        DetermineMovementSpeed();
        HandleMovement();
        HandleStamina();
    }

    #endregion

    #region Private Methods

    private void DetermineMovementSpeed() {
        // Prioritize crouch speed, but if that doesn't check out then check for sprinting then walking
        if (isCrouching) currentSpeed = crouchSpeed;
        else currentSpeed = isSprinting ? sprintSpeed : walkingSpeed;
    }

    private void HandleMovement() {
        var input = moveAction.ReadValue<Vector2>();
        var move = transform.right * input.x + transform.forward * input.y;

        // Move the character controller in the proper Vector3 direction at the currentSpeed
        characterController.Move(move * (currentSpeed * Time.deltaTime));

        // If the player is not on the ground, set the up-down velocity to -2 so they go down
        if (characterController.isGrounded && velocity.y < 0f) velocity.y = -2f;

        // Set the velocity to the gravity value
        // Then move the character controller by the velocity alongside the previous Vector3 movement
        velocity.y += GRAVITY * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        player.isMoving = input.sqrMagnitude > 0;
    }

    private void HandleStamina() {
        temporaryStaminaSlider.value = currentStamina / MAX_STAMINA;

        // Remove dependance on the UI being in the scene
        if (temporaryStaminaSlider != null) {
            temporaryStaminaSlider.value = currentStamina / MAX_STAMINA;
        }

        // If the player is moving, sprinting, and not sprint locked then do the stuff in this if statement
        if (isSprinting && !sprintLocked && player.isMoving) {
            // Determine the total drain rate based on the default drain rate * the depletion modifier if applicable
            // Then subtract the current stamina based on this determined drain rate
            var finalDrainRate = staminaDrainRate * (1f + player.staminaDepletionModifier);
            currentStamina = Mathf.Max(currentStamina - finalDrainRate * Time.deltaTime, 0f);

            // Is the stamina more than 0? Too bad, go away
            if (!(currentStamina <= 0f)) return;

            // Okay I guess you have stamina still. I'll give you zero stamina on the dot, and stop you from sprinting
            currentStamina = 0f;
            sprintLocked = true;
            isSprinting = false;
            player.isSprinting = false;
        }
        // If the player is not moving, sprinting, or sprint locked then check if curr stamina is less than max stamina
        else if (currentStamina < MAX_STAMINA) {
            // Start refilling the stamina based on the staminaRegenRate
            currentStamina = Mathf.Min(currentStamina + staminaRegenRate * Time.deltaTime, MAX_STAMINA);

            // Wait until stamina is greater than 0.1 before player can sprint again. Presents sprint tapping
            if (currentStamina > 0.1f)
                sprintLocked = false;
        }

        //temporaryStaminaSlider.value = currentStamina / MAX_STAMINA;
    }

    #region Input Callbacks
    private void OnSprintStarted(InputAction.CallbackContext ctx) {
        if (isCrouching || sprintLocked) return;
        isSprinting = true;
        player.isSprinting = true;
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx) {
        isSprinting = false;
        player.isSprinting = false;
    }

    private void OnCrouchPerformed(InputAction.CallbackContext ctx) => AttemptToToggleCrouch();
    #endregion

    #region Crouching

    // When hitting the crouch button, if crouching stand, otherwise start crouching
    private void AttemptToToggleCrouch() {
        if (player.disableInput) return;
        if (isCrouching) TryToStand();
        else StartCrouch();
    }

    // If crouching already, don't attempt to crouch again. Important for that one frame where it could bug out
    // Assuming it continues, set the isCrouching variable to true and start tweening player height to crouching
    // Also play some crouch foley for some extra cool detail
    private void StartCrouch() {
        if (isCrouching) return;
        isCrouching = true;
        player.isCrouching = true;
        TweenHeight(characterController.height, CROUCHING_HEIGHT);
        AudioManager.PlayOneShot(AudioEventsHolder.Instance.crouchFoley, transform.position);
    }

    // If not crouching already and the player cannot stand, then do nothing
    // Otherwise set crouching to false and tween the player height back to the standing height
    // Also play some crouch foley for some extra cool detail
    private void TryToStand() {
        if (!isCrouching || !CanStand()) return;
        isCrouching = false;
        player.isCrouching = false;
        TweenHeight(characterController.height, STANDING_HEIGHT);
        AudioManager.PlayOneShot(AudioEventsHolder.Instance.crouchFoley, transform.position);
    }

    // Method that uses PrimeTween to smoothly interpolate between standing and crouching heights
    private void TweenHeight(float from, float to) {
        // Tween height and center.y together so the capsule stays grounded. Prevents that weird stuttering
        Tween.Custom(from, to, 0.1f, val => characterController.height = val);
    }

    // Some more black magic to determine if the player can stand or not
    // Checks for any layer collisions above the player by looking above the player for any layers that aren't on
    // the same Player layer, then returning true or false depending on if there is
    private bool CanStand() {
        var playerRadius = characterController.radius;
        var playerBottom = transform.position + Vector3.up * playerRadius;
        var playerTop = transform.position + Vector3.up * (STANDING_HEIGHT - playerRadius);
        var mask = ~(1 << LayerMask.NameToLayer("Player"));
        return !Physics.CheckCapsule(playerBottom, playerTop, playerRadius - 0.05f, mask, QueryTriggerInteraction.Ignore);
    }

    #endregion

    #endregion
}