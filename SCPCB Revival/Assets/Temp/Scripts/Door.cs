using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isOpen;
    public bool isLocked;
    public bool isBroken;
    public bool requiresKeycard;
    public int requiredKeyLevel;
    public float regularOpenCloseSpeed;
    public float regularOpenCloseDistance;

    public enum Axis { X, Y, Z }
    [Header("Movement Settings")]
    public Axis moveAxis = Axis.X;

    [Header("Door Objects")]
    public GameObject doorPart01;
    public GameObject doorPart02;

    [Header("Audio")]
    public AudioSource doorAudio;
    public AudioClip[] doorOpenSFX;
    public AudioClip[] doorCloseSFX;

    private Vector3 door01InitialPosition;
    private Vector3 door01TargetPosition;
    private Vector3 door02InitialPosition;
    private Vector3 door02TargetPosition;
    private bool isOpening = false;

    private void Start()
    {
        door01InitialPosition = doorPart01.transform.position;
        door02InitialPosition = doorPart02.transform.position;
    }

    public void ToggleLockState()
    {
        isLocked = !isLocked;
    }

    public void OpenDoor()
    {
        if (!isOpening && !isOpen)
        {
            Vector3 offset = GetMovementOffset(regularOpenCloseDistance);
            door01TargetPosition = door01InitialPosition + offset;
            door02TargetPosition = door02InitialPosition - offset;
            StartCoroutine(SlideDoor(doorPart01, door01TargetPosition, regularOpenCloseSpeed));
            StartCoroutine(SlideDoor(doorPart02, door02TargetPosition, regularOpenCloseSpeed));
            PlayDoorSound(doorOpenSFX);
            isOpen = true;
        }
    }

    public void CloseDoor()
    {
        if (gameObject.activeSelf)
        {
            if (isOpen && !isOpening)
            {
                door01TargetPosition = door01InitialPosition;
                door02TargetPosition = door02InitialPosition;
                StartCoroutine(SlideDoor(doorPart01, door01TargetPosition, regularOpenCloseSpeed));
                StartCoroutine(SlideDoor(doorPart02, door02TargetPosition, regularOpenCloseSpeed));
                PlayDoorSound(doorCloseSFX);
                isOpen = false;
            }
        }
    }

    public void ToggleDoorState() {
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    private Vector3 GetMovementOffset(float distance)
    {
        switch (moveAxis)
        {
            case Axis.Y: return transform.up * distance;
            case Axis.Z: return transform.forward * distance;
            default: return transform.right * distance;
        }
    }

    IEnumerator SlideDoor(GameObject door, Vector3 targetPosition, float speed)
    {
        isOpening = true;

        var startPos = door.transform.position;
        var distance = Vector3.Distance(startPos, targetPosition);
        var duration = distance / speed;
        var timePassed = 0f;

        while (timePassed < duration)
        {
            var factor = timePassed / duration;

            factor = Mathf.SmoothStep(0, 1, factor);

            door.transform.position = Vector3.Lerp(startPos, targetPosition, factor);

            yield return null;

            timePassed += Time.deltaTime;
        }

        door.transform.position = targetPosition;

        isOpening = false;
    }

    void PlayDoorSound(AudioClip[] soundEffects)
    {
        if (doorAudio != null && soundEffects.Length > 0)
        {
            doorAudio.PlayOneShot(soundEffects[Random.Range(0, soundEffects.Length)]);
        }
    }
}