using UnityEngine;
using System.Collections;

public class I_ButtonDoor : MonoBehaviour, IInteractable {
    [Header("Button Settings")]
    [SerializeField] private bool playButtonSound = true;
    [SerializeField] private float interactionCooldown = 0.5f;

    [Header("References")]
    [SerializeField] private Door linkedDoor;
    [SerializeField] private Animator buttonAnimator;

    private bool canInteract = true;

    public void Interact(PlayerInteraction playerInteraction) {
        if (!canInteract) return;

        if (linkedDoor.isLocked) {
            if (playButtonSound) AudioManager.instance.PlaySound(FMODEvents.instance.buttonPress2, transform.position);
            if (buttonAnimator != null) buttonAnimator.Play("ModernButtonPress");

            StartCoroutine(Cooldown());
            return;
        }

        if (playButtonSound) AudioManager.instance.PlaySound(FMODEvents.instance.buttonPress, transform.position);
        if (buttonAnimator != null) buttonAnimator.Play("ModernButtonPress");

        linkedDoor.ToggleDoorState();

        StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown() {
        canInteract = false;
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }
}