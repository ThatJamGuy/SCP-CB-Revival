using FMODUnity;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class SCP_106_New : MonoBehaviour {
    private enum State : byte {
        Idle,        // Spawned, ChaseTarget() not called yet
        Emerging,       // Emerging from the floor
        Chasing,
        Attacking,
        CatchingUp,
        Traversing,  // Passing through a wall / door
        Despawning
    }

    [Header("Behavior")]
    [SerializeField] private float chaseDuration = 63f;
    [SerializeField] private float catchUpDistance = 15f;
    [SerializeField] private float catchUpBehindDistance = 2f;
    [SerializeField] private float catchUpRetryDelay = 1f;
    [SerializeField] private float attackRange = 2f;

    [Header("Wall Traverse")]
    [SerializeField] private LayerMask wallLayerMask;
    [SerializeField] private float wallCheckDistance = 5f;
    [SerializeField] private float maxWallThickness = 2.5f;
    [SerializeField] private float wallTraverseCooldown = 5f;
    [SerializeField] private float minDistanceForTraverse = 1f;
    [SerializeField] private float wallRayHeight = 0.5f;
    [SerializeField] private float exitOffset = 0.5f;
    [Tooltip("Only traverse when the NavMesh route is this many times longer than the straight line (or doesn't exist). 0 = traverse whenever a wall is in the way.")]
    [SerializeField] private float minDetourRatio = 1.5f;

    [Header("Animation Timing")]
    [SerializeField] private float floorEmerge2AnimTime = 2.5f;
    [SerializeField] private float attackAnimTime = 2f;
    [SerializeField] private float despawnAnimTime = 2.6f;
    [FormerlySerializedAs("TeslaShockAnimTime")]
    [SerializeField] private float teslaShockAnimTime = 5f;
    [FormerlySerializedAs("CatchUpAnimTime")]
    [SerializeField] private float catchUpAnimTime = 1.25f;
    [SerializeField] private float wallTraverseAnimTime = 1f;

    [Header("FMOD Audio")]
    [SerializeField] private EventReference targetHitEvent;
    [FormerlySerializedAs("horror_106")]
    [SerializeField] private EventReference horror106Event;
    [SerializeField] private EventReference randomLaughEvent;
    [SerializeField] private EventReference teslaRetreatSound;
    [FormerlySerializedAs("CatchUpEvent")]
    [SerializeField] private EventReference catchUpEvent;
    [FormerlySerializedAs("WallTraverseEvent")]
    [SerializeField] private EventReference wallTraverseEvent;

    [Header("References")]
    [SerializeField] private GameObject despawnGoodPrefab;

    private static readonly int WalkHash = Animator.StringToHash("Walk");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int Despawn1Hash = Animator.StringToHash("Despawn1");
    private static readonly int ShockHash = Animator.StringToHash("Shock");
    private static readonly int CatchUpHash = Animator.StringToHash("CatchUp");
    private static readonly int WallTraverseHash = Animator.StringToHash("WallTraverse");

    private const string LOCAL_EVENT_PARAM = "LocalEventIndex";
    private const float REPATH_DISTANCE_SQR = 0.25f;
    private const float REPATH_RETRY_INTERVAL = 0.5f;
    private const float NAV_SAMPLE_RADIUS = 15f;
    private const float LANDING_SAMPLE_RADIUS = 2f;
    private const float MIN_WALL_THICKNESS = 0.05f;
    private const float LAUGH_MIN_INTERVAL = 10f;
    private const float LAUGH_MAX_INTERVAL = 20f;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform activeTarget;

    private State state = State.Idle;
    private bool walkingAllowed = true;
    private bool currentTargetCaptured;
    private bool needsRepath;

    private float stateEndTime;
    private float chaseEndTime;
    private float nextLaughTime;
    private float nextTraverseTime;
    private float nextCatchUpTime;
    private float nextRepathTime;

    private float catchUpDistanceSqr;
    private float attackRangeSqr;
    private float sqrDistanceToTarget;

    private Vector3 lastRequestedDestination;
    private Vector3 catchUpSoundPosition;

    #region Unity Lifecycle

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        catchUpDistanceSqr = catchUpDistance * catchUpDistance;
        attackRangeSqr = attackRange * attackRange;
    }

    private void Start() {
        // Automatically set the target to the player upon spawning for now, come back later when other targets are allowed
        SetTarget(Player.Instance.transform);
        ChaseTarget();
    }

    private void Update() {
        float now = Time.time;

        switch (state) {
            case State.Idle:
            case State.Despawning:
                return;
            case State.Emerging:
                if (now >= stateEndTime) StartChasing(now);
                return;
        }

        // Timers that run for the whole chase regardless of sub-state
        if (now >= chaseEndTime) {
            Despawn();
            return;
        }

        UpdateLaugh(now);

        switch (state) {
            case State.Chasing:
                UpdateChasing(now);
                break;
            case State.Attacking:
                if (now >= stateEndTime) ResumeChasing();
                break;
            case State.CatchingUp:
                if (now >= stateEndTime) {
                    AudioManager.PlayOneShot(catchUpEvent, catchUpSoundPosition);
                    ResumeChasing();
                }
                break;
            case State.Traversing:
                if (now >= stateEndTime) {
                    nextTraverseTime = now + wallTraverseCooldown;
                    ResumeChasing();
                }
                break;
        }
    }

    private void OnDestroy() {
        EntitySystem.Instance.UnregisterEntity(EntitySystem.Instance.cached106Entity, gameObject);
    }

    #endregion

    #region State Logic

    private void StartChasing(float now) {
        chaseEndTime = now + chaseDuration;
        nextLaughTime = now;

        AudioManager.PlayOneShot(horror106Event, LOCAL_EVENT_PARAM, 0);
        RevivalSessionEngine.Instance.RequestChaseTrack(this, 10, MusicManager.MusicTrack.SCP_106);

        EntitySystem.Instance.cached106Entity.chasingPlayer = true;

        ResumeChasing();
    }

    private void ResumeChasing() {
        state = State.Chasing;
        agent.isStopped = !walkingAllowed;
        needsRepath = true; // Path was cleared by a warp / agent was halted
        animator.SetTrigger(WalkHash);
    }

    private void UpdateChasing(float now) {
        if (activeTarget == null || !walkingAllowed) return;

        Vector3 targetPosition = activeTarget.position;
        Vector3 toTarget = targetPosition - transform.position;
        sqrDistanceToTarget = toTarget.sqrMagnitude;

        UpdateDestination(now, targetPosition);

        if (TryWallTraverse(now, toTarget, sqrDistanceToTarget)) return;

        if (sqrDistanceToTarget > catchUpDistanceSqr) {
            if (now >= nextCatchUpTime) TryStartCatchUp(now);
            return;
        }

        if (sqrDistanceToTarget <= attackRangeSqr && !currentTargetCaptured) StartAttack(now);
    }

    private void UpdateDestination(float now, Vector3 targetPosition) {
        bool targetMoved = (targetPosition - lastRequestedDestination).sqrMagnitude >= REPATH_DISTANCE_SQR;
        bool pathLost = !agent.hasPath && !agent.pathPending && now >= nextRepathTime;

        if (!needsRepath && !targetMoved && !pathLost) return;

        needsRepath = false;
        lastRequestedDestination = targetPosition;
        nextRepathTime = now + REPATH_RETRY_INTERVAL;
        agent.SetDestination(targetPosition);
    }

    private void UpdateLaugh(float now) {
        if (currentTargetCaptured || now < nextLaughTime) return;

        AudioManager.PlayOneShot(randomLaughEvent);
        nextLaughTime = now + Random.Range(LAUGH_MIN_INTERVAL, LAUGH_MAX_INTERVAL);
    }

    private void StartAttack(float now) {
        state = State.Attacking;
        stateEndTime = now + attackAnimTime;
        HaltAgent();
        animator.SetTrigger(AttackHash);
    }

    private void TryStartCatchUp(float now) {
        Vector3 targetPosition = activeTarget.position;
        Vector3 desiredPosition = targetPosition - activeTarget.forward * catchUpBehindDistance;

        if (!TryFindNavMeshPosition(desiredPosition, targetPosition, out Vector3 validPosition) ||
            !TeleportAgent(validPosition, targetPosition - validPosition)) {
            nextCatchUpTime = now + catchUpRetryDelay;
            return;
        }

        catchUpSoundPosition = desiredPosition;
        state = State.CatchingUp;
        stateEndTime = now + catchUpAnimTime;
        animator.SetTrigger(CatchUpHash);
    }

    private void BeginDespawn(int animHash, float duration, bool tesla) {
        if (state == State.Despawning) return;
        state = State.Despawning;

        if (tesla) AudioManager.PlayOneShot(teslaRetreatSound, transform.position);
        AudioManager.PlayOneShot(horror106Event, LOCAL_EVENT_PARAM, 1);
        RevivalSessionEngine.Instance.ReleaseChaseTrack(this);

        activeTarget = null;

        Instantiate(despawnGoodPrefab, transform.position, Quaternion.Euler(90f, 0f, 0f));
        HaltAgent();
        animator.SetTrigger(animHash);

        Destroy(gameObject, duration);
    }

    #endregion

    #region Wall Traverse

    private bool TryWallTraverse(float now, Vector3 toTarget, float sqrDistance) {
        if (now < nextTraverseTime) return false;
        if (sqrDistance < minDistanceForTraverse * minDistanceForTraverse) return false;
        if (minDetourRatio > 0f && !IsPathObstructed(Mathf.Sqrt(sqrDistance))) return false;

        // Horizontal only, so a target on a different height doesn't tilt the ray into floors/ceilings
        toTarget.y = 0f;
        float flatDistance = toTarget.magnitude;
        if (flatDistance < 0.01f) return false;

        Vector3 direction = toTarget / flatDistance;
        Vector3 origin = transform.position;
        origin.y += wallRayHeight;

        if (!Physics.Raycast(origin, direction, out RaycastHit entry, Mathf.Min(flatDistance, wallCheckDistance),
                wallLayerMask, QueryTriggerInteraction.Ignore)) return false;

        // If the wall is thicker than maxWallThickness the back ray starts inside it and finds nothing
        float backStart = entry.distance + maxWallThickness;
        if (!Physics.Raycast(origin + direction * backStart, -direction, out RaycastHit exit, maxWallThickness,
                wallLayerMask, QueryTriggerInteraction.Ignore)) return false;

        float exitDistance = backStart - exit.distance;
        if (exitDistance - entry.distance < MIN_WALL_THICKNESS) return false;

        Vector3 landing = origin + direction * (exitDistance + exitOffset);
        if (!NavMesh.SamplePosition(landing, out NavMeshHit navHit, LANDING_SAMPLE_RADIUS, NavMesh.AllAreas)) return false;

        // Reject samples that snapped back to the near side of the wall
        if (Vector3.Dot(navHit.position - origin, direction) < exitDistance) return false;

        return StartTraverse(now, navHit.position, direction);
    }

    private bool StartTraverse(float now, Vector3 exitPosition, Vector3 forward) {
        if (!TeleportAgent(exitPosition, forward)) return false;

        state = State.Traversing;
        stateEndTime = now + wallTraverseAnimTime;

        AudioManager.PlayOneShot(wallTraverseEvent, transform.position);
        animator.SetTrigger(WallTraverseHash);
        return true;
    }

    // True when the agent has no route, or the route is a big detour compared to the straight line
    private bool IsPathObstructed(float straightDistance) {
        if (agent.pathPending) return false;
        if (!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete) return true;
        return agent.remainingDistance > straightDistance * minDetourRatio;
    }

    #endregion

    #region Private Utilities

    private bool TeleportAgent(Vector3 position, Vector3 forward) {
        agent.ResetPath();
        agent.velocity = Vector3.zero;

        if (!agent.Warp(position)) return false;

        forward.y = 0f; // Keep the model upright
        if (forward.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(forward);

        agent.isStopped = true;
        return true;
    }

    private void HaltAgent() {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    private static bool TryFindNavMeshPosition(Vector3 desired, Vector3 fallback, out Vector3 validPosition) {
        if (NavMesh.SamplePosition(desired, out NavMeshHit hit, NAV_SAMPLE_RADIUS, NavMesh.AllAreas) ||
            NavMesh.SamplePosition(fallback, out hit, NAV_SAMPLE_RADIUS, NavMesh.AllAreas)) {
            validPosition = hit.position;
            return true;
        }

        validPosition = Vector3.zero;
        return false;
    }

    #endregion

    #region Public Methods

    public void SetTarget(Transform target) { activeTarget = target; }

    public void ChaseTarget() {
        if (activeTarget == null || state != State.Idle) return;

        state = State.Emerging;
        stateEndTime = Time.time + floorEmerge2AnimTime;
    }

    public void AllowWalking(bool allowWalking) {
        walkingAllowed = allowWalking;
        if (state == State.Chasing) agent.isStopped = !allowWalking;
    }

    // Triggered via the animation event on the SCP-106 model
    public void AttackTargetCheck() {
        if (activeTarget == null || state != State.Attacking) return;
        if (sqrDistanceToTarget > attackRangeSqr) return;

        if (Player.Instance.isDead) {
            currentTargetCaptured = true;
            return;
        }

        AudioManager.PlayOneShot(targetHitEvent, transform.position);
        Player.Instance.KillPlayer(3, 0.2f, 0, "Subject D-9341. Body partially decomposed by what is assumed to be SCP-106's \"corrosion\" effect. Body disposed of via incineration.");

        currentTargetCaptured = true;
        Despawn();
    }

    public void Despawn() { BeginDespawn(Despawn1Hash, despawnAnimTime, false); }
    public void DespawnTesla() { BeginDespawn(ShockHash, teslaShockAnimTime, true); }

    #endregion
}