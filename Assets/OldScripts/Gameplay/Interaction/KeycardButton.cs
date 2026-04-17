using UnityEngine;
using FMODUnity;
using System.Collections;

public class KeycardButton : MonoBehaviour, IInteractable {
    [Header("Button Settings")]
    [SerializeField] private float interactionCooldown = 0.5f;

    [Header("FMOD Audio")]
    [SerializeField] private EventReference keycardSwipeUnlock;
    [SerializeField] private EventReference keycardSwipeLocked;

    [Header("References")]
    [SerializeField] private Door linkedDoor;

    private int requiredKeyLevel = 0;
    private bool canInteract = true;

    public void Interact(PlayerInteraction playerInteraction) {
        if (requiredKeyLevel == 0) requiredKeyLevel = linkedDoor.requiredKeyLevel;

        if (!canInteract) return;
        StartCoroutine(Cooldown());

        if (InventorySystem.instance.currentHeldItemData == null || InventorySystem.instance.currentHeldItemData.keyLevel == 0) {
            AudioManager.instance.PlaySound(keycardSwipeLocked, transform.position);
            InfoTextManager.Instance.NotifyPlayer("A keycard is required to operate this door.");
            return;
        }

        if (InventorySystem.instance.currentHeldItemData != null) {
            if (InventorySystem.instance.currentHeldItemData.keyLevel >= requiredKeyLevel) {
                AudioManager.instance.PlaySound(keycardSwipeUnlock, transform.position);
                InfoTextManager.Instance.NotifyPlayer("The keycard was inserted into the slot.");
                InventorySystem.instance.UnequipItem();
                linkedDoor.ToggleDoorState();
            } else {
                AudioManager.instance.PlaySound(keycardSwipeLocked, transform.position);
                InfoTextManager.Instance.NotifyPlayer($"A keycard with security clearance {requiredKeyLevel} or higher is required to operate this door.");
                InventorySystem.instance.UnequipItem();
                return;
            }
        }
    }

    private IEnumerator Cooldown() {
        canInteract = false;
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }
}