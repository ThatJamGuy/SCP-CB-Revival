using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerClickHandler {
    public OldItemData itemData;

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.clickCount == 2) {
            HandleDoubleClick();
        }
    }

    private void HandleDoubleClick() {
        switch (itemData.itemType) {
            case OldItemData.Type.Normal:
                break;
            case OldItemData.Type.Keycard:
                OldInventorySystem.instance.EquipItem(itemData);
                break;
            case OldItemData.Type.Document:
                OldInventorySystem.instance.EquipDocument(itemData);
                break;
            case OldItemData.Type.Consumable:
            case OldItemData.Type.Equipment:
                Debug.Log($"Double clicked on {itemData.itemType}: {itemData.itemName}");
                break;
        }
    }
}