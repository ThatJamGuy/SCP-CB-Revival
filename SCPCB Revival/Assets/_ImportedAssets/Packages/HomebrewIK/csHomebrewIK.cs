/*
 * Created :    Spring 2022
 * Author :     SeungGeon Kim (keithrek@hanmail.net)
 * Project :    HomebrewIK
 * Filename :   csHomebrewIK.cs (non-static monobehaviour module)
 * 
 * All Content (C) 2022 Unlimited Fischl Works, all rights reserved.
 */



using System;       // Convert
using UnityEngine;  // Monobehaviour
using UnityEditor;  // Handles



namespace FischlWorks
{



    public class csHomebrewIK : MonoBehaviour
    {
        private Animator playerAnimator = null;

        private Transform leftFootTransform = null;
        private Transform rightFootTransform = null;

        private Transform leftFootOrientationReference = null;
        private Transform rightFootOrientationReference = null;

        private Vector3 initialForwardVector = new Vector3();

        public float _LengthFromHeelToToes {
            get { return lengthFromHeelToToes; }
        }

        public float _RaySphereRadius {
            get { return raySphereRadius; }
        }

        public float _LeftFootProjectedAngle {
            get { return leftFootProjectedAngle; }
        }

        public float _RightFootProjectedAngle {
            get { return rightFootProjectedAngle; }
        }

        public Vector3 _LeftFootIKPositionTarget {
            get {
                if (Application.isPlaying == true)
                {
                    return leftFootIKPositionTarget;
                }
                else
                {
                    // This is being done because the IK target only gets updated during playmode
                    return new Vector3(0, GetAnkleHeight() + _WorldHeightOffset, 0);
                }
            }
        }

        public Vector3 _RightFootIKPositionTarget {
            get {
                if (Application.isPlaying == true)
                {
                    return rightFootIKPositionTarget;
                }
                else
                {
                    // This is being done because the IK target only gets updated during playmode
                    return new Vector3(0, GetAnkleHeight() + _WorldHeightOffset, 0);
                }
            }
        }

        public float _AnkleHeightOffset {
            get {
                return ankleHeightOffset;
            }
        }

        public float _WorldHeightOffset {
            get {
                if (giveWorldHeightOffset == true)
                {
                    return worldHeightOffset;
                }
                else
                {
                    return 0;
                }
            }
        }

        [BigHeader("Foot Properties")]

        [SerializeField]
        [Range(0, 0.25f)]
        private float lengthFromHeelToToes = 0.1f;
        [SerializeField]
        [Range(0, 60)]
        private float maxRotationAngle = 45;
        [SerializeField]
        [Range(-0.05f, 0.125f)]
        private float ankleHeightOffset = 0;

        [BigHeader("IK Properties")]

        [SerializeField]
        private bool enableIKPositioning = true;
        [SerializeField]
        private bool enableIKRotating = true;
        [SerializeField]
        [Range(0, 1)]
        private float globalWeight = 1;
        [SerializeField]
        [Range(0, 1)]
        private float leftFootWeight = 1;
        [SerializeField]
        [Range(0, 1)]
        private float rightFootWeight = 1;
        [SerializeField]
        [Range(0, 0.1f)]
        private float smoothTime = 0.075f;

        [BigHeader("Ray Properties")]

        [SerializeField]
        [Range(0.05f, 0.1f)]
        private float raySphereRadius = 0.05f;
        [SerializeField]
        [Range(0.1f, 2)]
        private float rayCastRange = 2;
        [SerializeField]
        private LayerMask groundLayers = new LayerMask();
        [SerializeField]
        private bool ignoreTriggers = true;

        [BigHeader("Raycast Start Heights")]

        [SerializeField]
        [Range(0.1f, 1)]
        private float leftFootRayStartHeight = 0.5f;
        [SerializeField]
        [Range(0.1f, 1)]
        private float rightFootRayStartHeight = 0.5f;

        [BigHeader("Advanced")]

        [SerializeField]
        private bool enableFootLifting = true;
        [ShowIf("enableFootLifting")]
        [SerializeField]
        private float floorRange = 0;
        [SerializeField]
        private bool enableBodyPositioning = true;
        [ShowIf("enableBodyPositioning")]
        [SerializeField]
        private float crouchRange = 0.25f;
        [ShowIf("enableBodyPositioning")]
        [SerializeField]
        private float stretchRange = 0;
        [SerializeField]
        private bool giveWorldHeightOffset = false;
        [ShowIf("giveWorldHeightOffset")]
        [SerializeField]
        private float worldHeightOffset = 0;

