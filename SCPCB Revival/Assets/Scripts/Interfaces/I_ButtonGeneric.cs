using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class I_ButtonGeneric : MonoBehaviour, IInteractable {
    [Header("Button Settings")]
    [SerializeField] private bool useInteractMessage;
    [SerializeField] private float interactionCooldown = 0.5f;
    [SerializeField] private string interactMessage;

    [Header("Events")]
    [SerializeField] private UnityEvent onInteract;

    private bool canInteract = true;

    public void Interact(PlayerInteraction playerInteraction) {
        if (!canInteract) return;

        onInteract?.Invoke();
        if (useInteractMessage) InfoTextManager.Instance.NotifyPlayer(interactMessage);
        StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown() {
        canInteract = false;
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;
    }
}