using EditorAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GenericButton : MonoBehaviour, IInteractable {
    [Header("Button Settings")]
    [SerializeField] private bool useInteractMessage;
    [SerializeField] private float interactionCooldown = 0.2f;

    [SerializeField, ShowField(nameof(useInteractMessage))] private string interactMessage;
    [SerializeField, ShowField(nameof(useInteractMessage))] private float messageDuration = 3f;
    [SerializeField, ShowField(nameof(useInteractMessage))] private float messageFadeDuration = 2f;

    [Header("Events")]
    [SerializeField] private UnityEvent onInteract;

    private bool canInteract = true;

    public void Interact(PlayerInteraction playerInteraction) {
        if (!canInteract) return; // Do nothing if the player cannot interact

        onInteract?.Invoke(); // Invoke the event

        // If there is to be an interact message, trigger that as well
        if (useInteractMessage && InfoTextManager.Instance)
            InfoTextManager.Instance.NotifyPlayer(interactMessage, messageDuration, messageFadeDuration);

        StartCoroutine(Cooldown()); // Begin the cooldown before interaction can occur again
    }

    private IEnumerator Cooldown() {
        canInteract = false;
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }
}