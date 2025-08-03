using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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
            if (Mouse.current == null) return false;

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePosition, null, out var localPoint)) {
                return rectTransform.rect.Contains(localPoint);
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