using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerClickHandler {
    public ItemData itemData;

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.clickCount == 2) {
            HandleDoubleClick();
        }
    }

    private void HandleDoubleClick() {
        switch (itemData.itemType) {
            case ItemData.Type.Normal:
                break;
            case ItemData.Type.Keycard:
                InventorySystem.instance.EquipItem(itemData);
                break;
            case ItemData.Type.Document:
                InventorySystem.instance.EquipDocument(itemData);
                break;
            case ItemData.Type.Consumable:
            case ItemData.Type.Equipment:
                Debug.Log($"Double clicked on {itemData.itemType}: {itemData.itemName}");
                break;
        }
    }
}