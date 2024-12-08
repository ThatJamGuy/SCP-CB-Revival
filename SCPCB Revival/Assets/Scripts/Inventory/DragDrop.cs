using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// The script that manages the dragging and dropping of items in the inventory.
/// </summary>
public class DragDrop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Canvas canvas;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private InventoryItem inventoryItem;
    public static GameObject itemBeingDragged;

    Vector3 startPosition;
    Transform startParent;

    private bool droppedOnValidSlot;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = FindFirstObjectByType<Canvas>();
        inventoryItem = GetComponent<InventoryItem>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        startParent = transform.parent;

        canvasGroup.blocksRaycasts = false;
        transform.SetParent(canvas.transform);
        itemBeingDragged = gameObject;
        droppedOnValidSlot = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        itemBeingDragged = null;

        if (droppedOnValidSlot)
        {
            transform.localPosition = Vector3.zero;
        }
        else
        {
            DropItemFromInventory();
        }

        canvasGroup.blocksRaycasts = true;
    }

    public void SetDroppedOnValidSlot(Transform newParent)
    {
        droppedOnValidSlot = true;
        transform.SetParent(newParent);
    }

    public void ResetToOriginalSlot()
    {
        transform.SetParent(startParent);
        transform.localPosition = Vector3.zero;  
        droppedOnValidSlot = true;
    }

    private void DropItemFromInventory()
    {
        if (inventoryItem.itemPrefab == null) return;

        Transform playerCamera = Camera.main.transform;
        Vector3 spawnPosition = playerCamera.position + playerCamera.forward * 1.5f;

        Instantiate(inventoryItem.itemPrefab, spawnPosition, Quaternion.identity);

        InventorySystem.instance.itemList.Remove(inventoryItem.itemPrefab.name);
        Destroy(gameObject);
    }
}