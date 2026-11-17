using PrimeTween;
using System.Reflection.Metadata.Ecma335;
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

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform playerCameraRoot;
    [SerializeField] private CapsuleCollider capsuleCollider;

    [Header("Crouching Camera")]
    [SerializeField] private float crouchingHeight = 0.9f;
    [SerializeField] private float crouchCameraDrop = 1f;
    private Vector3 defaultCameraLocalPos;
    private Vector3 defaultCenter;

    #region Private Variables
    private static Player player => Player.Instance;
    private static RevivalRuntimeEngine runtimeEngine => RevivalRuntimeEngine.Instance;

    private static readonly RaycastHit[] hitBuffer = new RaycastHit[4];
    private static int playerLayerMask = -1;

    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction crouchAction;

    private Vector3 velocity;

    private Slider currStaminaSlider;

    private bool cantFunction;
    private bool isSprinting;
    private bool isCrouching;
    private bool sprintLocked;

    private Tween heightTween;

    private float currentSpeed;
    private float walkingSpeed;
    private float sprintSpeed;
    private float crouchSpeed;

    private const float GRAVITY = -9.81f;
    private const float STANDING_HEIGHT = 2.45f;
    private const float MAX_STAMINA = 1;


    #endregion

    #region Unity Lifecycle

    private void OnDisable() {
        // Cleanup to make sure we unsubscribe to the sprint and crouch actions
        if (sprintAction == null || crouchAction == null) return;
        sprintAction.started -= OnSprintStarted;
        sprintAction.canceled -= OnSprintCanceled;
        crouchAction.performed -= OnCrouchPerformed;
    }

    private void Start()
    {
        // If there is no InputManager available at the start, disallow functionality and print a warning in console
        if (runtimeEngine == null)
        {
            cantFunction = true;
            Debug.Log("<color=red>[PlayerMovement]</color> InputManager was not found, moving will not work!");

            return;
        }

        // Retrieve the different input actions from InputManager so they can be cached
        moveAction = runtimeEngine.GetAction("Player", "Move");
        sprintAction = runtimeEngine.GetAction("Player", "Sprint");
        crouchAction = runtimeEngine.GetAction("Player", "Crouch");

        // Subscribe to the sprint and crouch actions so it can determine if the player is holding the right keys
        sprintAction.started += OnSprintStarted;
        sprintAction.canceled += OnSprintCanceled;
        crouchAction.performed += OnCrouchPerformed;

        // Set the different speeds to their values from settings.json, which are stored on the player instance
        walkingSpeed = player.walkSpeed;
        sprintSpeed = player.sprintSpeed;
        crouchSpeed = player.crouchSpeed;

        // Default the slider to the chosen one in CanvasInstance based on the players settings
        currStaminaSlider = CanvasInstance.Instance.currSprintSlider;

        // caches default camera height
        if (playerCameraRoot != null)
        {
            defaultCameraLocalPos = playerCameraRoot.localPosition;
        }

        if (capsuleCollider != null)
        {
            capsuleCollider.center = characterController.center;
            capsuleCollider.height = characterController.height;
            capsuleCollider.radius = characterController.radius;
        }


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
        Vector2 input = moveAction.ReadValue<Vector2>();

        if (characterController.isGrounded && velocity.y < 0f) velocity.y = -2f;
        velocity.y += GRAVITY * Time.deltaTime;

        Vector3 move = (transform.right * input.x + transform.forward * input.y) * currentSpeed;
        move.y = velocity.y;

        characterController.Move(move * Time.deltaTime);

        player.isMoving = input.sqrMagnitude > 0f;
    }

    private void HandleStamina() {
        // make sure the stamina slider design is up to date with the chosen one in the player settings during runtime
        if (currStaminaSlider != CanvasInstance.Instance.currSprintSlider)
            currStaminaSlider = CanvasInstance.Instance.currSprintSlider;

        currStaminaSlider.value = currentStamina / MAX_STAMINA;

        // If the player is moving, sprinting, and not sprint locked then do the stuff in this if statement
        if (isSprinting && !sprintLocked && player.isMoving) {
            // Determine the total drain rate based on the default drain rate * the depletion modifier if applicable
            // Then subtract the current stamina based on this determined drain rate
            float finalDrainRate = staminaDrainRate * (1f + player.staminaDepletionModifier);
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
            if (currentStamina > 0.1f) sprintLocked = false;
        }
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
        TweenHeight(characterController.height, crouchingHeight);
        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.crouchFoley, transform.position);
    }

    // If not crouching already and the player cannot stand, then do nothing
    // Otherwise set crouching to false and tween the player height back to the standing height
    // Also play some crouch foley for some extra cool detail
    private void TryToStand() {
        if (!isCrouching || !CanStand()) return;
        isCrouching = false;
        player.isCrouching = false;
        TweenHeight(characterController.height, STANDING_HEIGHT);
        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.crouchFoley, transform.position);
    }

    // Method that uses PrimeTween to smoothly interpolate between standing and crouching heights
    private void TweenHeight(float from, float to)
    {
        heightTween.Stop();
        // Tween height and center.y together so the capsule stays grounded. Prevents that weird stuttering
        heightTween = Tween.Custom(from, to, 0.15f, val =>
        {
            float heightDifference = STANDING_HEIGHT - val;
            Vector3 newCenter = defaultCenter;
            newCenter.y -= heightDifference * 0.5f;

            characterController.height = val;
            characterController.center = newCenter;


            //keeps capscule collider grounded
            if (capsuleCollider != null)
            {
                capsuleCollider.height = val;
                capsuleCollider.center = newCenter;
            }

            //Smoothly lerps camera root
            if (playerCameraRoot != null)
            {
                float t = Mathf.InverseLerp(STANDING_HEIGHT, crouchingHeight, val);
                playerCameraRoot.localPosition = defaultCameraLocalPos + (Vector3.down * (crouchCameraDrop * t));
            }
        });
    }

    // Some more black magic to determine if the player can stand or not
    // Checks for any layer collisions above the player by looking above the player for any layers that aren't on
    // the same Player layer, then returning true or false depending on if there is

    //Note: Now Uses SphereCastNonAlloc from the top of crouched to standing    
    private bool CanStand()
    {
        float distance = STANDING_HEIGHT - characterController.height;
        if (distance <= 0.01f) return true;

        if (playerLayerMask == -1)
        {
            playerLayerMask = ~(1 << LayerMask.NameToLayer("Player"));
        }

        // finds top of your "head" when crouched
        float LocalTop = characterController.center.y + (characterController.height * 0.5f);
        Vector3 headCenter = characterController.transform.TransformPoint(new Vector3(0f, LocalTop, 0f));

        float checkRadius = 0.1f;

        int hitCount = Physics.SphereCastNonAlloc(
            headCenter,
            checkRadius,
            Vector3.up,
            hitBuffer,
            distance,
            playerLayerMask,
            QueryTriggerInteraction.Ignore
            );

        for (int i = 0; i < hitCount; i++)
        {

            RaycastHit hit = hitBuffer[i];

            //Ignores player own collider
            if (hit.collider.transform.root == characterController.transform.root) continue;

            if (hit.normal.y < -0.3f)
            {
                return false;
            }

        }
        return true;
    }
    
    #endregion

    #endregion
    }