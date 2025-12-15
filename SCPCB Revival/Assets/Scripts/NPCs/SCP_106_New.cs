using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SCP_106_New : MonoBehaviour {

    [Header("Behavior")]
    [SerializeField] private bool targetPlayerOnStart = true;
    [SerializeField] private float chaseDuration = 63f;
    [SerializeField] private float catchUpDistance = 15f;

    [Header("Wall Traverse")]
    [SerializeField] private LayerMask wallLayerMask;
    [SerializeField] private float wallCheckDistance = 5f;
    [SerializeField] private float maxWallThickness = 2.5f;
    [SerializeField] private float wallTraverseCooldown = 5f;
    [SerializeField] private float minDistanceForTraverse = 1f;

    [Header("Animation Timing")]
    [SerializeField] private float floorEmerge2AnimTime = 2.5f;
    [SerializeField] private float attackAnimTime = 2f;
    [SerializeField] private float despawnAnimTime = 2.6f;
    [SerializeField] private float TeslaShockAnimTime = 5f;
    [SerializeField] private float CatchUpAnimTime = 1.25f;
    [SerializeField] private float wallTraverseAnimTime = 1f;

    [Header("FMOD Audio")]
    [SerializeField] private EventReference targetHitEvent;
    [SerializeField] private EventReference chaseStartEvent;
    [SerializeField] private EventReference chaseEndEvent;
    [SerializeField] private EventReference randomLaughEvent;
    [SerializeField] private EventReference teslaRetreatSound;
    [SerializeField] private EventReference CatchUpEvent;
    [SerializeField] private EventReference WallTraverseEvent;

    [Header("References")]
    [SerializeField] private GameObject despawnGoodPrefab;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform activeTarget;

    private bool canWalk = false;
    private bool isAttacking = false;
    private bool currentTargetCaptured = false;
    private bool currentTargetIsPlayer = false;
    private bool isDespawning = false;
    //private bool isTraversing = false;
    private float distanceToTarget;
    private float wallTraverseTimer = 0f;

    #region Unity Callbacks
    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start() {
        // If that one little checkbox is ticked about, then SCP-106 will immediately target the player and start chasing them. This is true in most cases.
        if (targetPlayerOnStart) {
            currentTargetIsPlayer = true;
            SetTarget(PlayerAccessor.instance.transform);
            ChaseTarget();
        }
    }

    private void Update() {
        if (activeTarget == null || !canWalk) return;
        agent.SetDestination(activeTarget.position);
        distanceToTarget = Vector3.Distance(transform.position, activeTarget.position);

        if (wallTraverseTimer > 0f) wallTraverseTimer -= Time.deltaTime;

        if (wallTraverseTimer <= 0f && TryWallTraverse()) return;

        if (distanceToTarget > catchUpDistance) {
            StartCoroutine(CatchUpRoutine());
            return;
        }

        if (distanceToTarget <= 2 && !isAttacking && !currentTargetCaptured) {
            StartCoroutine(AttackCoroutine());
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Give SCP-106 something to target. Player, other npcs, etc.
    /// </summary>
    /// <param name="target"></param>
    public void SetTarget(Transform target) { 
        activeTarget = target;
    }

    /// <summary>
    /// Actually start chasing the set target
    /// </summary>
    public void ChaseTarget() {
        if (activeTarget == null) return;
        StartCoroutine(BeginChaseRoutine());

        GameManager.instance.scp106Active = true;
    }

    /// <summary>
    /// Call this guy to stop allowing SCP-106 to move. Useful for animations where agent movement shouldnt happen.
    /// </summary>
    public void CantWalk() { 
        canWalk = false;
        agent.isStopped = true;
    }

    /// <summary>
    /// Similar to CanWalk() but for allowing SCP-106 to move again.
    /// </summary>
    public void CanWalk() { 
        canWalk = true;
        agent.isStopped = false;
    }

    /// <summary>
    /// Triggered via animation event, checks if the target is STILL in range by the time the attack hits and then if so do that stuff.
    /// </summary>
    public void AttackTargetCheck() {
        if (activeTarget == null) return;
        if (distanceToTarget <= 2 && isAttacking) {
            if (currentTargetIsPlayer) {
                if (PlayerAccessor.instance.isDead) {
                    currentTargetCaptured = true;
                    return;
                }

                AudioManager.instance.PlaySound(targetHitEvent, transform.position);
                GameManager.instance.ShowDeathScreen("Subject D-9341. Body partially decomposed by what is assumed to be SCP-106's \"corrosion\" effect. Body disposed of via incineration.");
                PlayerAccessor.instance.isDead = true;
            }

            currentTargetCaptured = true;
            Despawn();
        }
    }

    /// <summary>
    /// Depsawns SCP-106 the normal way, via simply getting bored and walking into the floor.
    /// </summary>
    public void Despawn() {
        if (isDespawning) return;

        if (currentTargetIsPlayer) {
            AudioManager.instance.PlaySound(chaseEndEvent, transform.position);
            MusicManager.instance.SetMusicState(MusicState.LCZ);
        }

        if (activeTarget != null) activeTarget = null;
        StartCoroutine(DespawnCoroutine());
    }

    public void DespawnTesla() {
        if (isDespawning) return;

        AudioManager.instance.PlaySound(teslaRetreatSound, transform.position);
        if (currentTargetIsPlayer) {
            AudioManager.instance.PlaySound(chaseEndEvent, transform.position);
            MusicManager.instance.SetMusicState(MusicState.LCZ);
        }

        if (activeTarget != null) activeTarget = null;
        StartCoroutine(DespawnTeslaCoroutine());
    }
    #endregion

    #region Private Methods
    private void TeleportAgent(Vector3 position, Vector3 forward) {
        //agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.enabled = false;

        transform.position = position;
        transform.rotation = Quaternion.LookRotation(forward);

        agent.enabled = true;
        agent.Warp(position);
        agent.velocity = Vector3.zero;

        if (activeTarget != null) {
            agent.SetDestination(activeTarget.position);
        }
    }
    #endregion

    #region Coroutines
    // Start all the necessary stuff for chasing a target, mostly animation and setting some values.
    private IEnumerator BeginChaseRoutine() {
        yield return new WaitForSeconds(floorEmerge2AnimTime);
        animator.SetTrigger("Walk");
        CanWalk();

        if (currentTargetIsPlayer) {
            AudioManager.instance.PlaySound(chaseStartEvent, transform.position);
            MusicManager.instance.SetMusicState(MusicState.scp106);

            StartCoroutine(LaughCoroutine());
        }

        StartCoroutine(WaitForChaseDurationCoroutine());
    }

    private IEnumerator WaitForChaseDurationCoroutine() {
        yield return new WaitForSeconds(chaseDuration);
        Despawn();
    }

    // Start the animation to attack the target
    private IEnumerator AttackCoroutine() { 
        CantWalk();
        isAttacking = true;
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(attackAnimTime);
        isAttacking = false;
        CanWalk();
        animator.SetTrigger("Walk");
    }

    // Despawn SCP-106 the regular way
    private IEnumerator DespawnCoroutine() {
        Instantiate(despawnGoodPrefab, transform.position, Quaternion.Euler(90, 0, 0));
        isDespawning = true;
        CantWalk();
        animator.SetTrigger("Despawn1");
        yield return new WaitForSeconds(despawnAnimTime);
        GameManager.instance.scp106Active = false;
        Destroy(gameObject);
    }

    // Make this man do some hefty laughing from time to time HAHAHAHAHAHAHA
    private IEnumerator LaughCoroutine() {
        while (currentTargetIsPlayer && !currentTargetCaptured) {
            AudioManager.instance.PlaySound(randomLaughEvent, transform.position);
            yield return new WaitForSeconds(Random.Range(10, 20));
        }
    }

    private IEnumerator DespawnTeslaCoroutine() {
        Instantiate(despawnGoodPrefab, transform.position, Quaternion.Euler(90, 0, 0));
        isDespawning = true;
        CantWalk();
        animator.SetTrigger("Shock");
        yield return new WaitForSeconds(TeslaShockAnimTime);
        GameManager.instance.scp106Active = false;
        Destroy(gameObject);
    }

    private IEnumerator CatchUpRoutine() {
        CantWalk();
        Vector3 targetPosition = activeTarget.position - activeTarget.forward * 2f;

        if (FindValidNavMeshPosition(targetPosition, out Vector3 validPosition)) {
            TeleportAgent(validPosition, (activeTarget.position - validPosition).normalized);
            animator.SetTrigger("CatchUp");
            yield return new WaitForSeconds(CatchUpAnimTime);
            AudioManager.instance.PlaySound(CatchUpEvent, targetPosition);       
        }

        CanWalk();
    }
    #endregion

    #region Complex Variables
    private bool FindValidNavMeshPosition(Vector3 target, out Vector3 validPosition) {
        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 15, NavMesh.AllAreas)) {
            validPosition = hit.position;
            return true;
        }

        if (activeTarget && NavMesh.SamplePosition(activeTarget.position, out hit, 15, NavMesh.AllAreas)) {
            validPosition = hit.position;
            return true;
        }

        validPosition = Vector3.zero;
        return false;
    }
    #endregion

    #region Wall Traverse
    private bool TryWallTraverse() {
        if (activeTarget == null || distanceToTarget < minDistanceForTraverse) return false;

        Vector3 directionToTarget = (activeTarget.position - transform.position).normalized;
        Vector3 checkOrigin = transform.position + Vector3.up * 0.5f;

        RaycastHit[] hits = Physics.RaycastAll(checkOrigin, directionToTarget,
            Mathf.Min(distanceToTarget, wallCheckDistance), wallLayerMask);

        if (hits.Length < 2) return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        RaycastHit entryHit = hits[0];
        RaycastHit exitHit = hits[1];

        float wallThickness = Vector3.Distance(entryHit.point, exitHit.point);
        if (wallThickness > maxWallThickness) return false;

        bool oppositeSides = Vector3.Dot(entryHit.normal, exitHit.normal) < -0.5f;
        bool differentColliders = entryHit.collider != exitHit.collider;

        if (oppositeSides || differentColliders) {
            StartCoroutine(TraverseWallRoutine(entryHit, exitHit));
            return true;
        }

        return false;
    }

    private IEnumerator TraverseWallRoutine(RaycastHit entry, RaycastHit exit) {
        //isTraversing = true;
        CantWalk();

        Vector3 exitPosition = exit.point + (exit.point - entry.point).normalized * 0.5f;

        if (NavMesh.SamplePosition(exitPosition, out NavMeshHit navHit, 2f, NavMesh.AllAreas)) {
            exitPosition = navHit.position;
        }

        TeleportAgent(exitPosition, -exit.normal);
        AudioManager.instance.PlaySound(WallTraverseEvent, transform.position);

        animator.SetTrigger("WallTraverse");
        yield return new WaitForSeconds(wallTraverseAnimTime);

        animator.SetTrigger("Walk");
        wallTraverseTimer = wallTraverseCooldown;
        //isTraversing = false;
        CanWalk();
    }
    #endregion
}