using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Local component script that identifies objects the player can interact with and handles the interaction.
/// Also drives the hand icon shown when near an interactable.
/// </summary>
public class PlayerInteraction : MonoBehaviour {
    [Header("Interaction Settings")]
    [SerializeField] private string interactTag = "Interact";
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private float interactRadius = 2.8f;
    [SerializeField] private float interactIconScreenPadding = 20f;

    [Header("Look Priority")]
    [SerializeField, Range(0f, 1f)] private float lookWeight = 0.7f; // 0 - closest obj chosen, 1 - looked at object chosen
    [SerializeField] private float minLookDot = -1f;

    [Header("References")]
    [SerializeField] private Camera playerCamera;

    private const int MAX_COLLIDERS = 32;
    private const float MIN_SQR_DISTANCE = 0.0001f;

    // Lifts the evaluated point slightly so the linecast doesn't skim the item's surface
    private static readonly Vector3 HitOffset = new Vector3(0f, 0.02f, 0f);

    private readonly Collider[] hitColliders = new Collider[MAX_COLLIDERS];

    private InputAction interactAction;
    private RectTransform canvasRectTransform;
    private Image interactIcon;
    private Transform cameraTransform;

    private Collider currentCollider;
    private IInteractable currentPress;
    private IHoldInteractable currentHold;

    private bool isHolding;
    private bool iconVisible;
    private bool cantFunction;

    #region Unity Lifecycle

    private void Awake() {
        if (playerCamera == null) playerCamera = Camera.main;
        cameraTransform = playerCamera != null ? playerCamera.transform : transform;
    }

    private void Start() {
        if (RevivalRuntimeEngine.Instance == null) {
            DisableWithWarning("InputManager was not found, interacting will not work!");
            return;
        }

        // Will likely later redo this so that instead of not working, it just doesn't show the prompt (No Hud Mode?)
        if (CanvasInstance.Instance == null) {
            DisableWithWarning("CanvasInstance was not found, interacting will not work!");
            return;
        }

        interactAction = RevivalRuntimeEngine.Instance.GetAction("Player", "Interact");
        interactIcon = CanvasInstance.Instance.interactIcon;
        canvasRectTransform = CanvasInstance.Instance.canvasRectTransform;

        if (interactAction == null || interactIcon == null || canvasRectTransform == null) {
            DisableWithWarning("Missing interact action or canvas references, interacting will not work!");
            return;
        }

        iconVisible = interactIcon.gameObject.activeSelf;
    }

    private void Update() {
        if (cantFunction) return;

        if (Player.Instance.disableInput) {
            ReleaseHold();
            return;
        }

        Vector3 camPos = cameraTransform.position;
        Collider best = FindBestInteractable(camPos, cameraTransform.forward);

        SetTarget(best);
        UpdateIcon();
        HandleInteraction();
    }

    private void OnDisable() {
        ReleaseHold();
    }

    #endregion

    #region Target Selection

    private Collider FindBestInteractable(Vector3 camPos, Vector3 camForward) {
        int count = Physics.OverlapSphereNonAlloc(camPos, interactRadius, hitColliders, interactableMask);

        float distWeight = 1f - lookWeight;
        float invRadius = 1f / interactRadius;

        float bestScore = float.MinValue;
        Collider best = null;

        for (int i = 0; i < count; i++) {
            Collider hit = hitColliders[i];

            Vector3 hitPos = hit.bounds.center + HitOffset;
            Vector3 toHit = hitPos - camPos;
            float sqr = toHit.sqrMagnitude;

            // One sqrt + one divide gives both distance and normalized dot
            float dist = 0f;
            float lookDot = 1f;
            if (sqr > MIN_SQR_DISTANCE) {
                float invDist = 1f / Mathf.Sqrt(sqr);
                dist = sqr * invDist;
                lookDot = Vector3.Dot(camForward, toHit) * invDist;
            }

            if (lookDot < minLookDot) continue;

            float distScore = 1f - Mathf.Min(dist * invRadius, 1f);
            float score = (lookDot * 0.5f + 0.5f) * lookWeight + distScore * distWeight;

            // Cheap rejections first, expensive checks (tags, linecast) last
            if (score <= bestScore) continue;
            if (!HasInteractTag(hit.transform)) continue;
            if (Physics.Linecast(camPos, hitPos, obstructionMask)) continue;

            bestScore = score;
            best = hit;
        }

        return best;
    }

    // Handles item prefabs where the tag sits on the collider, its parent, or the root
    private bool HasInteractTag(Transform t) {
        if (t.CompareTag(interactTag)) return true;

        Transform parent = t.parent;
        if (parent != null && parent.CompareTag(interactTag)) return true;

        return t.root.CompareTag(interactTag);
    }

    private void SetTarget(Collider target) {
        if (ReferenceEquals(target, currentCollider)) return;

        // Never leave a hold dangling when the target changes
        ReleaseHold();

        currentCollider = target;

        if (target == null) {
            currentPress = null;
            currentHold = null;
            return;
        }

        // Collider and IInteractable can live on different objects of the same prefab
        GameObject go = target.gameObject;
        currentPress = Resolve<IInteractable>(go);
        currentHold = Resolve<IHoldInteractable>(go);
    }

    private static T Resolve<T>(GameObject go) where T : class {
        if (go.TryGetComponent(out T result)) return result;

        result = go.GetComponentInParent<T>();
        if (result != null) return result;

        return go.GetComponentInChildren<T>();
    }

    #endregion

    #region Icon

    private void UpdateIcon() {
        // Unity null check on purpose: the collider may have been destroyed
        if (currentCollider == null) {
            SetIconVisible(false);
            return;
        }

        PositionIcon(currentCollider.bounds.center);
        SetIconVisible(true);
    }

    private void PositionIcon(Vector3 worldPos) {
        Vector3 screenPos = playerCamera.WorldToScreenPoint(worldPos);
        float width = Screen.width;
        float height = Screen.height;

        // Flip mirrored coordinates when behind camera so clamping pushes to the correct edge
        if (screenPos.z <= 0f) {
            screenPos.x = width - screenPos.x;
            screenPos.y = height - screenPos.y;
        }

        screenPos.x = Mathf.Clamp(screenPos.x, interactIconScreenPadding, width - interactIconScreenPadding);
        screenPos.y = Mathf.Clamp(screenPos.y, interactIconScreenPadding, height - interactIconScreenPadding);

        // Converts screen pixels to canvas-local space (null camera = Screen Space - Overlay canvas)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPos, null, out Vector2 canvasPos);
        interactIcon.rectTransform.anchoredPosition = canvasPos;
    }

    private void SetIconVisible(bool visible) {
        if (iconVisible == visible) return;

        iconVisible = visible;
        interactIcon.gameObject.SetActive(visible);
    }

    #endregion

    #region Interaction Input

    private void HandleInteraction() {
        if (currentCollider == null) return;

        // Hold interactions
        if (currentHold != null) {
            if (interactAction.WasPressedThisFrame()) {
                isHolding = true;
                currentHold.BeginInteract(this);
            } else if (isHolding && interactAction.WasReleasedThisFrame()) {
                isHolding = false;
                currentHold.EndInteract(this);
            }

            return;
        }

        // Single press interactions
        if (currentPress != null && interactAction.triggered) {
            currentPress.Interact(this);
        }
    }

    private void ReleaseHold() {
        if (!isHolding) return;

        isHolding = false;
        if (currentHold != null) currentHold.EndInteract(this);
    }

    #endregion

    private void DisableWithWarning(string message) {
        cantFunction = true;
        Debug.LogWarning($"[PlayerInteraction] {message}", this);
    }
}