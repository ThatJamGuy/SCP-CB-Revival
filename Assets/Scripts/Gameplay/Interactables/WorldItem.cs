using EditorAttributes;
using UnityEngine;
using FMODUnity;

/// <summary>
/// Script attached to the physical representations of items scattered around the world. Handles pickup logic
/// </summary>

[AddComponentMenu("SCP:CBR/World Item")]
public class WorldItem : MonoBehaviour, IInteractable {
    public ItemData associatedItemData;
    [SerializeField] private EventReference pickupSound;
    
    [SerializeField] private bool giveAchievementOnPickup;
    [SerializeField, ShowField(nameof(giveAchievementOnPickup))] private string achievementId;
 
    public void Interact(PlayerInteraction playerInteraction) {
        if (!InventorySystem.Instance || InventorySystem.Instance.IsFull()) {
            InfoTextManager.Instance.NotifyPlayer("You cannot pick up any more items.");
            return;
        }
 
        InventorySystem.Instance.AddItemToInventory(associatedItemData.itemIdentifier);
        AudioManager.PlayOneShot(pickupSound, transform.position);
        
        if (giveAchievementOnPickup) AchievementSystem.Instance.GiveAchievement(achievementId);
        
        Destroy(gameObject);
    }
}