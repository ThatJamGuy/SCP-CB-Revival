using UnityEngine;
using System.Collections;
using FMODUnity;

/// <summary>
/// Simple preset button script to handle interaction for doors. Meant for the generic ones you find around a lot
/// </summary>
public class DoorButton : MonoBehaviour, IInteractable {
    [Header("Button Settings")]
    [SerializeField] private float interactionCooldown = 0.2f;

    [Header("Audio References")]
    [SerializeField] private EventReference buttonPressSound;
    [SerializeField] private EventReference buttonPressLockedSound;

    [Header("References")]
    [SerializeField] private Door linkedDoor;
    [SerializeField] private Animator buttonAnimator;

    private bool canInteract = true;

    #region Public Methods
    public void Interact(PlayerInteraction playerInteraction) {
        // If a linked door is missing, disallow functionality as this script in meant specifically for doors
        if (!linkedDoor) {
            Debug.Log($"[DoorButton ({linkedDoor})] Button is missing a linked door!");
            return;
        }
        
        // If the interaction is disabled, do not allow things inside interact to be executed
        if (!canInteract) return;

        // If the linked door has isLocked set to true, play access denied sound and display a message to the player
        if (linkedDoor.isLocked) {
            AudioManager.PlayOneShot(buttonPressLockedSound, transform.position);
            if (buttonAnimator) buttonAnimator.Play("ModernButtonPress");
            //nfoTextManager.Instance.NotifyPlayer("The door appears to be locked.");

            // Run the coroutine to disable interaction and then enable it again
            StartCoroutine(Cooldown());
            return;
        }

        // If the previous check passed, play button sound
        AudioManager.PlayOneShot(buttonPressSound, transform.position);
        if (buttonAnimator != null) buttonAnimator.Play("ModernButtonPress");

        // Tells the linked door to toggle it's current state
        linkedDoor.ToggleDoorState();

        // Run the coroutine to disable interaction and then enable it again
        StartCoroutine(Cooldown());
    }
    #endregion

    #region Private Coroutines
    private IEnumerator Cooldown() {
        // Set it so that interaction is disabled for the duration of interactCooldown, then allow interaction again
        canInteract = false;
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }
    #endregion
}