using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using FMODUnity;
using UnityEngine.AI;

/// <summary>
/// Script to handle doors that follow the sliding left-right movements
/// </summary>
public class Door : MonoBehaviour {
    public enum Axis { X, Y, Z }

    [Header("Door Settings")]
    public bool startOpen;
    [HideInInspector] public bool isOpen;
    public bool isLocked;
    public bool isBroken;
    public bool requiresKeycard;
    public int requiredKeyLevel;

    [SerializeField] private float openSpeed = 1.5f;
    [SerializeField] private float openDistance = 1.9f;

    [Header("Audio")]
    [SerializeField] private EventReference doorSoundEvent;

    [Header("Events")]
    public UnityEvent onDoorOpening;
    public UnityEvent onDoorClosing;
    public UnityEvent onDoorOpened;
    public UnityEvent onDoorClosed;

    [Header("Movement Settings")]
    public Axis moveAxis = Axis.X;

    [Header("Door Parts")]
    public GameObject doorPart01;
    public GameObject doorPart02;
    public GameObject doorPhys01;
    public GameObject doorPhys02;

    private Coroutine moveRoutine;
    
    private Vector3 door01InitialPos, door02InitialPos, door01Target, door02Target;
    private Vector3 offset;

    private bool isTransitioning;

    #region Unity Callbacks
    private void Start() {
        // Set the initial positions for the doors so they know where to slide back to when closing
        door01InitialPos = doorPart01.transform.position;
        door02InitialPos = doorPart02.transform.position;
        
        offset = GetOffset(openDistance); // Determine the offset the doors will be moved by when opening
        door01Target = door01InitialPos + offset; // Determine the target position for the first door object
        door02Target = door02InitialPos - offset; // Determine the target position for the second door object

        // If the door is to start open, set the doors positions to the open spots and set variables
        if (!startOpen) return;
        doorPart01.transform.position = door01InitialPos + offset;
        doorPart02.transform.position = door02InitialPos - offset;
        isOpen = true;
    }
    #endregion

    #region Private Methods
    // Actually trigger the movement for the door, passing in targets for both doors and the onComplete event
    private void StartMove(Vector3 door1Target, Vector3 door2Target, UnityEvent onComplete) {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveDoors(door1Target, door2Target, onComplete));
    }

    private IEnumerator MoveDoors(Vector3 target1, Vector3 target2, UnityEvent onComplete) {
        // Set some necessary variables
        var start1 = doorPart01.transform.position; // Starting pos for door01
        var start2 = doorPart02.transform.position; // Starting pos for door02
        var dist = Vector3.Distance(start1, target1); // Calculated distance between the start and the end
        var duration = dist / openSpeed; // Calculated movement duration
        var time = 0f; // Time elapsed

        // Immediately start playing the sound so it isn't delayed by the stuff below
        PlaySound();
        
        // While the elapsed time is less than the calculated curation...
        while (time < duration) {
            var factor = Mathf.SmoothStep(0, 1, time / duration);
            doorPart01.transform.position = Vector3.Lerp(start1, target1, factor);
            doorPart02.transform.position = Vector3.Lerp(start2, target2, factor);
            time += Time.deltaTime;
            yield return null;
        }
        
        doorPart01.transform.position = target1;
        doorPart02.transform.position = target2;
        onComplete?.Invoke();
        moveRoutine = null;
    }

    private void PlaySound() {
        // Set the doorStateValue based on the isOpen bool. Then play the door sound passing in the doorState as param
        var doorStateValue = isOpen ? 0 : 1;
        AudioManager.PlayOneShot(doorSoundEvent, transform.position, "DoorState", doorStateValue);
    }

    private Vector3 GetOffset(float distance) {
        return moveAxis switch { // Return moveAxis, determined by the doors move axis * preset distance
            Axis.Y => transform.up * distance, // Y-Axis
            Axis.Z => transform.forward * distance, // Z-Axis
            _ => transform.right * distance, // X-Axis
        };
    }
    #endregion
    
    #region Public Methods
    /// <summary>
    /// Toggle the lock state of the door to the opposite of what it currently is
    /// </summary>
    public void ToggleLockState() => isLocked = !isLocked;

    /// <summary>
    /// Toggle the open/close state of the door to the opposite of what it currently is
    /// </summary>
    public void ToggleDoorState() {
        if (isOpen) CloseDoor();
        else OpenDoor();
    }

    /// <summary>
    /// Trigger the door to open, and it will, assuming it's not already open/opening
    /// </summary>
    public void OpenDoor() {
        // If the door is already open, locked, or the move coroutine exists and is active already then do nothing
        if (isOpen || isLocked || isBroken || moveRoutine != null || isTransitioning) return;

        isTransitioning = true;
        isOpen = true;
        
        // Trigger the related event and start opening the door
        onDoorOpening?.Invoke();
        StartMove(door01Target, door02Target, onDoorOpened);
        isTransitioning = false;
    }

    /// <summary>
    /// Trigger the door to close, and it will, assuming it's not already closed/closing
    /// </summary>
    public void CloseDoor() {
        // If the door is not open or the move coroutine exists and is active already then do nothing
        if (!isOpen || moveRoutine != null || isTransitioning) return;
        
        isTransitioning = true;
        isOpen = false;
        
        // Trigger the related event and start closing the door
        onDoorClosing?.Invoke();
        StartMove(door01InitialPos, door02InitialPos, onDoorClosed);
        isTransitioning = false;
    }

    /// <summary>
    /// Enables gravity on the physics door children and applies force in a specified direction
    /// </summary>
    public void EnableGravityOnDoors(Vector3 explosionDirection, float explosionForce = 50f) {
        //TODO: FIX THIS (IT DOES NOT WORK RIGHT NOW BUT IT DOES ALLOW 096 RIGHT OF PASSAGE SO KEEPING IT LIKE THIS FOR NOW)

        isBroken = true;

        doorPart01.SetActive(false);
        doorPart02.SetActive(false);
        doorPhys01.SetActive(true);
        doorPhys02.SetActive(true);

        Rigidbody rbPhys01 = doorPhys01.GetComponent<Rigidbody>();
        Rigidbody rbPhys02 = doorPhys02.GetComponent<Rigidbody>();

        rbPhys01.AddForce(explosionDirection * explosionForce);
        rbPhys02.AddForce(explosionDirection * explosionForce);
    }
    #endregion
}