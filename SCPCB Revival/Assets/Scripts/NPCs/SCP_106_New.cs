using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using FMODUnity;

public class SCP_106_New : MonoBehaviour {

    [Header("Behavior")]
    [SerializeField] private bool targetPlayerOnStart = true;
    [SerializeField] private float chaseDuration = 63f;

    [Header("Animation Timing")]
    [SerializeField] private float floorEmerge2AnimTime = 2.5f;
    [SerializeField] private float attackAnimTime = 2f;
    [SerializeField] private float despawnAnimTime = 2.6f;

    [Header("FMOD Audio")]
    [SerializeField] private EventReference targetHitEvent;
    [SerializeField] private EventReference chaseStartEvent;
    [SerializeField] private EventReference chaseEndEvent;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform activeTarget;

    private bool canWalk = false;
    private bool isAttacking = false;
    private bool currentTargetCaptured = false;
    private float distanceToTarget;

    #region Unity Callbacks
    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start() {
        // If that one little checkbox is ticked about, then SCP-106 will immediately target the player and start chasing them. This is true in most cases.
        if (targetPlayerOnStart) {
            SetTarget(PlayerAccessor.instance.transform);
            ChaseTarget();
        }
    }

    private void Update() {
        if (activeTarget == null || !canWalk) return;
        agent.SetDestination(activeTarget.position);
        distanceToTarget = Vector3.Distance(transform.position, activeTarget.position);

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
            if (activeTarget.GetComponent<PlayerAccessor>() != null) {
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
        if (activeTarget.GetComponent<PlayerAccessor>()) {
            AudioManager.instance.PlaySound(chaseEndEvent, transform.position);
            MusicManager.instance.SetMusicState(MusicState.LCZ);
        }

        if (activeTarget != null) activeTarget = null;
        StartCoroutine(DespawnCoroutine());
    }
    #endregion

    #region Coroutines
    // Start all the necessary stuff for chasing a target, mostly animation and setting some values.
    private IEnumerator BeginChaseRoutine() {
        yield return new WaitForSeconds(floorEmerge2AnimTime);
        animator.SetTrigger("Walk");
        CanWalk();

        if (activeTarget.GetComponent<PlayerAccessor>()) {
            AudioManager.instance.PlaySound(chaseStartEvent, transform.position);
            MusicManager.instance.SetMusicState(MusicState.scp106);
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
        CantWalk();
        animator.SetTrigger("Despawn1");
        yield return new WaitForSeconds(despawnAnimTime);
        GameManager.instance.scp106Active = false;
        Destroy(gameObject);
    }
    #endregion
}