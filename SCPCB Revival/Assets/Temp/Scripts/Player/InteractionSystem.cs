using UnityEngine;
using UnityEngine.UI;

public class InteractionSystem : MonoBehaviour {
    [Header("Settings")]
    public string targetTag = "I_Interactable";
    public float interactRadius = 3f;
    public Canvas canvas;
    public Image interactDisplay;
    public LayerMask obstructionMask;

    GameObject currentInteractible;
    Lever currentLever;
    Camera playerCamera;
    RectTransform canvasRect;

    void Awake() {
        playerCamera = FindFirstObjectByType<PlayerController>().playerCamera;
        canvasRect = canvas.GetComponent<RectTransform>();
    }

    void Update() {
        if (GameManager.Instance.disablePlayerInputs) return;
        FindClosestInteractable();
        HandleInteraction();
    }

    void FindClosestInteractable() {
        var targets = GameObject.FindGameObjectsWithTag(targetTag);
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
            currentInteractible = closest;
            Vector2 screenPos = playerCamera.WorldToScreenPoint(closest.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out var canvasPos);
            interactDisplay.rectTransform.anchoredPosition = canvasPos;
            interactDisplay.gameObject.SetActive(true);
        }
        else {
            currentInteractible = null;
            interactDisplay.gameObject.SetActive(false);
        }
    }

    void HandleInteraction() {
        if (!currentInteractible) return;

        if (Input.GetMouseButtonDown(0)) {
            if (currentInteractible.TryGetComponent(out Button button)) button.PressButton();
            else if (currentInteractible.GetComponentInParent<Lever>() is Lever lever) {
                currentLever = lever;
                currentLever.UseLever(true);
            }
            else if (currentInteractible.TryGetComponent(out PhysicalItem item)) item.AddItemToInventory();
        }

        if (Input.GetMouseButtonUp(0) && currentLever != null) {
            currentLever.UseLever(false);
            currentLever = null;
        }
    }
}