        private RaycastHit leftFootRayHitInfo = new RaycastHit();
        private RaycastHit rightFootRayHitInfo = new RaycastHit();

        private float leftFootRayHitHeight = 0;
        private float rightFootRayHitHeight = 0;

        private Vector3 leftFootRayStartPosition = new Vector3();
        private Vector3 rightFootRayStartPosition = new Vector3();

        private Vector3 leftFootDirectionVector = new Vector3();
        private Vector3 rightFootDirectionVector = new Vector3();

        private Vector3 leftFootProjectionVector = new Vector3();
        private Vector3 rightFootProjectionVector = new Vector3();

        private float leftFootProjectedAngle = 0;
        private float rightFootProjectedAngle = 0;

        private Vector3 leftFootRayHitProjectionVector = new Vector3();
        private Vector3 rightFootRayHitProjectionVector = new Vector3();

        private float leftFootRayHitProjectedAngle = 0;
        private float rightFootRayHitProjectedAngle = 0;

        private float leftFootHeightOffset = 0;
        private float rightFootHeightOffset = 0;

        private Vector3 leftFootIKPositionBuffer = new Vector3();
        private Vector3 rightFootIKPositionBuffer = new Vector3();

        private Vector3 leftFootIKPositionTarget = new Vector3();
        private Vector3 rightFootIKPositionTarget = new Vector3();

        private float leftFootHeightLerpVelocity = 0;
        private float rightFootHeightLerpVelocity = 0;

        private Vector3 leftFootIKRotationBuffer = new Vector3();
        private Vector3 rightFootIKRotationBuffer = new Vector3();

        private Vector3 leftFootIKRotationTarget = new Vector3();
        private Vector3 rightFootIKRotationTarget = new Vector3();

        private Vector3 leftFootRotationLerpVelocity = new Vector3();
        private Vector3 rightFootRotationLerpVelocity = new Vector3();

        private GUIStyle helperTextStyle = null;



        // --- --- ---



        private void Start()
        {
            InitializeVariables();

            CreateOrientationReference();
        }



        private void Update()
        {
            UpdateFootProjection();

            UpdateRayHitInfo();

            UpdateIKPositionTarget();
            UpdateIKRotationTarget();
        }



        private void OnAnimatorIK()
        {
            LerpIKBufferToTarget();

            ApplyFootIK();
            ApplyBodyIK();
        }



        // --- --- ---



        private void InitializeVariables()
        {
            playerAnimator = GetComponent<Animator>();

            leftFootTransform = playerAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFootTransform = playerAnimator.GetBoneTransform(HumanBodyBones.RightFoot);

            // This is for faster development iteration purposes
            if (groundLayers.value == 0)
            {
                groundLayers = LayerMask.GetMask("Default");
            }

            // This is needed in order to wrangle with quaternions to get the final direction vector of each foot later
            initialForwardVector = transform.forward;

            // Initial value is given to make the first frames of lerping look natural, rotations should not need these
            leftFootIKPositionBuffer.y = transform.position.y + GetAnkleHeight();
            rightFootIKPositionBuffer.y = transform.position.y + GetAnkleHeight();

            // This is being done here due to internal unity reasons
            helperTextStyle = new GUIStyle()
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            helperTextStyle.normal.textColor = Color.yellow;
        }



        // This is being done to track bone orientation, since we cannot use footTransform's rotation in its own anyway
        private void CreateOrientationReference()
        {
            /* Just in case that this function gets called again... */

            if (leftFootOrientationReference != null)
            {
                Destroy(leftFootOrientationReference);
            }

            if (rightFootOrientationReference != null)
            {
                Destroy(rightFootOrientationReference);
            }

            /* These gameobjects hold different orientation values from footTransform.rotation, but the delta remains the same */

            leftFootOrientationReference = new GameObject("[RUNTIME] Normal_Orientation_Reference").transform;
            rightFootOrientationReference = new GameObject("[RUNTIME] Normal_Orientation_Reference").transform;

            leftFootOrientationReference.position = leftFootTransform.position;
            rightFootOrientationReference.position = rightFootTransform.position;

            leftFootOrientationReference.SetParent(leftFootTransform);
            rightFootOrientationReference.SetParent(rightFootTransform);
        }



