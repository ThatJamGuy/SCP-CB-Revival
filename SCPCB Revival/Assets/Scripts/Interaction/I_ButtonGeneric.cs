using UnityEngine;
using UnityEngine.Events;

public class I_ButtonGeneric : MonoBehaviour, IInteractable {
    [SerializeField] private UnityEvent onInteract;
    [SerializeField] private bool playButtonSound = true;

    public void Interact(PlayerInteraction playerInteraction) {
        onInteract?.Invoke();
    }
}