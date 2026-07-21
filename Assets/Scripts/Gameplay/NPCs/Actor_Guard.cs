using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Actor_Guard : MonoBehaviour {
    private enum State {
        Nothing = 0,
        Animating = 1,
        Navigating = 2
    }

    private enum WaypointMode {
        SequentialOnce = 0,
        SequentialLoop = 1,
        RandomConsuming = 2,
        RandomNoRepeat = 3
    }

    // Velocity magnitude squared below which the agent counts as "stopped".
    private const float StoppedThresholdSqr = 0.01f;

    [Header("Generic Settings")]
    [SerializeField] private string actorName;
    [SerializeField] private bool useNavigation;

    [Header("Animation Settings")]
    [SerializeField] private bool useWalkAnim;
    [SerializeField] private bool useSprintAnim;
    [SerializeField] private string initialAnimationName;
    [SerializeField] private string walkAnimBoolName = "Walking";
    [SerializeField] private string sprintAnimBoolName;

    [Header("Navigation Settings")]
    [SerializeField] private bool allowSprinting;
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private float minDistToSprint;
    [SerializeField] private Transform[] waypoints;

    [Header("Actor States")]
    [SerializeField] private State currentState = State.Nothing;

    [Header("References")]
    public Transform voiceSource;

    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent navMeshAgent;

    private bool travellingWaypoints;
    private int currentWaypointIndex;
    private WaypointMode waypointMode;
    private List<Transform> waypointPool; // working copy for RandomConsuming so the original array is never mutated
    private bool isSprinting;

    #region Unity Callbacks

    private void Start() {
        if (animator != null && !string.IsNullOrEmpty(initialAnimationName))
            animator.Play(initialAnimationName);
    }

    private void Update() {
        if (!useNavigation || navMeshAgent == null) return;

        bool isMoving = navMeshAgent.velocity.sqrMagnitude > StoppedThresholdSqr;

        if (isMoving) currentState = State.Navigating;
        else if (currentState != State.Animating) currentState = State.Nothing;

        if (useWalkAnim) animator.SetBool(walkAnimBoolName, isMoving && !isSprinting);
        if (useSprintAnim) animator.SetBool(sprintAnimBoolName, isMoving && isSprinting);

        if (travellingWaypoints && HasArrivedAtDestination()) AdvanceWaypoint();
    }

    #endregion

    #region Private Methods

    private bool HasArrivedAtDestination() {
        return !navMeshAgent.pathPending
            && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance
            && (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude < StoppedThresholdSqr);
    }

    private void AdvanceWaypoint() {
        switch (waypointMode) {
            case WaypointMode.SequentialOnce: AdvanceSequential(loop: false); break;
            case WaypointMode.SequentialLoop: AdvanceSequential(loop: true); break;
            case WaypointMode.RandomConsuming: AdvanceRandomConsuming(); break;
            case WaypointMode.RandomNoRepeat: AdvanceRandomNoRepeat(); break;
        }
    }

    private void AdvanceSequential(bool loop) {
        currentWaypointIndex++;

        if (currentWaypointIndex >= waypoints.Length) {
            if (!loop) { travellingWaypoints = false; return; }
            currentWaypointIndex = 0;
        }

        WalkToPoint(waypoints[currentWaypointIndex].position, forceWalk: false);
    }

    private void AdvanceRandomConsuming() {
        waypointPool.RemoveAt(currentWaypointIndex);

        if (waypointPool.Count == 0) { travellingWaypoints = false; return; }

        currentWaypointIndex = Random.Range(0, waypointPool.Count);
        WalkToPoint(waypointPool[currentWaypointIndex].position, forceWalk: false);
    }

    private void AdvanceRandomNoRepeat() {
        if (waypoints.Length > 1) {
            int previous = currentWaypointIndex;
            do { currentWaypointIndex = Random.Range(0, waypoints.Length); }
            while (currentWaypointIndex == previous);
        }

        WalkToPoint(waypoints[currentWaypointIndex].position, forceWalk: false);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Tells the actor to walk to a waypoint or group of waypoints.
    /// </summary>
    /// <param name="mode">0 = travel in order then stop, 1 = travel in order and loop,
    /// 2 = travel at random then stop (each waypoint visited once), 3 = travel at random and loop</param>
    public void WalkToWaypoint(int mode) {
        if (!useNavigation || waypoints == null || waypoints.Length == 0) return;

        waypointMode = (WaypointMode)Mathf.Clamp(mode, 0, 3);
        travellingWaypoints = true;

        if (waypointMode == WaypointMode.RandomConsuming) {
            waypointPool = new List<Transform>(waypoints);
            currentWaypointIndex = Random.Range(0, waypointPool.Count);
            WalkToPoint(waypointPool[currentWaypointIndex].position, forceWalk: false);
            return;
        }

        currentWaypointIndex = waypointMode == WaypointMode.RandomNoRepeat
            ? Random.Range(0, waypoints.Length)
            : 0;

        WalkToPoint(waypoints[currentWaypointIndex].position, forceWalk: false);
    }

    public void WalkToPoint(Vector3 position, bool forceWalk = true) {
        float distanceToPoint = Vector3.Distance(position, transform.position);

        isSprinting = allowSprinting && !forceWalk && distanceToPoint > minDistToSprint;
        navMeshAgent.speed = isSprinting ? runningSpeed : walkingSpeed;
        navMeshAgent.SetDestination(position);
    }

    /// <summary>Call when starting a scripted animation that should suppress navigation-state changes.</summary>
    public void EnterAnimatingState() => currentState = State.Animating;

    /// <summary>Call when a scripted animation ends, returning control to normal navigation-state tracking.</summary>
    public void ExitAnimatingState() => currentState = State.Nothing;

    #endregion

    [ContextMenu("Walk to WP 0")]
    public void DebugWalkPoints() {
        WalkToWaypoint(0);
    }
}