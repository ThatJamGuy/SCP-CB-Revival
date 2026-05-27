using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using FMODUnity;

/// <summary>
/// Handles a draggable lever interactable using player look input.
/// </summary>
public class Lever : MonoBehaviour, IHoldInteractable {
    [Header("Lever Handle Settings")]
    [SerializeField] private GameObject leverHandleObject;
    [SerializeField] private float handleRotationSpeed = 100f;
    [SerializeField] private float minRotationAngle = -90f;
    [SerializeField] private float maxRotationAngle = 90f;

    [Header("Lever State Settings")]
    [SerializeField] private float leverOffAngle = -90f;
    [SerializeField] private float leverOnAngle = 90f;
    [SerializeField] private float leverOnOffThreshold = 5f;

    [Header("Lever Events")]
    public UnityEvent OnLeverTurnedOn;
    public UnityEvent OnLeverTurnedOff;

    [Header("Lever Audio")]
    [SerializeField] private EventReference leverStartMovingSound;
    [SerializeField] private EventReference leverStopMovingSound;

    private InputAction lookAction;

    private float currentHandleXRotation;

    private bool cantFunction;
    private bool isBeingUsed;
    private bool lastState;
    private bool leverTurnedOn;
    private bool hasPlayedMoveSound;

    #region Unity Callbacks

    private void Awake() {
        // Cache the starting local X rotation of the handle
        currentHandleXRotation = NormalizeAngle(
            leverHandleObject.transform.localEulerAngles.x
        );
    }

    private void Start() {
        // If there is no InputManager available at startup, disable lever functionality and print a warning
        if (InputManager.Instance == null) {
            cantFunction = true;

            Debug.Log(
                "<color=red>[Lever]</color> InputManager was not found, so levers will not function."
            );

            return;
        }

        // Get the player's look action from the InputManager
        lookAction = InputManager.Instance.GetAction("Player", "Look");
    }

    private void Update() {
        // Goodbye functionality :(
        if (cantFunction) return;

        // While interacting, allow the player to move the lever
        if (isBeingUsed) {
            HandleLeverMovement();
        }

        // Continuously check if the lever reached an ON/OFF state
        UpdateLeverState();
        
        if (Player.Instance.isMoving) ForceStopInteract();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Handles moving the lever based on mouse/controller Y movement.
    /// </summary>
    private void HandleLeverMovement() {
        // Read vertical look input and scale it by rotation speed
        float input =
            lookAction.ReadValue<Vector2>().y *
            handleRotationSpeed *
            Time.deltaTime;

        // Ignore tiny inputs to prevent accidental movement/audio spam
        if (Mathf.Abs(input) < 0.001f)
            return;

        // Play the movement start sound once when the lever begins moving
        if (!hasPlayedMoveSound) {
            hasPlayedMoveSound = true;
            AudioManager.PlayOneShot(leverStartMovingSound, transform.position);
        }

        // Accumulate lever rotation and clamp within allowed range
        currentHandleXRotation += input;

        currentHandleXRotation = Mathf.Clamp(
            currentHandleXRotation,
            minRotationAngle,
            maxRotationAngle
        );

        // Apply the rotation locally so parent rotation does not interfere
        leverHandleObject.transform.localRotation =
            Quaternion.Euler(currentHandleXRotation, 0f, 0f);
    }

    /// <summary>
    /// Checks whether the lever has reached the ON or OFF position.
    /// </summary>
    private void UpdateLeverState() {
        // Check if the lever is close enough to the ON angle
        if (Mathf.Abs(currentHandleXRotation - leverOnAngle) <= leverOnOffThreshold) {
            leverTurnedOn = true;

            // Prevent repeatedly firing events every frame
            if (lastState == leverTurnedOn)
                return;

            lastState = leverTurnedOn;

            // Play the stop movement sound once the lever reaches a final state
            AudioManager.PlayOneShot(leverStopMovingSound, transform.position);

            // Allow the movement sound to play again next time the lever moves
            hasPlayedMoveSound = false;

            OnLeverTurnedOn?.Invoke();
        }
        // Check if the lever is close enough to the OFF angle
        else if (Mathf.Abs(currentHandleXRotation - leverOffAngle) <= leverOnOffThreshold) {
            leverTurnedOn = false;

            // Prevent repeatedly firing events every frame
            if (lastState == leverTurnedOn)
                return;

            lastState = leverTurnedOn;

            // Play the stop movement sound once the lever reaches a final state
            AudioManager.PlayOneShot(leverStopMovingSound, transform.position);

            // Allow the movement sound to play again next time the lever moves
            hasPlayedMoveSound = false;

            OnLeverTurnedOff?.Invoke();
        }
    }

    /// <summary>
    /// Converts a wrapped Euler angle into a signed angle.
    /// Example: 350 becomes -10.
    /// </summary>
    private float NormalizeAngle(float angle) {
        angle %= 360f;

        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
    #endregion

    #region Public Methods

    public void BeginInteract(PlayerInteraction playerInteraction) {
        Player.Instance.disableLooking = true;
        isBeingUsed = true;
    }

    public void EndInteract(PlayerInteraction playerInteraction) {
        Player.Instance.disableLooking = false;
        hasPlayedMoveSound = false;
        isBeingUsed = false;
    }
    
    public void ForceStopInteract() {
        Player.Instance.disableLooking = false;
        hasPlayedMoveSound = false;
        isBeingUsed = false;
    }

    #endregion
}