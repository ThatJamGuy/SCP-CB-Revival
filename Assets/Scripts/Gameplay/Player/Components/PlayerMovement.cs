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
    //[SerializeField] private Transform playerCameraRoot;

    private static Player player => Player.Instance;
    private static InputManager inputManager => InputManager.Instance;

    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private Vector2 moveDirection;
    private Vector3 velocity;

    private bool grounded;
    private bool sprintLocked;

    private float walkingSpeed;
    private float sprintSpeed;
    private float crouchSpeed;

    private const float GRAVITY = -9.81f;
    private const float STANDING_HEIGHT = 2.2f;
    private const float CROUCHING_HEIGHT = 0.5f;

    private void Start() {
        moveAction = inputManager.GetAction("Player", "Move");
        sprintAction = inputManager.GetAction("Player", "Sprint");
        crouchAction = inputManager.GetAction("Player", "Crouch");
        
        walkingSpeed = player.walkSpeed;
        sprintSpeed = player.sprintSpeed;
        crouchSpeed = player.crouchSpeed;
    }

    private void Update() {
        if (player.disableInput) return;
        
        HandleMovement();
        
        //if (im != null && im.IsCrouchTriggered) AttemptToggleCrouch();
    }
    
    private void HandleMovement() {
        grounded = characterController.isGrounded; 
        if (grounded && velocity.y < 0f) velocity.y = -2f;
        
        //HandleStamina();

        //float currentSpeed = playerAccessor.isCrouching ? crouchSpeed : walkSpeed;
        //bool wantsToSprint = playerAccessor.isSprinting;
        //bool canSprint = !playerAccessor.isCrouching && (playerAccessor.infiniteStamina || stamina > 0f) && playerAccessor.isMoving && !sprintLocked;

        //if (wantsToSprint && canSprint) currentSpeed = sprintSpeed;
        
        //IsActuallySprinting = wantsToSprint && canSprint;

        //Vector3 move = GetMoveDirection(true) * currentSpeed;
        velocity.y += GRAVITY * Time.deltaTime;
        //characterController.Move((move + velocity) * Time.deltaTime);
    }

    /*private Vector3 GetMoveDirection(bool flattenY) {
        var im = InputManager.Instance;
        //Vector2 input = im != null ? im.Move : Vector2.zero;

        Vector3 forward = playerCameraRoot.forward;
        Vector3 right = playerCameraRoot.right;

        if (flattenY) {
            forward.y = 0f;
            right.y = 0f;
        }

        Vector3 move = forward.normalized * input.y + right.normalized * input.x;
        if (move.sqrMagnitude > 1f) move.Normalize();
        return move;
    }*/

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