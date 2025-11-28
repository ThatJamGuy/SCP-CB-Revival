using NUnit.Framework.Interfaces;
using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable {
    public ItemData itemData;

    public void Interact(PlayerInteraction playerInteraciton) {
        if (InventorySystem.instance.IsFull()) {
            InfoTextManager.Instance.NotifyPlayer("You cannot carry any more items.");
            return;
        }
        InventorySystem.instance.AddToInventory(itemData.itemName);
        AudioManager.instance.PlaySound(itemData.pickupSound, transform.position);
        Destroy(gameObject);
    }
}