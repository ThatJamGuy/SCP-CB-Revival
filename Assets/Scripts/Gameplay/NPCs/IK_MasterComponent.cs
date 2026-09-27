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

    [Header("Other Settings")]
    public bool enableLeftArmIK;

    [Header("References")]
    [SerializeField] private Rig headRig;
    [SerializeField] private Transform headTransform;
    [SerializeField] private Transform headIkTarget;

    private float trackingRadiusSqr;

    #region Unity Callbacks

    private void Start() {
        trackingRadiusSqr = trackingRadius * trackingRadius;

        if (findPoisOnStart) {
            var foundPois = FindObjectsByType<IK_PointOfInterest>();
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
    }

    #endregion
}