        //This is being done because we want to know in what angle did the foot go underground
        private void UpdateFootProjection()
        {
            /* This is the only part in this script (except for those gizmos) that accesses footOrientationReference */

            leftFootDirectionVector = leftFootOrientationReference.rotation * initialForwardVector;
            rightFootDirectionVector = rightFootOrientationReference.rotation * initialForwardVector;

            /* World space based vector defines are used here for the representation of floor orientation */

            leftFootProjectionVector = Vector3.ProjectOnPlane(leftFootDirectionVector, Vector3.up);
            rightFootProjectionVector = Vector3.ProjectOnPlane(rightFootDirectionVector, Vector3.up);

            /* Cross is done in this order because we want the underground angle to be positive */

            leftFootProjectedAngle = Vector3.SignedAngle(
                leftFootProjectionVector,
                leftFootDirectionVector,
                Vector3.Cross(leftFootDirectionVector, leftFootProjectionVector) *
                // This is needed to cancel out the cross product's axis inverting behaviour
                Mathf.Sign(leftFootDirectionVector.y));

            rightFootProjectedAngle = Vector3.SignedAngle(
                rightFootProjectionVector,
                rightFootDirectionVector,
                Vector3.Cross(rightFootDirectionVector, rightFootProjectionVector) *
                // This is needed to cancel out the cross product's axis inverting behaviour
                Mathf.Sign(rightFootDirectionVector.y));
        }



        private void UpdateRayHitInfo()
        {
            /* Rays will be casted from above each foot, in the downward orientation of the world */

            leftFootRayStartPosition = leftFootTransform.position;
            leftFootRayStartPosition.y += leftFootRayStartHeight;

            rightFootRayStartPosition = rightFootTransform.position;
            rightFootRayStartPosition.y += rightFootRayStartHeight;

            /* SphereCast is used here just because we need a normal vector to rotate our foot towards */

            // Vector3.up is used here instead of transform.up to get normal vector in world orientation
            Physics.SphereCast(
                leftFootRayStartPosition,
                raySphereRadius,
                Vector3.up * -1,
                out leftFootRayHitInfo, rayCastRange, groundLayers,
                (QueryTriggerInteraction)(2 - Convert.ToInt32(ignoreTriggers)));

            // Vector3.up is used here instead of transform.up to get normal vector in world orientation
            Physics.SphereCast(
                rightFootRayStartPosition,
                raySphereRadius,
                Vector3.up * -1,
                out rightFootRayHitInfo, rayCastRange, groundLayers,
                (QueryTriggerInteraction)(2 - Convert.ToInt32(ignoreTriggers)));

            // Left Foot Ray Handling
            if (leftFootRayHitInfo.collider != null)
            {
                leftFootRayHitHeight = leftFootRayHitInfo.point.y;

                /* Angle from the floor is also calculated to isolate the rotation caused by the animation */

                // We are doing this crazy operation because we only want to count rotations that are parallel to the foot
                leftFootRayHitProjectionVector = Vector3.ProjectOnPlane(
                    leftFootRayHitInfo.normal,
                    Vector3.Cross(leftFootDirectionVector, leftFootProjectionVector));

                leftFootRayHitProjectedAngle = Vector3.Angle(
                    leftFootRayHitProjectionVector,
                    Vector3.up);
            }
            else
            {
                leftFootRayHitHeight = transform.position.y;
            }

            // Right Foot Ray Handling
            if (rightFootRayHitInfo.collider != null)
            {
                rightFootRayHitHeight = rightFootRayHitInfo.point.y;

                /* Angle from the floor is also calculated to isolate the rotation caused by the animation */

                // We are doing this crazy operation because we only want to count rotations that are parallel to the foot
                rightFootRayHitProjectionVector = Vector3.ProjectOnPlane(
                    rightFootRayHitInfo.normal,
                    Vector3.Cross(rightFootDirectionVector, rightFootProjectionVector));

                rightFootRayHitProjectedAngle = Vector3.Angle(
                    rightFootRayHitProjectionVector,
                    Vector3.up);
            }
            else
            {
                rightFootRayHitHeight = transform.position.y;
            }
        }



