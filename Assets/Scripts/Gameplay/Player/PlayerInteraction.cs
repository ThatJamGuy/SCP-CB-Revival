using System;
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
    [SerializeField] private float interactRadius = 2.8f;
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
    private const int MAX_COLLIDERS = 32;

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
    private void DetermineClosestInteractable()
    {
        //switched to camera postion rather then previous implemenation that was transform position
        //Correctly matches player eye level
        var camPos = cameraTransform != null ? cameraTransform.position : transform.position;
        var camForward = cameraTransform != null ? cameraTransform.forward : transform.forward;

        //Changed from transform.position to camPos so that interacting follows the camera height
        int numColliders = Physics.OverlapSphereNonAlloc(camPos, interactRadius, hitColliders, interactableMask);   

        float distWeight = 1f - lookWeight;
        float invRadius = 1f / interactRadius;

        var bestScore = float.MinValue;
        GameObject closestInteractable = null;

        // For every collider in the cool circle area around the player...
        for (int i = 0; i < numColliders; i++)
        {
            Collider hit = hitColliders[i];

            //Basically Handles item prefabs interactions
            bool hasTag = hit.CompareTag(interactTag) || (hit.transform.parent != null && hit.transform.parent.CompareTag(interactTag)) || hit.transform.root.CompareTag(interactTag);
            if (!hasTag) continue;

            // Evaluates from collider center with vertical offest to prevent linecast from not hitting the item
            Vector3 hitPosition = hit.bounds.center + (Vector3.up * 0.02f);

            var toHit = hitPosition - camPos;

            var toHitFromCam = hitPosition - camPos;
            float dist = Mathf.Sqrt(toHit.sqrMagnitude);
            float distScore = 1f - Mathf.Min(dist * invRadius, 1f);
            float camSqr = toHitFromCam.sqrMagnitude;
            float lookDot = camSqr > 0.0001f ? Vector3.Dot(camForward, toHitFromCam) / Mathf.Sqrt(camSqr) : 1f;

            if (lookDot < minLookDot) continue;

            float score = (lookDot * 0.5f + 0.5f) * lookWeight + distScore * distWeight;

            if (score <= bestScore) continue;

            // Validates linecast line of sight
            if (Physics.Linecast(camPos, hitPosition, out RaycastHit obsHit, obstructionMask))
            {
                if (obsHit.collider.transform.root != hit.transform.root)
                {
                    continue;
                }
            }

            bestScore = score;
            closestInteractable = hit.gameObject;

            }

        if (!ReferenceEquals(closestInteractable, currentInteractable))
        {
            currentInteractable = closestInteractable;

            //Fixes items when their collider or IInteractable are on different objects in the same prefab.
            currentInteractablePress = closestInteractable != null ? closestInteractable.GetComponentInParent<IInteractable>() ?? closestInteractable.GetComponentInChildren<IInteractable>() : null;
            currentInteractableHold = closestInteractable != null ? closestInteractable.GetComponentInParent<IHoldInteractable>() ?? closestInteractable.GetComponentInChildren<IHoldInteractable>() : null;
        }


        if (closestInteractable)
        {
            //Projects to visual collider center instead of root transform origin
            Collider col = closestInteractable.GetComponent<Collider>();
            Vector3 iconPos = col != null ? col.bounds.center : closestInteractable.transform.position;
            TryToShowInteractIcon(iconPos);
        }
        else if (interactIcon.gameObject != null)
        {
            interactIcon.gameObject.SetActive(false);
        }
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