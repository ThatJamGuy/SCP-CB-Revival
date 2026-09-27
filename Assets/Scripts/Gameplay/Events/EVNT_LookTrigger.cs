using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Script to handle visibility based events. Can base itself on general direction or accurate visibility
/// </summary>
public class EVNT_LookTrigger : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private int maxViewDistance = 10;
    [SerializeField] private bool useAccurateLineOfSight;
    [SerializeField] private bool triggerOnce;

    [Header("References")]
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private Collider thisCollider;

    [Header("Unity Events")]
    public UnityEvent onLookedAt;
    public UnityEvent onLookAway;

    private Camera playerCamera;

    private bool hasTriggered;
    private bool isLooking;

    private void Start() {
        if (Player.Instance != null) playerCamera = Player.Instance.playerCamera;
    }

    private void Update() {
        if (playerCamera == null || (triggerOnce && hasTriggered)) return;

        Transform cameraTransform = playerCamera.transform;
        Vector3 cameraPosition = cameraTransform.position;
        float sqrDistanceToPlayer = (transform.position - cameraPosition).sqrMagnitude;
        bool lookingAtPlayer = false;

        if (sqrDistanceToPlayer <= maxViewDistance * maxViewDistance) {
            Vector3 thisPosition = thisCollider != null ? thisCollider.bounds.center : transform.position;
            Vector3 directionToPlayer = (thisPosition - cameraPosition).normalized;
            float dot = Vector3.Dot(cameraTransform.forward, directionToPlayer);

            if (dot >= 0.98f) {
                if (useAccurateLineOfSight) {
                    float distanceToPlayer = Mathf.Sqrt(sqrDistanceToPlayer);
                    if (Physics.Raycast(cameraPosition, directionToPlayer, out RaycastHit hit, distanceToPlayer, obstructionMask))
                        lookingAtPlayer = hit.collider == thisCollider;
                } else lookingAtPlayer = true;
            }
        }

        if (lookingAtPlayer && !isLooking) {
            onLookedAt.Invoke();

            if (triggerOnce) hasTriggered = true;
        }

        if (!lookingAtPlayer && isLooking) onLookAway.Invoke();

        isLooking = lookingAtPlayer;
    }
}