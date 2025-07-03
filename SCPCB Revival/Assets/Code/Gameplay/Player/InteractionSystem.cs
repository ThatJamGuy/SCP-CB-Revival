using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractionSystem : MonoBehaviour {
    [Header("Input")]
    public InputActionAsset playerControls;

    [Header("References")]
    public Camera playerCamera;

    [Header("Settings")]
    public float interactRadius = 2f;
    public Canvas canvas;
    public Image interactDisplay;
    public LayerMask obstructionMask;

    GameObject currentInteractible;
    Lever currentLever;
    RectTransform canvasRect;

    private const string targetTag = "I_Interactable";
    private InputAction interactAction;

    private void OnEnable() {
        playerControls.Enable();
    }

    private void OnDisable() {
        playerControls.Disable();
    }

    void Awake() {
        canvasRect = canvas.GetComponent<RectTransform>();

        interactAction = playerControls.FindAction("Fire", true);
    }

    void Update() {
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

        if (interactAction != null && interactAction.WasPressedThisFrame()) {
            if (currentInteractible.TryGetComponent(out Button button)) button.PressButton();
            else if (currentInteractible.GetComponentInParent<Lever>() is Lever lever) {
                currentLever = lever;
                currentLever.UseLever(true);
            }
            else if (currentInteractible.TryGetComponent(out PhysicalItem item)) item.AddItemToInventory();
        }

        if (interactAction != null && interactAction.WasReleasedThisFrame() && currentLever != null) {
            currentLever.UseLever(false);
            currentLever = null;
        }
    }
}