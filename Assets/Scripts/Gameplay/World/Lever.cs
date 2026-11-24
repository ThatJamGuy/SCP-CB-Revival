using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

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
    private bool leverTurnedOn;
    private bool hasPlayedMoveSound;

    #region Unity Lifecycle

    private void Awake() {
        // Cache the starting local X rotation of the handle
        currentHandleXRotation = NormalizeAngle(leverHandleObject.transform.localEulerAngles.x);
        leverTurnedOn = Mathf.Abs(currentHandleXRotation - leverOnAngle) <= leverOnOffThreshold;
    }

    private void Start() {
        // If there is no InputManager available at startup, disable lever functionality and print a warning
        if (RevivalRuntimeEngine.Instance == null) {
            cantFunction = true;

            Debug.Log(
                "<color=red>[Lever]</color> InputManager was not found, so levers will not function."
            );

            return;
        }

        // Get the player's look action from the InputManager
        lookAction = RevivalRuntimeEngine.Instance.GetAction("Player", "Look");
    }

    private void Update() {
        // Goodbye functionality :(
        if (cantFunction) return;

        // While interacting, allow the player to move the lever
        if (isBeingUsed) {
            // Stop if the player starts walking
            if (Player.Instance.isMoving) {
                ForceStopInteract();
                return;
            }

            HandleLeverMovement();
        }
    }

    #endregion

    #region Private Methods

    private void HandleLeverMovement() {
        // Read vertical look input and scale it by rotation speed
        float input = lookAction.ReadValue<Vector2>().y * handleRotationSpeed * Time.deltaTime;

        // Ignore tiny inputs to prevent accidental movement/audio spam
        if (Mathf.Abs(input) < 0.001f) return;

        float previousRotation = currentHandleXRotation;
        float newRotation = Mathf.Clamp(previousRotation + input, minRotationAngle, maxRotationAngle);

        if (Mathf.Approximately(newRotation, previousRotation)) return;

        currentHandleXRotation = newRotation;

        // Play the movement start sound once when the lever begins moving
        if (!hasPlayedMoveSound && !IsInEndZone(newRotation)) {
            hasPlayedMoveSound = true;
            AudioManager.PlayOneShot(leverStartMovingSound, transform.position);
        }

        // Apply the rotation locally so parent rotation does not interfere
        leverHandleObject.transform.localRotation = Quaternion.Euler(currentHandleXRotation, 0f, 0f);

        // Only evaluate ON/OFF transitions when the lever actually moved
        UpdateLeverState();
    }

    private void UpdateLeverState() {
        bool reachedOn = Mathf.Abs(currentHandleXRotation - leverOnAngle) <= leverOnOffThreshold;
        bool reachedOff = !reachedOn && Mathf.Abs(currentHandleXRotation - leverOffAngle) <= leverOnOffThreshold;

        // Not in either end zone, nothing to do
        if (!reachedOn && !reachedOff) return;

        // Already in this state, prevent repeated firing
        if (leverTurnedOn == reachedOn) return;

        leverTurnedOn = reachedOn;

        // Play the stop sound once the lever reaches a final state
        AudioManager.PlayOneShot(leverStopMovingSound, transform.position);

        // Allow the movement sound to play again next time the lever moves
        hasPlayedMoveSound = false;

        if (reachedOn) {
            OnLeverTurnedOn?.Invoke();
        } else {
            OnLeverTurnedOff?.Invoke();
        }
    }

    /// <summary>
    /// Converts a wrapped Euler angle into a signed angle.
    /// Example: 350 becomes -10.
    /// </summary>
    private float NormalizeAngle(float angle) {
        angle %= 360f;

        if (angle > 180f) angle -= 360f;
        return angle;
    }

    private bool IsInEndZone(float angle) {
        return Mathf.Abs(angle - leverOnAngle) <= leverOnOffThreshold ||
               Mathf.Abs(angle - leverOffAngle) <= leverOnOffThreshold;
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