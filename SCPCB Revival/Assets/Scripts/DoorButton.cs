using System.Collections;
using UnityEngine;

public class DoorButton : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip buttonSFX;
    [SerializeField] private AudioClip lockedSFX;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource buttonSource;

    [Header("Door Settings")]
    public Door door;
    public bool doorButton = true;

    private bool canInteract;

    private void Start()
    {
        canInteract = true;
    }

    public void UseButton()
    {
        if (!door.isLocked && canInteract)
        {
            buttonSource.clip = buttonSFX;
            buttonSource.Play();

            ToggleDoorState();
        }
        else if (canInteract)
        {
            buttonSource.clip = lockedSFX;
            buttonSource.Play();

            canInteract = false;
            StartCoroutine(Cooldown());
        }
    }

    private void ToggleDoorState()
    {
        if (door != null)
        {
            if (door.isOpen)
            {
                door.CloseDoor();
            }
            else
            {
                door.OpenDoor();
            }

            canInteract = false;
            StartCoroutine(Cooldown());
        }
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(1f);
        canInteract = true;
    }
}