using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler {
    [SerializeField] private GameObject outline;

    public GameObject Item => transform.childCount > 1 ? transform.GetChild(1).gameObject : null;

    private RectTransform rectTransform;

    private bool isHovered;

    private void Awake() {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable() {
        outline.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        isHovered = true;
        SetOutlineActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovered = false;
        SetOutlineActive(false);
    }

    private void SetOutlineActive(bool active) {
        if (outline != null && outline.activeSelf != active)
            outline.SetActive(active);
    }

    public void OnDrop(PointerEventData eventData) {
        DragDrop dragDrop = DragDrop.itemBeingDragged?.GetComponent<DragDrop>();
        if (dragDrop == null) return;

        if (Item == null) {
            dragDrop.SetDroppedOnValidSlot(transform);
        }
        else {
            InfoTextManager.Instance.NotifyPlayer("You cannot combine these two items.");
            dragDrop.ResetToOriginalSlot();
        }
    }
}