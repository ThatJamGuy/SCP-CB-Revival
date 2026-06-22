using UnityEngine;
using System.Collections;
using FMODUnity;
using EditorAttributes;

/// <summary>
/// Simple preset button script to handle interaction for doors. Meant for the generic ones you find around a lot
/// </summary>
public class DoorButton : MonoBehaviour, IInteractable {
    [Header("Button Settings")]
    [SerializeField] private bool isKeycardButton;
    [SerializeField] private float interactionCooldown = 0.2f;

    [Header("Audio References")]
    [SerializeField, HideField(nameof(isKeycardButton))] private EventReference buttonPressSound;
    [SerializeField, HideField(nameof(isKeycardButton))] private EventReference buttonPressLockedSound;
    [SerializeField, ShowField(nameof(isKeycardButton))] private EventReference keycardFailedEvent;
    [SerializeField, ShowField(nameof(isKeycardButton))] private EventReference keycardGoodEvent;

    [Header("References")]
    public Door linkedDoor;
    [SerializeField] private Animator buttonAnimator;

    private bool canInteract = true;

    #region Private Methods

    private void HandleKeycardDoorThings() {
        if (!linkedDoor || !canInteract || !linkedDoor.requiresKeycard) return;

        // Player not holding anything
        if (InventorySystem.Instance.currentlyHeldItem == null || InventorySystem.Instance.currentlyHeldItem.chosenBehavior != PresetBehavior.Key) {
            InfoTextManager.Instance.NotifyPlayer("A keycard is required to operate this door.");
            return;
        }

        // Player has key, but it's not high enough
        if (InventorySystem.Instance.currentlyHeldItem.chosenBehavior == PresetBehavior.Key
            && InventorySystem.Instance.currentlyHeldItem.clearance < linkedDoor.requiredKeyLevel) {
            InfoTextManager.Instance.NotifyPlayer("A keycard with security clearance " + linkedDoor.requiredKeyLevel + " or higher is required to operate this door.");
            AudioManager.PlayOneShot(keycardFailedEvent, transform.position);

            InventorySystem.Instance.UnequipCurrentItem();

            return;
        }

        // Player has key and it's all good, so open the door
        if (InventorySystem.Instance.currentlyHeldItem.chosenBehavior == PresetBehavior.Key
            && InventorySystem.Instance.currentlyHeldItem.clearance >= linkedDoor.requiredKeyLevel) {
            InfoTextManager.Instance.NotifyPlayer("The keycard was inserted into the slot.");
            AudioManager.PlayOneShot(keycardGoodEvent, transform.position);

            InventorySystem.Instance.UnequipCurrentItem();

            linkedDoor.ToggleDoorState();
            StartCoroutine(Cooldown());
        }
    }

    #endregion

    #region Public Methods

    public void Interact(PlayerInteraction playerInteraction) {
        if (isKeycardButton) { HandleKeycardDoorThings(); return; }

        // If a linked door is missing, disallow functionality as this script in meant specifically for doors
        if (!linkedDoor) return;
        
        // If the interaction is disabled, do not allow things inside interact to be executed
        if (!canInteract) return;

        // If the linked door has isLocked set to true, play access denied sound and display a message to the player
        if (linkedDoor.isLocked) {
            AudioManager.PlayOneShot(buttonPressLockedSound, transform.position);
            if (buttonAnimator) buttonAnimator.Play("ModernButtonPress");
            InfoTextManager.Instance.NotifyPlayer("The door appears to be locked.");

            // Run the coroutine to disable interaction and then enable it again
            StartCoroutine(Cooldown());
            return;
        }

        // If the previous check passed, play button sound
        AudioManager.PlayOneShot(buttonPressSound, transform.position);
        if (buttonAnimator) buttonAnimator.Play("ModernButtonPress");

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