        private void UpdateIKPositionTarget()
        {
            /* We reset the offset values here instead of declaring them as local variables, since other functions reference it */

            leftFootHeightOffset = 0;
            rightFootHeightOffset = 0;

            /* Foot height correction based on the projected angle */

            float trueLeftFootProjectedAngle = leftFootProjectedAngle - leftFootRayHitProjectedAngle;

            if (trueLeftFootProjectedAngle > 0)
            {
                leftFootHeightOffset += Mathf.Abs(Mathf.Sin(
                    Mathf.Deg2Rad * trueLeftFootProjectedAngle) *
                    lengthFromHeelToToes);

                // There's no Abs here to support negative manual height offset
                leftFootHeightOffset += Mathf.Cos(
                    Mathf.Deg2Rad * trueLeftFootProjectedAngle) *
                    GetAnkleHeight();
            }
            else
            {
                leftFootHeightOffset += GetAnkleHeight();
            }

            /* Foot height correction based on the projected angle */

            float trueRightFootProjectedAngle = rightFootProjectedAngle - rightFootRayHitProjectedAngle;

            if (trueRightFootProjectedAngle > 0)
            {
                rightFootHeightOffset += Mathf.Abs(Mathf.Sin(
                    Mathf.Deg2Rad * trueRightFootProjectedAngle) *
                    lengthFromHeelToToes);

                // There's no Abs here to support negative manual height offset
                rightFootHeightOffset += Mathf.Cos(
                    Mathf.Deg2Rad * trueRightFootProjectedAngle) *
                    GetAnkleHeight();
            }
            else
            {
                rightFootHeightOffset += GetAnkleHeight();
            }

            /* Application of calculated position */

            leftFootIKPositionTarget.y = leftFootRayHitHeight + leftFootHeightOffset + _WorldHeightOffset;
            rightFootIKPositionTarget.y = rightFootRayHitHeight + rightFootHeightOffset + _WorldHeightOffset;
        }



        private void UpdateIKRotationTarget()
        {
            if (leftFootRayHitInfo.collider != null)
            {
                leftFootIKRotationTarget = Vector3.Slerp(
                    transform.up,
                    leftFootRayHitInfo.normal,
                    Mathf.Clamp(Vector3.Angle(transform.up, leftFootRayHitInfo.normal), 0, maxRotationAngle) /
                    // Addition of 1 is to prevent division by zero, not a perfect solution but somehow works
                    (Vector3.Angle(transform.up, leftFootRayHitInfo.normal) + 1));
            }
            else
            {
                leftFootIKRotationTarget = transform.up;
            }

            if (rightFootRayHitInfo.collider != null)
            {
                rightFootIKRotationTarget = Vector3.Slerp(
                    transform.up,
                    rightFootRayHitInfo.normal,
                    Mathf.Clamp(Vector3.Angle(transform.up, rightFootRayHitInfo.normal), 0, maxRotationAngle) /
                    // Addition of 1 is to prevent division by zero, not a perfect solution but somehow works
                    (Vector3.Angle(transform.up, rightFootRayHitInfo.normal) + 1));
            }
            else
            {
                rightFootIKRotationTarget = transform.up;
            }
        }



        private void LerpIKBufferToTarget()
        {
            /* Instead of wrangling with weights, we switch the lerp targets to make movement smooth */

            if (enableFootLifting == true &&
                playerAnimator.GetIKPosition(AvatarIKGoal.LeftFoot).y >=
                leftFootIKPositionTarget.y + floorRange)
            {
                leftFootIKPositionBuffer.y = Mathf.SmoothDamp(
                    leftFootIKPositionBuffer.y,
                    playerAnimator.GetIKPosition(AvatarIKGoal.LeftFoot).y,
                    ref leftFootHeightLerpVelocity,
                    smoothTime);
            }
            else 
            {
                leftFootIKPositionBuffer.y = Mathf.SmoothDamp(
                    leftFootIKPositionBuffer.y,
                    leftFootIKPositionTarget.y,
                    ref leftFootHeightLerpVelocity,
                    smoothTime);
            }
            
            if (enableFootLifting == true &&
                playerAnimator.GetIKPosition(AvatarIKGoal.RightFoot).y >=
                rightFootIKPositionTarget.y + floorRange)
            {
                rightFootIKPositionBuffer.y = Mathf.SmoothDamp(
                    rightFootIKPositionBuffer.y,
                    playerAnimator.GetIKPosition(AvatarIKGoal.RightFoot).y,
                    ref rightFootHeightLerpVelocity,
                    smoothTime);
            }
            else 
            {
                rightFootIKPositionBuffer.y = Mathf.SmoothDamp(
                    rightFootIKPositionBuffer.y,
                    rightFootIKPositionTarget.y,
                    ref rightFootHeightLerpVelocity,
                    smoothTime);
            }

            leftFootIKRotationBuffer = Vector3.SmoothDamp(
                leftFootIKRotationBuffer,
                leftFootIKRotationTarget,
                ref leftFootRotationLerpVelocity,
                smoothTime);

            rightFootIKRotationBuffer = Vector3.SmoothDamp(
                rightFootIKRotationBuffer,
                rightFootIKRotationTarget,
                ref rightFootRotationLerpVelocity,
                smoothTime);
        }



