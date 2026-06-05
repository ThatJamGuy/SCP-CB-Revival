using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Script attached to the inventory item template to handle various "What should I do here" situations
/// </summary>
public class InventoryItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public static GameObject itemBeingDragged { get; private set; }
    
    // Hide this to avoid confusion as it will never ever be modified before runtime
    [HideInInspector] public ItemData itemData;
    
    // Set these ahead of time in the editor as they aren't changed at runtime and will save on calculations on start
    [Header("Local References")]
    [SerializeField] private RectTransform itemRectTransform;
    [SerializeField] private Image itemImageComponent;
    
    private Canvas screenCanvas;
    private Camera playerCamera;
    private Transform itemPreDragParent;
    //private Vector3 itemPreDragPosition;
    
    private bool itemDroppedOnValidSlot;

    #region Unity Callbacks

    private void Start() {
        if (CanvasInstance.Instance != null) screenCanvas = CanvasInstance.Instance.screensCanvas;
        if (Player.Instance != null) playerCamera = Player.Instance.playerCamera;
    }
    
    // Handle double-clicking logic
    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.clickCount == 2) {
            HandleDoubleClick();
        }
    }

    // Set some stuff up so clicking and dragging doesn't break everything
    public void OnBeginDrag(PointerEventData eventData) {
        itemDroppedOnValidSlot = false;
        itemPreDragParent = transform.parent;
        itemImageComponent.raycastTarget = false;
        itemBeingDragged = gameObject;
        
        transform.SetParent(screenCanvas.transform);
    }

    // Handle the actual dragging, more or less keeping the item up with the cursor
    public void OnDrag(PointerEventData eventData) {
        itemRectTransform.anchoredPosition += eventData.delta;
    }

    // Stuff to do once the drag is completed. Handles slot changes, item dropping, combinations, etc.
    public void OnEndDrag(PointerEventData eventData) {
        itemBeingDragged = null;
        itemImageComponent.raycastTarget = true;

        if (itemDroppedOnValidSlot) transform.localPosition = Vector3.zero;
        else DropItemIntoWorld();
    }

    #endregion

    #region Private Methods
    
    private void HandleDoubleClick() {
        Debug.Log("Item " + itemData +" has been double clicked.");
    }

    private void DropItemIntoWorld() {
        var spawnPos = playerCamera.transform.position + playerCamera.transform.forward * 0.5f;
        
        Instantiate(itemData.itemWorldPrefab, spawnPos, Quaternion.identity);
        InventorySystem.Instance.RemoveItemFromInventory(itemData.itemIdentifier, false);
        
        Destroy(gameObject);
    }
    
    #endregion
    
    public void SetNewValidParent(Transform newParent) {
        itemDroppedOnValidSlot = true;
        transform.SetParent(newParent);
    }
    
    public void ResetToOriginalSlot() {
        transform.SetParent(itemPreDragParent);
        transform.localPosition = Vector3.zero;
        itemDroppedOnValidSlot = true;
    }
}