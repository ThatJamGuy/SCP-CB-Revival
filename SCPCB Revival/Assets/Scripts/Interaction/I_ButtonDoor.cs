using UnityEngine;
using System.Collections;
using FMODUnity;

public class I_ButtonDoor : MonoBehaviour, IInteractable {
    [Header("Button Settings")]
    [SerializeField] private float interactionCooldown = 0.5f;

    [Header("FMOD Audio")]
    [SerializeField] private EventReference buttonPressSound;
    [SerializeField] private EventReference buttonPressLockedSound;

    [Header("References")]
    [SerializeField] private Door linkedDoor;
    [SerializeField] private Animator buttonAnimator;

    private bool canInteract = true;

    public void Interact(PlayerInteraction playerInteraction) {
        if (!canInteract) return;

        if (linkedDoor.isLocked) {
            AudioManager.instance.PlaySound(buttonPressLockedSound, transform.position);
            if (buttonAnimator != null) buttonAnimator.Play("ModernButtonPress");
            InfoTextManager.Instance.NotifyPlayer("The door appears to be locked.");

            StartCoroutine(Cooldown());
            return;
        }

        AudioManager.instance.PlaySound(buttonPressSound, transform.position);
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