using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Master handling class for various IK systems on NPCs.
/// Can work on it's own but contains various helpers for scripted events and the like.
/// </summary>
public class IK_MasterComponent : MonoBehaviour {
    public List<IK_PointOfInterest> pointsOfInterest;

    [Header("Head IK Settings")]
    public bool enableHeadIK;
    public bool findPoisOnStart;
    public float trackingRadius = 10;
    public float retargetSpeed = 5;
    public float maxAngle = 90;

    [Header("Left Hand IK Settings")]
    public bool enableLeftHandIK;
    public float leftHandBlendSpeed = 5f;

    [Header("Two Handed Grip (Optional)")]
    [SerializeField] private Transform gripReference;
    [SerializeField] private Transform leftHandIkTarget;

    [Header("References")]
    [SerializeField] private Rig headRig;
    [SerializeField] private Rig leftArmRig;
    [SerializeField] private Transform headTransform;
    [SerializeField] private Transform headIkTarget;

    private bool hasLeftArmRig;
    private bool hasGrip;
    private float trackingRadiusSqr;

    #region Unity Lifecycle

    private void Start() {
        trackingRadiusSqr = trackingRadius * trackingRadius;
        hasLeftArmRig = leftArmRig != null;
        RefreshGripCache();

        if (findPoisOnStart) {
            IK_PointOfInterest[] foundPois = FindObjectsByType<IK_PointOfInterest>();
            pointsOfInterest = new List<IK_PointOfInterest>(foundPois);
        }
    }

    private void Update() {
        Transform tracking = null;

        Vector3 forward = transform.forward;

        if (enableHeadIK && pointsOfInterest != null) {
            Vector3 headPos = headTransform.position;
            float maxAngleCos = Mathf.Cos(maxAngle * Mathf.Deg2Rad);

            for (int i = 0; i < pointsOfInterest.Count; i++) {
                Transform poiTransform = pointsOfInterest[i].transform;
                Vector3 delta = poiTransform.position - headPos;
                float sqrDist = delta.sqrMagnitude;

                if (sqrDist < trackingRadiusSqr &&
                    Vector3.Dot(forward, delta) > maxAngleCos * Mathf.Sqrt(sqrDist)) {
                    tracking = poiTransform;
                    break;
                }
            }
        }

        float targetWeight = 0f;
        Vector3 targetPos = transform.position + forward * 2f;

        if (enableHeadIK && tracking != null) {
            targetWeight = 1f;
            targetPos = tracking.position;
        }

        headIkTarget.position = Vector3.Lerp(headIkTarget.position, targetPos, Time.deltaTime * retargetSpeed);
        headRig.weight = Mathf.Lerp(headRig.weight, targetWeight, Time.deltaTime * 2f);

        UpdateLeftHandWeight();
    }

    private void LateUpdate() {
        if (!hasGrip) return;

        // Keep following while blending out so the hand doesn't snap away from the object
        if (enableLeftHandIK || leftArmRig.weight > 0f)
            leftHandIkTarget.SetPositionAndRotation(gripReference.position, gripReference.rotation);
    }

    #endregion

    #region Private Methods

    private void UpdateLeftHandWeight() {
        if (!hasLeftArmRig) return;

        float target = enableLeftHandIK ? 1f : 0f;
        float current = leftArmRig.weight;

        // MoveTowards settles exactly on the target, so no writes happen once blended
        if (current != target) leftArmRig.weight = Mathf.MoveTowards(current, target, Time.deltaTime * leftHandBlendSpeed);
    }

    private void RefreshGripCache() {
        hasGrip = hasLeftArmRig && gripReference != null && leftHandIkTarget != null;
    }

    #endregion

    #region Public Methods

    public void SetLeftHandIK(bool enabled) => enableLeftHandIK = enabled;

    public void SetGripReference(Transform grip, bool enableIK = true) {
        gripReference = grip;
        RefreshGripCache();

        if (enableIK && grip != null) enableLeftHandIK = true;
    }

    public void ClearGripReference() {
        gripReference = null;
        RefreshGripCache();
        enableLeftHandIK = false;
    }

    #endregion
}