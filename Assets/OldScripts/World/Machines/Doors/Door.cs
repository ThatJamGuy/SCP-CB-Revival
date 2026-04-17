using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

public class Door : MonoBehaviour {
    public enum Axis { X, Y, Z }

    [Header("Door Settings")]
    public bool startOpen;
    public bool isOpen;
    public bool isLocked;
    public bool requiresKeycard;
    public int requiredKeyLevel;

    [SerializeField] private float openSpeed = 1.5f;
    [SerializeField] private float openDistance = 1.9f;

    [Header("FMOD Audio")]
    [SerializeField] private EventReference doorEventReference;

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

    private Vector3 door01InitialPos, door02InitialPos;
    private Coroutine moveRoutine;

    void Start() {
        door01InitialPos = doorPart01.transform.position;
        door02InitialPos = doorPart02.transform.position;

        if (startOpen) ToggleDoorState();
    }

    public void ToggleLockState() => isLocked = !isLocked;

    public void ToggleDoorState() {
        if (isOpen) CloseDoor();
        else OpenDoor();
    }

    public void OpenDoor() {
        if (isOpen || isLocked || moveRoutine != null) return;
        onDoorOpening?.Invoke();
        Vector3 offset = GetOffset(openDistance);
        Vector3 door01Target = door01InitialPos + offset;
        Vector3 door02Target = door02InitialPos - offset;
        StartMove(door01Target, door02Target, onDoorOpened);
        isOpen = true;
    }

    public void CloseDoor() {
        if (!isOpen || moveRoutine != null) return;
        onDoorClosing?.Invoke();
        StartMove(door01InitialPos, door02InitialPos, onDoorClosed);
        isOpen = false;
    }

    void StartMove(Vector3 door1Target, Vector3 door2Target, UnityEvent onComplete) {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveDoors(door1Target, door2Target, onComplete));
    }

    IEnumerator MoveDoors(Vector3 target1, Vector3 target2, UnityEvent onComplete) {
        PlaySound();
        Vector3 start1 = doorPart01.transform.position;
        Vector3 start2 = doorPart02.transform.position;
        float dist = Vector3.Distance(start1, target1);
        float duration = dist / openSpeed;
        float t = 0f;

        while (t < duration) {
            float factor = Mathf.SmoothStep(0, 1, t / duration);
            doorPart01.transform.position = Vector3.Lerp(start1, target1, factor);
            doorPart02.transform.position = Vector3.Lerp(start2, target2, factor);
            t += Time.deltaTime;
            yield return null;
        }

        doorPart01.transform.position = target1;
        doorPart02.transform.position = target2;
        onComplete?.Invoke();
        moveRoutine = null;
    }

    void PlaySound()
    {
        if (doorEventReference.Guid.IsNull)
            return;

        var instance = AudioManager.instance.CreateInstance(doorEventReference);
        if (!instance.isValid())
            return;

        var attributes = RuntimeUtils.To3DAttributes(transform.position);
        instance.set3DAttributes(attributes);

        float doorStateValue = isOpen ? 1f : 0f;
        instance.setParameterByName("DoorState", doorStateValue);

        instance.start();
        instance.release();
    }

    Vector3 GetOffset(float distance) {
        return moveAxis switch {
            Axis.Y => transform.up * distance,
            Axis.Z => transform.forward * distance,
            _ => transform.right * distance,
        };
    }
}