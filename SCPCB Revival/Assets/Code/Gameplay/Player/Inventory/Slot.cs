using UnityEngine;
using UnityEngine.EventSystems;

namespace scpcbr {
    public class Slot : MonoBehaviour, IDropHandler {
        [SerializeField] private GameObject outline;
        [SerializeField] private RectTransform rectTransform;

        private void Awake() {
            if (outline != null)
                outline.SetActive(false);
        }

        private void Update() {
            if (IsMouseOverSlot()) {
                ShowOutline();
            }
            else {
                HideOutline();
            }
        }

        public GameObject Item => transform.childCount > 0 ? transform.GetChild(0).gameObject : null;

        public void OnDrop(PointerEventData eventData) {
            DragDrop dragDrop = DragDrop.itemBeingDragged?.GetComponent<DragDrop>();
            if (dragDrop == null) return;

            if (!Item) {
                dragDrop.SetDroppedOnValidSlot(transform);
                DragDrop.itemBeingDragged.transform.localPosition = Vector3.zero;
            }
            else {
                InfoTextManager.Instance.NotifyPlayer("You cannot combine these two items.");
                dragDrop.ResetToOriginalSlot();
            }
        }

        private bool IsMouseOverSlot() {
            Vector2 mousePosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out mousePosition)) {
                return rectTransform.rect.Contains(mousePosition);
            }
            return false;
        }

        private void ShowOutline() {
            if (outline != null && !outline.activeSelf)
                outline.SetActive(true);
        }

        private void HideOutline() {
            if (outline != null && outline.activeSelf)
                outline.SetActive(false);
        }
    }
}