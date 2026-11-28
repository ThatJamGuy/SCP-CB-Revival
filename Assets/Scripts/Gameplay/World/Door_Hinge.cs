using FMODUnity;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;

public class Door_Hinge : MonoBehaviour {
    [Header("State")]
    public bool isOpen;
    public bool locked;
    public bool requiresKeycard;

    [Header("Settings")]
    [SerializeField] private float moveTime;
    [SerializeField] private float maxPositiveAngle;
    [SerializeField] private float maxNegativeAngle;

    [Header("Audio")]
    [SerializeField] private EventReference openSound;
    [SerializeField] private EventReference closeSound;
    [SerializeField] private EventReference handleSound;

    [Header("Events")]
    public UnityEvent onDoorOpening;
    public UnityEvent onDoorClosing;
    public UnityEvent onDoorOpened;
    public UnityEvent onDoorClosed;

    [Header("References")]
    [SerializeField] private Transform hinge;
    [SerializeField] private Animator handleAnimator;

    private bool isMoving;
    private float targetAngle;

    #region Unity Lifecycle

    #endregion

    #region Private Methods

    private void OnCompleteOpen() {
        isMoving = false;
        isOpen = true;
        onDoorOpened.Invoke();
    }

    private void OnCompleteClose() {
        isMoving = false;
        isOpen = false;
        onDoorClosed.Invoke();
    }

    #endregion

    #region Public Methods

    // Open the door away from the players position
    public void OpenDoor(bool notByPlayer = false, Transform source = null) {
        handleAnimator.Play("CB_HandlePivot_Use");
        AudioManager.PlayOneShot(handleSound, handleAnimator.transform.position);

        if (locked || requiresKeycard) RevivalSessionEngine.Instance.NotifyPlayer("The door seems to be locked.");
        if (isMoving || isOpen || locked) return;

        if (notByPlayer && source != null) {
            // Check which side the source is on and set the targetAngle to a defined angle that faces away
            Vector3 toSource = source.position - transform.position;
            float sourcesSide = Vector3.Dot(transform.forward, toSource);
            targetAngle = sourcesSide >= 0 ? maxPositiveAngle : maxNegativeAngle;
        } else {
            // Check which side the player is on and set the targetAngle to a defined angle that faces away
            Vector3 toPlayer = Player.Instance.transform.position - transform.position;
            float side = Vector3.Dot(transform.forward, toPlayer);
            targetAngle = side >= 0 ? maxPositiveAngle : maxNegativeAngle;
        }

        Tween.LocalRotation(hinge, endValue: Quaternion.Euler(0, targetAngle, 0), duration: moveTime)
            .OnComplete(() => OnCompleteOpen());

        AudioManager.PlayOneShot(openSound, transform.position, "DoorState", 0);

        onDoorOpening.Invoke();
        isMoving = true;
    }

    public void CloseDoor() {
        // Close the door back to it's original rotation (Usually 0,0,0)
        // Ignores players current position
        if (isMoving || !isOpen) return;

        //handleAnimator.Play("CB_HandlePivot_Use");
        //AudioManager.PlayOneShot(handleSound, handleAnimator.transform.position);

        Tween.LocalRotation(hinge, endValue: Quaternion.Euler(0, 0, 0), duration: moveTime)
            .OnComplete(() => OnCompleteClose());

        AudioManager.PlayOneShot(closeSound, transform.position, "DoorState", 1);

        onDoorClosing.Invoke();
        isMoving = true;
    }

    public void ToggleDoorState() {
        if (isOpen) CloseDoor();
        else OpenDoor();
    }

    #endregion
}