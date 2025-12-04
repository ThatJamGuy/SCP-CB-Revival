using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour {
    [Header("Interaction Settings")]
    [SerializeField] private string interactTag = "Interact";
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private float interactRadius = 2f;

    [Header("References")]
    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Image interactIcon;

    private GameObject currentInteractable;

    private void Update() {
        if (!PlayerAccessor.instance.allowInput) return;

        DetermineClosestInteractable();
        HandleInteraction();
    }

    #region Determine Current Interactable
    private void DetermineClosestInteractable() {
        var targets = GameObject.FindGameObjectsWithTag(interactTag);
        float closestDist = float.MaxValue;
        GameObject closest = null;

        foreach (var obj in targets) {
            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist <= interactRadius && dist < closestDist) {
                if (!Physics.Linecast(transform.position, obj.transform.position, obstructionMask)) {
                    closestDist = dist;
                    closest = obj;
                }
            }
        }

        if (closest) {
            currentInteractable = closest;
            Vector2 screenPos = playerCamera.WorldToScreenPoint(closest.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPos, null, out var canvasPos);
            interactIcon.rectTransform.anchoredPosition = canvasPos;
            interactIcon.gameObject.SetActive(true);
        }
        else {
            currentInteractable = null;
            interactIcon.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Handle Interaction Input
    private void HandleInteraction() {
        if (!currentInteractable) return;

        if (InputManager.Instance.interactAction.triggered) {
            IInteractable interactable = currentInteractable.GetComponent<IInteractable>();
            if (interactable != null) {
                interactable.Interact(this);
            }
        }
    }
    #endregion
}