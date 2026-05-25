public interface IInteractable {
    void Interact(PlayerInteraction playerInteraction);
}

public interface IHoldInteractable {
    void BeginInteract(PlayerInteraction playerInteraction);
    void EndInteract(PlayerInteraction playerInteraction);
}