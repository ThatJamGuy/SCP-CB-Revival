using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class SCP_049 : MonoBehaviour, IRoamingSCP {
    [Header("IK Things")]
    [SerializeField] private Transform ikHandTarget;
    [SerializeField] private Vector3 buttonHandOffset   = new Vector3(0f, 0f, 0.1f);
    [SerializeField] private Vector3 buttonHandRotation = new Vector3(0f, 0f, 0f);

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private NPC_Locomotion locomotionSystem;
    [SerializeField] private TwoBoneIKConstraint handIKConstraint;

    [Header("Button Interaction")]
    [SerializeField] private float buttonPressDistance = 1f;
    private float ButtonPressDistanceSqr => buttonPressDistance * buttonPressDistance;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask doorLayerMask;
    
    private const float IK_DISTANCE     = 5f;
    private const float IK_DISTANCE_SQR = IK_DISTANCE * IK_DISTANCE;
    private const float IK_BLEND_SPEED  = 5f;
    
    private const float BUTTON_IK_DISTANCE     = 2.5f;
    private const float BUTTON_IK_DISTANCE_SQR = BUTTON_IK_DISTANCE * BUTTON_IK_DISTANCE;
    private const float BUTTON_HAND_SPEED      = IK_BLEND_SPEED * 2f;
    private const float BUTTON_BLEND_SPEED     = 3f;

    // Allocation buffer for OverlapSphere results — avoids per-frame allocation
    private static readonly Collider[] _overlapBuffer = new Collider[8];

    private Transform currentTarget;
    
    private float _buttonBlend;
    
    private Vector3    _ikHandDefaultLocalPosition;
    private Quaternion _ikHandDefaultLocalRotation;
    
    private DoorButton _pressedButton;
    
    private Vector3 _pendingDestination;
    private bool    _hasInterruptedPath;
    private bool    _hasPendingDestination;
    
    private bool _isResumingPath;
    private bool isChasingPlayer;

    #region Unity Callbacks

    private void Start() {
        EntitySystem.Instance.RegisterEntity(this);

        // Cache the IK hand target's default local pose so it can be restored after button use
        _ikHandDefaultLocalPosition = ikHandTarget.localPosition;
        _ikHandDefaultLocalRotation = ikHandTarget.localRotation;

        // Set the default target to the player FOR NOW
        currentTarget = Player.Instance.transform;
    }

    private void Update() {
        UpdateHandIK();
        
        // Temporary for testing
        if (currentTarget != null)
            WalkTo(currentTarget.position);
    }

    #endregion

    #region Private Methods
    
    private void UpdateHandIK() {
        // Try to find a nearby DoorButton whose door is actually closed
        DoorButton nearestButton = GetNearestActionableButton();
        Transform  buttonTarget  = nearestButton != null ? nearestButton.transform : null;

        // Clear the pressed button reference once it has fully left IK range
        if (nearestButton == null)
            _pressedButton = null;

        // If a closed-door button is in range and the door is blocking the path, capture the pending destination first,
        // then redirect movement to the button
        if (nearestButton != null && !_isResumingPath && IsDoorBlockingPath(nearestButton.linkedDoor)) {
            if (!_hasInterruptedPath) {
                _hasPendingDestination = TryCapturePendingDestination();
                _hasInterruptedPath    = true;
            }

            // Keep redirecting toward the button each frame until it gets pressed
            WalkTo(nearestButton.transform.position);
        }

        // Once the button has been pressed and the door has opened, resume the original destination.
        // deferred by one frame to let the agent settle
        if (_hasInterruptedPath && !_isResumingPath && _pressedButton != null && _pressedButton.linkedDoor.isOpen) {
            _isResumingPath     = true;
            _hasInterruptedPath = false;
            StartCoroutine(ResumePathNextFrame());
        }

        // Blend the button influence in or out depending on whether a button is in range
        float buttonBlendTarget = buttonTarget != null ? 1f : 0f;
        _buttonBlend = Mathf.MoveTowards(_buttonBlend, buttonBlendTarget, BUTTON_BLEND_SPEED * Time.deltaTime);

        if (buttonTarget != null) {
            // If the NPC is within the tighter press threshold fire ToggleDoorState exactly once per approach
            bool withinPressRange = (buttonTarget.position - transform.position).sqrMagnitude <= ButtonPressDistanceSqr;

            if (withinPressRange && nearestButton != _pressedButton && !nearestButton.linkedDoor.isOpen) {
                nearestButton.linkedDoor.ToggleDoorState();
                _pressedButton = nearestButton;
            }

            // Do some math stuff to determine the offset from the button to position the hand IK target
            Vector3 rawPosition    = buttonTarget.position;
            Vector3 offsetPosition = buttonTarget.TransformPoint(buttonHandOffset);

            // Lerp between the raw and offset positions based on blend, then drive the hand
            Vector3 targetPosition = Vector3.Lerp(rawPosition, offsetPosition, _buttonBlend);
            ikHandTarget.position  = Vector3.MoveTowards(
                ikHandTarget.position,
                targetPosition,
                BUTTON_HAND_SPEED * Time.deltaTime
            );

            // Slerp the hand rotation from neutral toward the button rotation based on blend
            ikHandTarget.rotation = Quaternion.Slerp(
                Quaternion.identity,
                Quaternion.Euler(buttonHandRotation),
                _buttonBlend
            );

            // Blend the IK constraint weight fully in
            handIKConstraint.weight = Mathf.MoveTowards(
                handIKConstraint.weight,
                1f,
                IK_BLEND_SPEED * Time.deltaTime
            );

            return;
        }

        // If there is just no button then revert all the IK stuff back to default so the hand isn't broken
        if (_buttonBlend > 0f) {
            ikHandTarget.localPosition = Vector3.Lerp(
                _ikHandDefaultLocalPosition,
                ikHandTarget.localPosition,
                _buttonBlend
            );

            ikHandTarget.localRotation = Quaternion.Slerp(
                _ikHandDefaultLocalRotation,
                ikHandTarget.localRotation,
                _buttonBlend
            );
        }

        // Fall back to the default logic to extend the hand when the current target is close by
        bool isWithinRange = (currentTarget.position - transform.position).sqrMagnitude < IK_DISTANCE_SQR;

        handIKConstraint.weight = Mathf.MoveTowards(
            handIKConstraint.weight,
            isWithinRange ? 1f : 0f,
            IK_BLEND_SPEED * Time.deltaTime
        );
    }
    
    //TODO: LOOK AT THIS AGAIN LATER, PATH RESUMING DOESN'T REALLY WORK RIGHT NOW
    private bool TryCapturePendingDestination() {
        if (agent.pathStatus != NavMeshPathStatus.PathComplete || !agent.hasPath)
            return false;
        
        _pendingDestination = agent.destination;
        return true;
    }
    
    //TODO: LOOK AT THIS AGAIN LATER, PATH RESUMING DOESN'T REALLY WORK RIGHT NOW
    private IEnumerator ResumePathNextFrame() {
        yield return null;
        
        if (_hasPendingDestination) {
            WalkTo(_pendingDestination);
            _hasPendingDestination = false;
        }

        _isResumingPath = false;
    }
    
    private bool IsDoorBlockingPath(Door door) {
        // Set up an XYZ as toTarget and take the  position 
        Vector3 toTarget = currentTarget.position - transform.position;

        // 2. Raycast along that direction on the door layer only
        if (!Physics.Raycast(transform.position, toTarget.normalized, out RaycastHit hit, toTarget.magnitude, doorLayerMask))
            return false;

        // 3. Return true only if the hit collider belongs to the door in question
        return hit.collider.gameObject == door.gameObject
            || hit.collider.transform.IsChildOf(door.transform);
    }

    /// <summary>
    /// Scans for the nearest DoorButton within BUTTON_IK_DISTANCE whose linked door
    /// is currently closed. Buttons with an already-open door are skipped entirely.
    /// Returns null if no actionable buttons are in range.
    /// </summary>
    private DoorButton GetNearestActionableButton() {
        // 1. Broad-phase: collect all colliders within the button interaction radius
        int count = Physics.OverlapSphereNonAlloc(transform.position, BUTTON_IK_DISTANCE, _overlapBuffer);

        DoorButton nearest        = null;
        float      nearestDistSqr = BUTTON_IK_DISTANCE_SQR;

        // 2. Narrow-phase: filter for DoorButton components with a closed linked door
        for (int i = 0; i < count; i++) {
            if (!_overlapBuffer[i].TryGetComponent(out DoorButton button)) continue;

            // 3. Skip this button entirely if its door is already open
            if (button.linkedDoor.isOpen) continue;

            float distSqr = (button.transform.position - transform.position).sqrMagnitude;

            // 4. Track whichever valid button is closest
            if (distSqr < nearestDistSqr) {
                nearestDistSqr = distSqr;
                nearest        = button;
            }
        }

        return nearest;
    }

    #endregion

    #region Public Methods

    public void WalkTo(Vector3 position) {
        locomotionSystem.WalkToPosition(position);
    }

    public void Teleport(Vector3 position) {
        locomotionSystem.Warp(position);
    }

    #endregion
}