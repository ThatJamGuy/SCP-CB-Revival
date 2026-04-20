using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Local component script that will handle identifying objects the player can interact with, and then handling it
/// Also handles the visual aspect of it (IE. the hand icon that shows when near an interactable)
/// </summary>
public class PlayerInteraction : MonoBehaviour {
    [Header("Interaction Settings")]
    [SerializeField] private string interactTag = "Interact";
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private float interactRadius = 2.2f;
    [SerializeField] private float interactIconScreenPadding = 20f;

    [Header("References")]
    [SerializeField] private Camera playerCamera;

    private InputAction interactAction;
    
    private GameObject currentInteractable;
    private RectTransform canvasRectTransform;
    private Image interactIcon;

    private bool cantFunction;

    #region Unity Callbacks
    private void Start() {
        // If there is no InputManager available at the start, disallow functionality and print a warning in console
        if (InputManager.Instance == null) {
            cantFunction = true;
            Debug.Log("<color=red>[PlayerInteraction]</color> InputManager was not found, interacting will not work!");

            return;
        }
        
        // If there is no CanvasInstance available at the start, disallow functionality and print a warning in console
        // Will likely later redo this so that instead of just not working, it doesn't show the prompt (No Hud Mode ?)
        if (CanvasInstance.Instance == null) {
            cantFunction = true;
            Debug.Log("<color=red>[PlayerInteraction]</color> CanvasInstance was not found, interacting will not work!");

            return;
        }
        
        // Get the inputAction from the InputManager
        // Also set the interactIcon to the one defined in the CanvasInstance
        // Finally get the canvasRectTransform so the thing can know how to position the interact icon on the screen
        interactAction = InputManager.Instance.GetAction("Player", "Interact");
        interactIcon = CanvasInstance.Instance.interactIcon;
        canvasRectTransform = CanvasInstance.Instance.canvasRectTransform;
    }
    
    private void Update() {
        // If input is currently not allowed or cannot function, then don't do anything
        if (Player.Instance.disableInput || cantFunction) return;

        DetermineClosestInteractable();
        HandleInteraction();
    }
    #endregion

    #region Private Methods
    
    #region Determine Current Interactable
    private void DetermineClosestInteractable() {
        // hits checks a sphere around the player in a radius instead of the entire scene
        // In terms of the other stuff, set the closestInteractable to nothing since we don't have anything yet obv
        var hits = Physics.OverlapSphere(transform.position, interactRadius, interactableMask);
        var closestDist = float.MaxValue;
        GameObject closestInteractable = null;
        
        // For every collider in the cool circle area around the player...
        foreach (var hit in hits) {
            // Check if it uses the interactTag. If not, give up this foreach check
            if (!hit.CompareTag(interactTag)) continue;
            
            // Create a distance variable from the component transform to the colliders in the radius
            var distance = Vector3.Distance(transform.position, hit.transform.position);
            
            // If the distance is greater or equal to the closestDist then give up this foreach check
            // Also give up if the Linecast in the radius hits an object collider with one of the obstructionMasks
            if (distance >= closestDist) continue;
            if (Physics.Linecast(transform.position, hit.transform.position, obstructionMask)) continue;
            
            // Set closestDist to the previously provided distance variable
            // Set the closestInteractable to the gameObject that matches all checks (Uses Interact tag and in radius)
            closestDist = distance;
            closestInteractable = hit.gameObject;
        }
        
        // Set the current interactable to the closest one found in the foreach method
        currentInteractable = closestInteractable;
        
        // If there is a closestInteractable available, try and show the interact icon at the screen position of it
        // Otherwise set that John to be inactive
        if (closestInteractable) TryToShowInteractIcon(closestInteractable.transform.position);
        else interactIcon.gameObject.SetActive(false);
    }

    private void TryToShowInteractIcon(Vector3 worldPos) {
        // Set screenPos to a spot on the screen that matches the world position of the interactable
        var screenPos = playerCamera.WorldToScreenPoint(worldPos);
        
        // Flip mirrored coordinates when behind camera so clamping pushes to the correct edge
        if (screenPos.z <= 0f) {
            screenPos.x = Screen.width - screenPos.x;
            screenPos.y = Screen.height - screenPos.y;
        }
        
        // Set the clamp values so that the interact icon cannot leave the screen, basically kidnapping the guy
        screenPos.x = Mathf.Clamp(screenPos.x, interactIconScreenPadding, Screen.width - interactIconScreenPadding);
        screenPos.y = Mathf.Clamp(screenPos.y, interactIconScreenPadding, Screen.height - interactIconScreenPadding);
        
        // Black magic stuff, I actually don't know what this does, but it's important
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform, screenPos, null, out var canvasPos);
        
        // Set the anchored position of the interact icon to the set canvasPos and make him active
        interactIcon.rectTransform.anchoredPosition = canvasPos;
        interactIcon.gameObject.SetActive(true);
    }
    #endregion
    
    #region Handle Interaction Input
    private void HandleInteraction() {
        if (!currentInteractable) return;
        if (!interactAction.triggered) return;
        
        var interactable = currentInteractable.GetComponent<IInteractable>();
            
        interactable?.Interact(this);
    }
    #endregion
    
    #endregion
}