        private void ApplyFootIK()
        {
            /* Weight designation */

            playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, globalWeight * leftFootWeight);
            playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, globalWeight * rightFootWeight);
            
            playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, globalWeight * leftFootWeight);
            playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightFoot, globalWeight * rightFootWeight);

            /* Position handling */

            CopyByAxis(ref leftFootIKPositionBuffer, playerAnimator.GetIKPosition(AvatarIKGoal.LeftFoot),
                true, false, true);

            CopyByAxis(ref rightFootIKPositionBuffer, playerAnimator.GetIKPosition(AvatarIKGoal.RightFoot),
                true, false, true);

            if (enableIKPositioning == true)
            {
                playerAnimator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootIKPositionBuffer);
                playerAnimator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootIKPositionBuffer);
            }

            /* Rotation handling */

            /* This part may be a bit tricky to understand intuitively, refer to docs for an explanation in depth */

            // FromToRotation is used because we need the delta, not the final target orientation
            Quaternion leftFootRotation =
                Quaternion.FromToRotation(transform.up, leftFootIKRotationBuffer) *
                playerAnimator.GetIKRotation(AvatarIKGoal.LeftFoot);

            // FromToRotation is used because we need the delta, not the final target orientation
            Quaternion rightFootRotation =
                Quaternion.FromToRotation(transform.up, rightFootIKRotationBuffer) *
                playerAnimator.GetIKRotation(AvatarIKGoal.RightFoot);

            if (enableIKRotating == true)
            {
                playerAnimator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRotation);
                playerAnimator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootRotation);
            }
        }



        private void ApplyBodyIK()
        {
            if (enableBodyPositioning == false)
            {
                return;
            }

            float minFootHeight = Mathf.Min(
                    playerAnimator.GetIKPosition(AvatarIKGoal.LeftFoot).y,
                    playerAnimator.GetIKPosition(AvatarIKGoal.RightFoot).y);

            /* This part moves the body 'downwards' by the root gameobject's height */

            playerAnimator.bodyPosition = new Vector3(
            playerAnimator.bodyPosition.x,
            playerAnimator.bodyPosition.y +
            LimitValueByRange(minFootHeight - transform.position.y, 0),
            playerAnimator.bodyPosition.z);
        }



        private float GetAnkleHeight()
        {
            return raySphereRadius + _AnkleHeightOffset;
        }



        private void CopyByAxis(ref Vector3 target, Vector3 source, bool copyX, bool copyY, bool copyZ)
        {
            target = new Vector3(
                Mathf.Lerp(
                    target.x,
                    source.x,
                    Convert.ToInt32(copyX)),
                Mathf.Lerp(
                    target.y,
                    source.y,
                    Convert.ToInt32(copyY)),
                Mathf.Lerp(
                    target.z,
                    source.z,
                    Convert.ToInt32(copyZ)));
        }



        private float LimitValueByRange(float value, float floor)
        {
            if (value < floor - stretchRange)
            {
                return value + stretchRange;
            }
            else if (value > floor + crouchRange)
            {
                return value - crouchRange;
            }
            else
            {
                return floor;
            }
        }



