using System.Collections.Generic;
using System.Linq;
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

        if (findPoisOnStart)
            pointsOfInterest = FindObjectsByType<IK_PointOfInterest>(FindObjectsSortMode.None).ToList();
    }

    private void Update() {
        if (enableHeadIK && pointsOfInterest != null) {
            Transform tracking = null;

            foreach (IK_PointOfInterest poi in pointsOfInterest) {
                Vector3 delta = poi.transform.position - headTransform.position;

                if (delta.sqrMagnitude < trackingRadiusSqr) {
                    float angle = Vector3.Angle(transform.forward, delta);
                    if (angle < maxAngle) {
                        tracking = poi.transform;
                        break;
                    }
                }
            }

            float rigWeight = 0;
            Vector3 targetPos = transform.position + (transform.forward * 2f);
            if (tracking != null) {
                targetPos = tracking.position;
                rigWeight = 1;
            }
            headIkTarget.position = Vector3.Lerp(headIkTarget.position, targetPos, Time.deltaTime * retargetSpeed);
            headRig.weight = Mathf.Lerp(headRig.weight, rigWeight, Time.deltaTime * 2);
        }
    }

    #endregion
}