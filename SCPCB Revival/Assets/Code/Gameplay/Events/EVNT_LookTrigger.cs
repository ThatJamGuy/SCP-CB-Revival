using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public class EVNT_LookTrigger : MonoBehaviour {
    [Header("Settings")]
    public Camera playerCamera;
    [OnValueChanged("OnConfigChanged")] public bool useMaxDistance = true;
    [ShowIf("useMaxDistance")] public float maxDistance = 10f;
    [OnValueChanged("OnConfigChanged")] public bool useLineOfSight = true;
    public LayerMask obstructionMask = ~0;
    [Space]
    [OnValueChanged("OnConfigChanged")] public bool useColliderCheck = false;
    [ShowIf("useColliderCheck")] public Collider targetCollider;

    [Header("Events")]
    public UnityEvent onLookedAt;

    bool hasTriggered;

    void Awake() {
        if (useColliderCheck && !targetCollider) targetCollider = GetComponent<Collider>();
    }

    void Update() {
        if (!playerCamera) return;

        if (useColliderCheck) {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, useMaxDistance ? maxDistance : Mathf.Infinity, obstructionMask)) {
                if (hit.collider == targetCollider && !hasTriggered) {
                    onLookedAt.Invoke();
                    hasTriggered = true;
                }
            }
        }
        else {
            Vector3 dir = (transform.position - playerCamera.transform.position).normalized;
            float dot = Vector3.Dot(playerCamera.transform.forward, dir);
            if (dot < 0.98f) return;

            float dist = Vector3.Distance(playerCamera.transform.position, transform.position);
            if (useMaxDistance && dist > maxDistance) return;

            if (useLineOfSight) {
                if (Physics.Raycast(playerCamera.transform.position, dir, out RaycastHit hit, dist, obstructionMask))
                    if (hit.collider.transform != transform) return;
            }

            if (!hasTriggered) {
                onLookedAt.Invoke();
                hasTriggered = true;
            }
        }
    }

    void OnConfigChanged() {
        if (!useMaxDistance) maxDistance = 0f;
        if (useColliderCheck && !targetCollider) targetCollider = GetComponent<Collider>();
    }

    public void ResetTrigger() => hasTriggered = false;
}