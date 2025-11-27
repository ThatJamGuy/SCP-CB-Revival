using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public static GameObject itemBeingDragged;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private InventoryItem inventoryItem;
    private Canvas canvas;
    private Camera playerCamera;

    private Vector3 startPosition;
    private Transform startParent;
    private bool droppedOnValidSlot;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryItem = GetComponent<InventoryItem>();
        canvas = FindFirstObjectByType<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        startPosition = transform.position;
        startParent = transform.parent;
        droppedOnValidSlot = false;

        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform);
        itemBeingDragged = gameObject;
    }

    public void OnDrag(PointerEventData eventData) {
        rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData) {
        itemBeingDragged = null;
        canvasGroup.blocksRaycasts = true;

        if (droppedOnValidSlot) {
            transform.localPosition = Vector3.zero;
        }
        else {
            DropItemIntoWorld();
        }
    }

    public void SetDroppedOnValidSlot(Transform newParent) {
        droppedOnValidSlot = true;
        transform.SetParent(newParent);
    }

    public void ResetToOriginalSlot() {
        transform.SetParent(startParent);
        transform.localPosition = Vector3.zero;
        droppedOnValidSlot = true;
    }

    private void DropItemIntoWorld() {
        if (inventoryItem.itemData?.worldPrefab == null) return;

        Camera cam = PlayerAccessor.instance.playerCamera;
        if (cam == null) return;

        Vector3 spawnPos = cam.transform.position + cam.transform.forward * 1.5f;
        Instantiate(inventoryItem.itemData.worldPrefab, spawnPos, Quaternion.identity);

        Destroy(gameObject);
    }
}