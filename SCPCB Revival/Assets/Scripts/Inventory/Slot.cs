using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{
    [SerializeField] private GameObject outline;
    [SerializeField] private RectTransform rectTransform;

    private void Awake()
    {
        if (outline != null)
            outline.SetActive(false);
    }

    private void Update()
    {
        if (IsMouseOverSlot())
        {
            ShowOutline();
        }
        else
        {
            HideOutline();
        }
    }

    public GameObject Item
    {
        get
        {
            if (transform.childCount > 0)
            {
                return transform.GetChild(0).gameObject;
            }

            return null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!Item)
        {
            DragDrop dragDrop = DragDrop.itemBeingDragged.GetComponent<DragDrop>();
            if (dragDrop != null)
            {
                dragDrop.SetDroppedOnValidSlot(transform);
                DragDrop.itemBeingDragged.transform.localPosition = Vector3.zero;
            }
        }
    }

    private bool IsMouseOverSlot()
    {
        Vector2 mousePosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out mousePosition))
        {
            return rectTransform.rect.Contains(mousePosition);
        }
        return false;
    }

    private void ShowOutline()
    {
        if (outline != null && !outline.activeSelf)
            outline.SetActive(true);
    }

    private void HideOutline()
    {
        if (outline != null && outline.activeSelf)
            outline.SetActive(false);
    }
}