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
        if (!canInteract) return;

        if (!door.isLocked)
        {
            PlaySound(buttonSFX);
            ToggleDoorState();
        }
        else
        {
            if (door.isBroken)
            {
                if (door.requiresKeycard && InventorySystem.instance.currentKeyLevel > 0)
                    NotifyAndCooldown("The keycard was inserted into the slot but nothing happened.", lockedSFX);
                else
                    NotifyAndCooldown("You pushed the button but nothing happened.", lockedSFX);
            }
            else if (door.requiresKeycard)
            {
                if (InventorySystem.instance.currentKeyLevel >= door.requiredKeyLevel)
                {
                    PlaySound(buttonSFX);
                    ToggleDoorState();

                    InventorySystem.instance.UnequipItem();
                }
                else if (InventorySystem.instance.currentKeyLevel < door.requiredKeyLevel && InventorySystem.instance.currentKeyLevel > 0)
                {
                    NotifyAndCooldown("A keycard with security clearance " + door.requiredKeyLevel + " or higher is required to operate this door.", lockedSFX);
                    InventorySystem.instance.UnequipItem();
                }
                else
                {
                    NotifyAndCooldown("A keycard is required to operate this door.", lockedSFX);
                }
            }
            else
            {
                NotifyAndCooldown("The door appears to be locked.", lockedSFX);
            }
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

    private void PlaySound(AudioClip clip)
    {
        buttonSource.clip = clip;
        buttonSource.Play();
    }

    private void NotifyAndCooldown(string message, AudioClip clip)
    {
        PlaySound(clip);
        InfoTextManager.Instance.NotifyPlayer(message);
        canInteract = false;
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(1f);
        canInteract = true;
    }
}