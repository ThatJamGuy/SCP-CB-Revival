using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IK_HeadTracking : MonoBehaviour
{
    public Transform headIKTarget;
    public Rig headRig;

    public float Radius = 10f;
    public float retargetSpeed = 5f;
    public float maxAngle = 90f;
    
    public List<IK_PontOfInterest> POIs;

    private float RadiusSqr;

    private void Start()
    {
        RadiusSqr = Radius * Radius;
    }

    private void Update()
    {
        Transform tracking  = null;
        foreach (IK_PontOfInterest poi in POIs)
        {
            Vector3 delta = poi.transform.position - transform.position;
            if (delta.sqrMagnitude < RadiusSqr)
            {
                float angle = Vector3.Angle(transform.forward, delta);
                if(angle < maxAngle) {
                    tracking = poi.transform;
                    break;
                }
            }
        }
        float rigWeight = 0;
        Vector3 targetPos = transform.position + (transform.forward * 2f);
        if(tracking != null)
        {
            targetPos = tracking.position;
            rigWeight = 1;
        }
        headIKTarget.position = Vector3.Lerp(headIKTarget.position, targetPos, Time.deltaTime * retargetSpeed);
        headRig.weight = Mathf.Lerp(headRig.weight, rigWeight, Time.deltaTime * 2);
    }
}