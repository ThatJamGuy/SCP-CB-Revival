using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;

/// <summary>
/// Player component that handles all the movement logic. Also pulls a bunch of values from the Player class.
/// </summary>
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
    private bool isSprinting;
    private bool isCrouching;

    private float currentSpeed;
    private float walkingSpeed;
    private float sprintSpeed;
    private float crouchSpeed;

    private const float GRAVITY = -9.81f;
    private const float STANDING_HEIGHT = 2.2f;
    private const float CROUCHING_HEIGHT = 0.5f;

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

    private void OnSprintStarted(InputAction.CallbackContext ctx) {
        if (isCrouching) return;
        isSprinting = true;
        player.isSprinting = true;
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx) {
        isSprinting = false;
        player.isSprinting = false;
    }

    private void OnCrouchPerformed(InputAction.CallbackContext ctx) => AttemptToToggleCrouch();

    #region Crouching
    // When hitting the crouch button, if crouching stand, otherwise start crouching
    private void AttemptToToggleCrouch() {
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

    /*#region Stamina
    private void HandleStamina() {
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
    #endregion*/
}