using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject itemPrefab;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    public static GameObject itemBeingDragged;

    private bool droppedOnValidSlot;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = FindFirstObjectByType<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
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

    private void DropItemFromInventory()
    {
        if (itemPrefab == null) return;

        Transform playerCamera = Camera.main.transform;
        Vector3 spawnPosition = playerCamera.position + playerCamera.forward * 1.5f;

        Instantiate(itemPrefab, spawnPosition, Quaternion.identity);

        InventorySystem.instance.itemList.Remove(itemPrefab.name);
        Destroy(gameObject);
    }
}