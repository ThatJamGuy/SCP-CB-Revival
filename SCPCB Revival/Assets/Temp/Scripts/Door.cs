using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour {
    public enum Axis { X, Y, Z }

    [Header("Door Settings")]
    public bool isOpen;
    public bool isLocked;
    public bool isBroken;
    public bool requiresKeycard;
    public int requiredKeyLevel;
    public float openSpeed = 1f;
    public float openDistance = 1f;

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

    [Header("Audio")]
    public AudioSource doorAudio;
    public AudioClip[] doorOpenSFX;
    public AudioClip[] doorCloseSFX;

    Vector3 door01InitialPos, door02InitialPos;
    Coroutine moveRoutine;

    void Start() {
        door01InitialPos = doorPart01.transform.position;
        door02InitialPos = doorPart02.transform.position;
    }

    public void ToggleLockState() => isLocked = !isLocked;

    public void ToggleDoorState() {
        if (isOpen) CloseDoor();
        else OpenDoor();
    }

    public void OpenDoor() {
        if (isOpen || isLocked || isBroken || moveRoutine != null) return;
        onDoorOpening?.Invoke();
        Vector3 offset = GetOffset(openDistance);
        Vector3 door01Target = door01InitialPos + offset;
        Vector3 door02Target = door02InitialPos - offset;
        StartMove(door01Target, door02Target, doorOpenSFX, onDoorOpened);
        isOpen = true;
    }

    public void CloseDoor() {
        if (!isOpen || moveRoutine != null) return;
        onDoorClosing?.Invoke();
        StartMove(door01InitialPos, door02InitialPos, doorCloseSFX, onDoorClosed);
        isOpen = false;
    }

    void StartMove(Vector3 door1Target, Vector3 door2Target, AudioClip[] sfx, UnityEvent onComplete) {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveDoors(door1Target, door2Target, sfx, onComplete));
    }

    IEnumerator MoveDoors(Vector3 target1, Vector3 target2, AudioClip[] sfx, UnityEvent onComplete) {
        PlaySound(sfx);
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

    Vector3 GetOffset(float distance) {
        return moveAxis switch {
            Axis.Y => transform.up * distance,
            Axis.Z => transform.forward * distance,
            _ => transform.right * distance,
        };
    }

    void PlaySound(AudioClip[] clips) {
        if (doorAudio && clips.Length > 0)
            doorAudio.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}