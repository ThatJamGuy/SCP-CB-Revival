using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using FMODUnity;

public class NPC_106 : MonoBehaviour {
    [System.Serializable]
    public struct AnimationConfig {
        public string emergeTrigger, walkTrigger, wallTraverseTrigger, catchUpEmergeTrigger, chaseEndTrigger;
        public float emergeLength, wallTraverseLength, catchUpEmergeLength;
    }

    [System.Serializable]
    public struct WallTraverseConfig {
        public LayerMask layerMask;
        public float maxThickness, checkDistance, cooldownDuration;
    }

    [System.Serializable]
    public struct CatchUpConfig {
        public float distance, navSampleRadius, cooldownDuration;
    }

    [Header("Chase Settings")]
    [SerializeField] private float chaseDuration = 63f;
    [SerializeField] private AnimationConfig animationConfig;

    [Header("Systems")]
    [SerializeField] private WallTraverseConfig wallConfig;
    [SerializeField] private CatchUpConfig catchUpConfig;

    [Header("Audio")]
    [SerializeField] private EventReference laughSounds;
    [SerializeField] private float laughMinTime = 5f, laughMaxTime = 20f;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private bool isChasing, isActive, isTraversing;
    private float wallCooldown, catchUpCooldown;
    private Coroutine chaseEndCoroutine, laughCoroutine;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (!agent) Debug.LogError($"NavMeshAgent missing on {gameObject.name}");
        if (!animator) Debug.LogError($"Animator missing on {gameObject.name}");
    }

    private void Start() {
        player = GameObject.FindWithTag("Player")?.transform;
        if (!player) Debug.LogWarning("Player not found!");

        animator.enabled = true;
        agent.enabled = false;
        isActive = true;

        animator.SetTrigger(animationConfig.emergeTrigger);
        StartCoroutine(EmergeSequence());
        chaseEndCoroutine = StartCoroutine(ChaseEndSequence());
    }

    private void Update() {
        if (!isActive) return;

        UpdateCooldowns();

        if (!isChasing || !player || isTraversing) return;

        if (ShouldCatchUp()) {
            StartCoroutine(CatchUp());
            return;
        }

        if (wallCooldown <= 0f && TryWallTraverse()) return;

        if (agent.enabled && !agent.isStopped) {
            agent.SetDestination(player.position);
        }
    }

    private void UpdateCooldowns() {
        if (wallCooldown > 0f) wallCooldown -= Time.deltaTime;
        if (catchUpCooldown > 0f) catchUpCooldown -= Time.deltaTime;
    }

    private bool ShouldCatchUp() =>
        catchUpCooldown <= 0f &&
        Vector3.Distance(transform.position, player.position) > catchUpConfig.distance;

    [ContextMenu("Start Chase")]
    public void StartChase() {
        if (isActive) return;

        isActive = true;
        animator.enabled = true;
        animator.SetTrigger(animationConfig.emergeTrigger);

        StartCoroutine(EmergeSequence());
        chaseEndCoroutine = StartCoroutine(ChaseEndSequence());
    }

    private void EndChase() {
        if (animator && !string.IsNullOrEmpty(animationConfig.chaseEndTrigger))
            animator.SetTrigger(animationConfig.chaseEndTrigger);

        isChasing = false;
        isActive = false;

        if (agent.enabled) {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (laughCoroutine != null) {
            StopCoroutine(laughCoroutine);
            laughCoroutine = null;
        }

        if (MusicManager.instance != null)
            MusicManager.instance.SetMusicState(MusicState.LCZ);

        if (GameManager.instance != null)
            GameManager.instance.scp106Active = false;
    }

    private IEnumerator EmergeSequence() {
        yield return new WaitForSeconds(animationConfig.emergeLength);

        animator.SetTrigger(animationConfig.walkTrigger);
        agent.enabled = true;

        if (!player) player = GameObject.FindWithTag("Player")?.transform;

        isChasing = true;
        laughCoroutine = StartCoroutine(LaughLoop());

        if (MusicManager.instance != null)
            MusicManager.instance.SetMusicState(MusicState.scp106);
    }

    private bool TryWallTraverse() {
        if (!player) return false;

        Vector3 direction = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < 2f) return false;

        RaycastHit[] hits = Physics.RaycastAll(
            transform.position,
            direction,
            Mathf.Min(distance, wallConfig.checkDistance),
            wallConfig.layerMask
        );

        if (hits.Length < 2) return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        float thickness = Vector3.Distance(hits[0].point, hits[1].point);
        bool differentColliders = hits[0].collider != hits[1].collider;
        float normalDot = Vector3.Dot(hits[0].normal, hits[1].normal);

        if (thickness <= wallConfig.maxThickness && (normalDot < -0.7f || differentColliders)) {
            StartCoroutine(TraverseWall(hits[0], hits[1]));
            return true;
        }

        return false;
    }

    private IEnumerator TraverseWall(RaycastHit entry, RaycastHit exit) {
        SetTraversing(true);
        animator.SetTrigger(animationConfig.wallTraverseTrigger);

        Vector3 exitPosition = exit.point + (exit.point - entry.point).normalized * 0.15f;
        TeleportAgent(exitPosition, -exit.normal);

        animator.SetTrigger(animationConfig.emergeTrigger);
        yield return new WaitForSeconds(animationConfig.emergeLength);

        SetTraversing(false);
        wallCooldown = wallConfig.cooldownDuration;
    }

    private IEnumerator CatchUp() {
        if (!player) yield break;

        catchUpCooldown = catchUpConfig.cooldownDuration;
        SetTraversing(true);

        Vector3 targetPosition = player.position - player.forward * 2f;

        if (FindValidNavMeshPosition(targetPosition, out Vector3 validPosition)) {
            TeleportAgent(validPosition, (player.position - validPosition).normalized);
            animator.SetTrigger(animationConfig.catchUpEmergeTrigger);
            yield return new WaitForSeconds(animationConfig.catchUpEmergeLength);
        }

        SetTraversing(false);
    }

    private bool FindValidNavMeshPosition(Vector3 target, out Vector3 validPosition) {
        if (NavMesh.SamplePosition(target, out NavMeshHit hit, catchUpConfig.navSampleRadius, NavMesh.AllAreas)) {
            validPosition = hit.position;
            return true;
        }

        if (player && NavMesh.SamplePosition(player.position, out hit, catchUpConfig.navSampleRadius, NavMesh.AllAreas)) {
            validPosition = hit.position;
            return true;
        }

        validPosition = Vector3.zero;
        return false;
    }

    private void SetTraversing(bool traversing) {
        isTraversing = traversing;

        if (agent.enabled) {
            agent.isStopped = traversing;
            if (traversing) {
                agent.ResetPath();
            }
            else if (player) {
                agent.SetDestination(player.position);
            }
        }
    }

    private void TeleportAgent(Vector3 position, Vector3 forward) {
        agent.enabled = false;
        transform.position = position;
        transform.forward = forward;
        agent.enabled = true;
        agent.Warp(position);
    }

    private IEnumerator LaughLoop() {
        while (isChasing && isActive) {
            yield return new WaitForSeconds(Random.Range(laughMinTime, laughMaxTime));

            if (AudioManager.instance != null && isChasing) {
                AudioManager.instance.PlaySound(laughSounds, transform.position);
            }
        }
    }

    private IEnumerator ChaseEndSequence() {
        yield return new WaitForSeconds(chaseDuration);
        yield return new WaitForSeconds(1f);
        EndChase();
        yield return new WaitForSeconds(10f);
        Destroy(gameObject);
    }

    private void OnDestroy() {
        if (laughCoroutine != null) StopCoroutine(laughCoroutine);
        if (chaseEndCoroutine != null) StopCoroutine(chaseEndCoroutine);
    }
}