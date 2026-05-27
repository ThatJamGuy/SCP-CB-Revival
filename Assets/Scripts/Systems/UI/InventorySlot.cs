using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Script attached to every slot in the inventory to keep track of various things
/// </summary>
public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    private GameObject slotOutline;

    #region Unity Callbacks
    private void Awake() {
        // Automatically get the inventory slot outline on startup to make things easier
        slotOutline = transform.GetChild(0).gameObject;
    }

    private void OnEnable() {
        // Ensure the outline is disabled when the slot enables so it's not sitting their awkwardly already enabled
        slotOutline.SetActive(false);
    }

    // Set the outline to be active on mouse hover
    public void OnPointerEnter(PointerEventData eventData) {
        slotOutline.SetActive(true);
    }

    // Set the outline to be inactive when the mouse leaves
    public void OnPointerExit(PointerEventData eventData) {
        slotOutline.SetActive(false);
    }
    #endregion
}