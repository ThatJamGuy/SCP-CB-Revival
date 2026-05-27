using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler {
    [SerializeField] private GameObject outline;

    public GameObject Item => transform.childCount > 1 ? transform.GetChild(1).gameObject : null;

    private RectTransform rectTransform;

    private void Awake() {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
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