#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Debug draw function relies on objects that are dynamically located during runtime
            if (Application.isPlaying == false)
            {
                return;
            }

            /* Left Foot */

            if (leftFootRayHitInfo.collider != null)
            {
                Handles.color = Color.yellow;

                // Just note that the normal vector of RayCastHit object is used here
                Handles.DrawWireDisc(
                    leftFootRayHitInfo.point,
                    leftFootRayHitInfo.normal,
                    0.1f);
                Handles.DrawDottedLine(
                    leftFootTransform.position,
                    leftFootTransform.position + leftFootRayHitInfo.normal,
                    2);

                // Just note that the orientation of the parent transform is used here
                Handles.color = Color.green;
                Handles.DrawWireDisc(
                    leftFootRayHitInfo.point,
                    transform.up, 0.25f);

                Gizmos.color = Color.green;

                Gizmos.DrawSphere(
                    leftFootRayHitInfo.point + transform.up * raySphereRadius,
                    raySphereRadius);
            }
            else
            {
                Gizmos.color = Color.red;
            }

            if (leftFootProjectedAngle > 0)
            {
                Handles.color = Color.blue;
            }
            else
            {
                Handles.color = Color.red;
            }

            // Foot height correction related debug draws
            Handles.DrawWireDisc(
                leftFootTransform.position,
                leftFootOrientationReference.rotation * transform.up,
                0.15f);
            Handles.DrawSolidArc(
                leftFootTransform.position,
                Vector3.Cross(leftFootDirectionVector, leftFootProjectionVector) * -1,
                leftFootProjectionVector,
                // Abs is needed here because the cross product will deal with axis direction
                Mathf.Abs(leftFootProjectedAngle),
                0.25f);
            Handles.DrawDottedLine(
                leftFootTransform.position,
                leftFootTransform.position + leftFootDirectionVector.normalized,
                2);

            // SphereCast related debug draws
            Gizmos.DrawWireSphere(
                leftFootRayStartPosition,
                0.1f);
            Gizmos.DrawLine(
                leftFootRayStartPosition,
                leftFootRayStartPosition - rayCastRange * Vector3.up);

            // Indicator text
            Handles.Label(leftFootTransform.position, "L", helperTextStyle);

            /* Right foot */

            if (rightFootRayHitInfo.collider != null)
            {
                Handles.color = Color.yellow;

                // Just note that the normal vector of RayCastHit object is used here
                Handles.DrawWireDisc(
                    rightFootRayHitInfo.point,
                    rightFootRayHitInfo.normal,
                    0.1f);
                Handles.DrawDottedLine(
                    rightFootTransform.position,
                    rightFootTransform.position + rightFootRayHitInfo.normal,
                    2);

                // Just note that the orientation of the parent transform is used here
                Handles.color = Color.green;
                Handles.DrawWireDisc(
                    rightFootRayHitInfo.point,
                    transform.up, 0.25f);

                Gizmos.color = Color.green;

                Gizmos.DrawSphere(
                    rightFootRayHitInfo.point + transform.up * raySphereRadius,
                    raySphereRadius);
            }
            else
            {
                Gizmos.color = Color.red;
            }

            if (rightFootProjectedAngle > 0)
            {
                Handles.color = Color.blue;
            }
            else
            {
                Handles.color = Color.red;
            }

            // Foot height correction related debug draws
            Handles.DrawWireDisc(
                rightFootTransform.position,
                rightFootOrientationReference.rotation * transform.up,
                0.15f);
            Handles.DrawSolidArc(
                rightFootTransform.position,
                Vector3.Cross(rightFootDirectionVector, rightFootProjectionVector) * -1,
                rightFootProjectionVector,
                // Abs is needed here because the cross product will deal with axis direction
                Mathf.Abs(rightFootProjectedAngle),
                0.25f);
            Handles.DrawDottedLine(
                rightFootTransform.position,
                rightFootTransform.position + rightFootDirectionVector.normalized,
                2);

            // SphereCast related debug draws
            Gizmos.DrawWireSphere(
                rightFootRayStartPosition,
                0.1f);
            Gizmos.DrawLine(
                rightFootRayStartPosition,
                rightFootRayStartPosition - rayCastRange * Vector3.up);

            // Indicator text
            Handles.Label(rightFootTransform.position, "R", helperTextStyle);
        }
#endif
    }



    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string _BaseCondition {
            get { return mBaseCondition; }
        }

        private string mBaseCondition = String.Empty;

        public ShowIfAttribute(string baseCondition)
        {
            mBaseCondition = baseCondition;
        }
    }



    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class BigHeaderAttribute : PropertyAttribute
    {
        public string _Text {
            get { return mText; }
        }

        private string mText = String.Empty;

        public BigHeaderAttribute(string text)
        {
            mText = text;
        }
    }



}