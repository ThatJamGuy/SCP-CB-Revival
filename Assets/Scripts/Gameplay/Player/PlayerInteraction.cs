using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    // New stuff to prioritize objects the player is looking at.
    [Header("Look Priority Things")]
    [SerializeField] private float lookWeight = 0.5f; // 0 - closest obj chosen, 1 - looked at object chosen
    [SerializeField] private float minLookDot = -1;

    [Header("References")]
    [SerializeField] private Camera playerCamera;

    // Not a clanker comment btw I'm learning about sphere overlap non alloc,
    // this is stuff from the Unity Docs
    // Set the maximum number of colliders that can be detected at once.
    private const int MAX_COLLIDERS = 10;

    private IInteractable currentInteractablePress;
    private IHoldInteractable currentInteractableHold;

    private InputAction interactAction;
    private GameObject currentInteractable;
    private RectTransform canvasRectTransform;
    private Image interactIcon;
    private Collider[] hitColliders; // Declare hitColliders as a reusable field.
    private Transform cameraTransform;

    private bool cantFunction;

    #region Unity Callbacks
    private void Awake() {
        // Initialize the colliders once
        hitColliders = new Collider[MAX_COLLIDERS];
        cameraTransform = playerCamera.transform;
    }

    private void Start() {
        // If there is no InputManager available at the start, disallow functionality and print a warning in console
        if (RevivalRuntimeEngine.Instance == null) {
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
        interactAction = RevivalRuntimeEngine.Instance.GetAction("Player", "Interact");
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

    /// <summary>
    /// Determines the closest interactable to the player.
    /// Recent changes allow this method to prioritize the object the player is looking at.
    /// </summary>
    private void DetermineClosestInteractable() {
        // Reuse the pre-allocated array for Physics.OverlapSphereNonAlloc.
        var origin = transform.position;
        int numColliders = Physics.OverlapSphereNonAlloc(origin, interactRadius, hitColliders, interactableMask);

        // Cache what we need for scoring outside the loop
        var camPos = cameraTransform.position;
        var camForward = cameraTransform.forward;
        float distWeight = 1f - lookWeight;
        float invRadius = 1f / interactRadius;

        var bestScore = float.MinValue;
        GameObject closestInteractable = null;

        // For every collider in the cool circle area around the player...
        for (int i = 0; i < numColliders; i++) {
            Collider hit = hitColliders[i];

            // Check if it uses the interactTag. If not, give up this foreach check
            if (!hit.CompareTag(interactTag)) continue;

            var hitPosition = hit.transform.position;

            // Distance from the player to the collider, normalized to 0..1 by the radius
            var toHit = hitPosition - origin;
            float dist = Mathf.Sqrt(toHit.sqrMagnitude);
            float distScore = 1f - Mathf.Min(dist * invRadius, 1f);

            // How directly the camera is aimed at the collider. 1 = dead center, -1 = directly behind
            var toHitFromCam = hitPosition - camPos;
            float camSqr = toHitFromCam.sqrMagnitude;
            float lookDot = camSqr > 0.0001f
                ? Vector3.Dot(camForward, toHitFromCam) / Mathf.Sqrt(camSqr)
                : 1f;

            if (lookDot < minLookDot) continue;

            // Blend the two. Remap the dot from -1..1 to 0..1 so both parts share the same scale
            float score = (lookDot * 0.5f + 0.5f) * lookWeight + distScore * distWeight;

            // Cheap math first, expensive Linecast only if this could actually win
            if (score <= bestScore) continue;
            if (Physics.Linecast(origin, hitPosition, obstructionMask)) continue;

            bestScore = score;
            closestInteractable = hit.gameObject;
        }

        // Set the current interactable to the closest one found in the foreach method
        if (!ReferenceEquals(closestInteractable, currentInteractable)) {
            currentInteractable = closestInteractable;

            currentInteractablePress = closestInteractable
                ? closestInteractable.GetComponent<IInteractable>()
                : null;

            currentInteractableHold = closestInteractable
                ? closestInteractable.GetComponent<IHoldInteractable>()
                : null;
        }

        // If there is a closestInteractable available, try and show the interact icon at the screen position of it
        // Otherwise set that John to be inactive
        if (closestInteractable) TryToShowInteractIcon(closestInteractable.transform.position);
        else if (interactIcon.gameObject != null) interactIcon.gameObject.SetActive(false);


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

        // Hold interactions
        if (currentInteractableHold != null) {
            if (interactAction.WasPressedThisFrame())
                currentInteractableHold.BeginInteract(this);

            if (interactAction.WasReleasedThisFrame())
                currentInteractableHold.EndInteract(this);

            return;
        }

        // Single press interactions
        if (currentInteractablePress != null &&
            interactAction.triggered) {
            currentInteractablePress.Interact(this);
        }
    }
    #endregion

    #endregion
}