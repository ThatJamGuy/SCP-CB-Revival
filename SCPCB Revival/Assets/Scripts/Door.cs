using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isOpen;
    public bool isLocked;
    public float regularOpenCloseSpeed;
    public float regularOpenCloseDistance;

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

    public void OpenDoor()
    {
        if (!isOpening && !isOpen)
        {
            door01TargetPosition = door01InitialPosition + transform.right * regularOpenCloseDistance;
            door02TargetPosition = door02InitialPosition - transform.right * regularOpenCloseDistance;
            StartCoroutine(SlideDoor(doorPart01, door01TargetPosition, regularOpenCloseSpeed));
            StartCoroutine(SlideDoor(doorPart02, door02TargetPosition, regularOpenCloseSpeed));
            PlayDoorSound(doorOpenSFX);
            isOpen = true;
        }
    }

    public void CloseDoor()
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