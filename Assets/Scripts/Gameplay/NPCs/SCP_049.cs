using UnityEngine;

public class SCP_049 : MonoBehaviour {
    [Header("Vision Settings")]
    [SerializeField] private Transform headBone;
    [SerializeField] private float visionRange = 20f;
    [SerializeField] private float visionAngle = 90f;
    [SerializeField] private float visionCheckInterval = 0.15f;
    [SerializeField] private LayerMask visionBlockMask;
    [SerializeField] private LayerMask targetMask;

    private Transform currentTarget;
    private bool hasTarget;
    private float visionTimer;

    private Transform VisionOrigin => headBone ? headBone : transform;

    #region Unity Callbacks
    
    private void Update() {
        visionTimer += Time.deltaTime;

        if (visionTimer >= visionCheckInterval) {
            visionTimer = 0f;
            CheckVision();
        }
    }
    #endregion

    #region Private Methods
    
    private void CheckVision() {
        hasTarget = false;
        currentTarget = null;

        var candidates = Physics.OverlapSphere(
            VisionOrigin.position,
            visionRange,
            targetMask
        );

        foreach (var col in candidates) {
            if (!IsInCone(col.transform) || !HasLineOfSight(col.transform))
                continue;

            currentTarget = col.transform;
            hasTarget = true;
            return;
        }
    }

    private bool IsInCone(Transform target) {
        var toTarget = (target.position - VisionOrigin.position).normalized;
        return Vector3.Angle(VisionOrigin.forward, toTarget) <= visionAngle * 0.5f;
    }

    private bool HasLineOfSight(Transform target) {
        var origin = VisionOrigin.position;
        var toTarget = target.position - origin;

        return Physics.Raycast(
            origin,
            toTarget.normalized,
            out RaycastHit hit,
            visionRange,
            visionBlockMask | targetMask
        ) && hit.transform == target;
    }
    #endregion

    #region Debug
#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        Transform origin = VisionOrigin;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.1f);
        Gizmos.DrawSphere(origin.position, visionRange);

        float halfAngle = visionAngle * 0.5f;

        Gizmos.color = hasTarget ? Color.red : Color.yellow;
        Gizmos.DrawRay(origin.position, Quaternion.AngleAxis(-halfAngle, origin.up) * origin.forward * visionRange);
        Gizmos.DrawRay(origin.position, Quaternion.AngleAxis(halfAngle, origin.up) * origin.forward * visionRange);
        Gizmos.DrawRay(origin.position, origin.forward * visionRange);
    }
#endif
    #endregion
}