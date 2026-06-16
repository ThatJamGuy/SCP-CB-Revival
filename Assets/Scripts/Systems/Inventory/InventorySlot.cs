using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Script attached to every slot in the inventory to keep track of various things
/// </summary>
public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler {
    // If the slots child count is greater than 2 (More than the two outlines), set the variable to the third child
    private GameObject currentItemInSlot => transform.childCount > 2 ? transform.GetChild(2).gameObject : null;
    
    private GameObject slotOutline;
    private GameObject equippedItemOutline;

    #region Unity Callbacks
    private void Awake() {
        // Automatically get the various outlines on startup to make things easier
        equippedItemOutline = transform.GetChild(0).gameObject;
        slotOutline = transform.GetChild(1).gameObject;
    }

    private void OnEnable() {
        slotOutline.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        slotOutline.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        slotOutline.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData) {
        var invItem = InventoryItem.itemBeingDragged?.GetComponent<InventoryItem>();
        if (invItem == null) return;
        
        // If the current slot is empty, accept this item and tell it to parent itself to this slot now
        if (currentItemInSlot == null) invItem.SetNewValidParent(transform);
        else {
            // TODO: ACTUALLY IMPLEMENT THE CHECK LATER TO SEE IF ITEMS CAN BE COMBINED. NEED THIS FOR BATTERY THINGS
            InfoTextManager.Instance.NotifyPlayer("You cannot combine these two items.");
            invItem.ResetToOriginalSlot();
        }
    }

    #endregion
}