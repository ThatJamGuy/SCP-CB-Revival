using UnityEngine;
using UnityEngine.AI;

public class NPC_173 : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] bool canRoam = true;
    [SerializeField, Range(0f, 1f)] float doorOpenChance = 0.3f;
    [SerializeField, Range(0f, 180f)] float viewAngle = 60f;
    [SerializeField] float chaseDistanceThreshold = 10f;
    [SerializeField] float doorCheckRadius = 5f;
    [SerializeField] float freeRoamRadius = 10f;
    [SerializeField] float closeDistanceThreshold = 2f;
    [SerializeField] LayerMask obstructionLayers;

    [Header("References")]
    [SerializeField] Renderer npcRenderer;

    [Header("Movement")]
    [SerializeField] float roamSpeed = 3.5f;
    [SerializeField] float chaseSpeed = 7f;
    [SerializeField] float acceleration = 8f;

    [Header("Timing")]
    [SerializeField] float visibilityCheckInterval = 0.1f;
    [SerializeField] float chaseMovementInterval = 0.1f;
    [SerializeField] float roamMovementInterval = 1f;

    [Header("Audio")]
    [SerializeField] AudioSource horrorSource;
    [SerializeField] AudioClip[] horrorSounds;
    [SerializeField] AudioSource movementSource;
    [SerializeField] float movementSoundThreshold = 0.1f;

    NavMeshAgent agent;
    PlayerController player;
    Camera playerCam;

    bool isVisible, hasDirectLineOfSight, hasPlayedHorrorSound, isChasing;
    float distanceFromPlayer, lastVisibilityCheckTime, lastMovementTime, originalStoppingDistance;
    Vector3 currentRoamDestination;
    const float doorCheckInterval = 10f;

    void Awake() {
        agent = GetComponent<NavMeshAgent>();
        originalStoppingDistance = agent.stoppingDistance;
    }

    void Start() {
        player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        playerCam = GameObject.FindWithTag("PlayerCam")?.GetComponent<Camera>();
        if (!player || !playerCam || (npcRenderer == null && !(npcRenderer = GetComponentInChildren<Renderer>()))) {
            Debug.LogError("Missing required components."); enabled = false; return;
        }
        currentRoamDestination = GetRandomNavMeshPosition(transform.position, freeRoamRadius);
        UpdateMovementParameters();
    }

    void Update() {
        if (player == null) return;
        if (Time.time - lastVisibilityCheckTime >= visibilityCheckInterval) {
            UpdateDistanceFromPlayer();
            CheckVisibility();
            lastVisibilityCheckTime = Time.time;
        }
        UpdateMovementSound();
        HandleMovement();
    }

    void UpdateDistanceFromPlayer() {
        distanceFromPlayer = Vector3.Distance(transform.position, player.transform.position);
        bool shouldChase = (hasDirectLineOfSight && isVisible) || distanceFromPlayer <= chaseDistanceThreshold;
        if (isChasing != shouldChase) {
            isChasing = shouldChase;
            UpdateMovementParameters();
        }
    }

    void CheckVisibility() {
        if (!npcRenderer.isVisible) { isVisible = hasDirectLineOfSight = false; return; }
        var camPos = playerCam.transform.position;
        var bounds = npcRenderer.bounds;
        float distToNPC = Vector3.Distance(camPos, bounds.center);
        bool inPlayerView = false;

        if (distToNPC <= closeDistanceThreshold) {
            Vector3[] keyPoints = {
                bounds.center,
                bounds.min + Vector3.up * bounds.size.y * 0.8f,
                bounds.center + Vector3.up * bounds.size.y * 0.3f,
                bounds.center - Vector3.up * bounds.size.y * 0.3f,
                bounds.center + playerCam.transform.right * bounds.size.x * 0.3f,
                bounds.center - playerCam.transform.right * bounds.size.x * 0.3f
            };
            foreach (var p in keyPoints) {
                var v = playerCam.WorldToViewportPoint(p);
                if (v.z > 0 && v.x >= -0.8f && v.x <= 1.8f && v.y >= -0.8f && v.y <= 1.8f) { inPlayerView = true; break; }
            }
            if (!inPlayerView) {
                Vector3[] corners = {
                    bounds.min, new(bounds.min.x, bounds.min.y, bounds.max.z),
                    new(bounds.min.x, bounds.max.y, bounds.min.z), new(bounds.min.x, bounds.max.y, bounds.max.z),
                    new(bounds.max.x, bounds.min.y, bounds.min.z), new(bounds.max.x, bounds.min.y, bounds.max.z),
                    new(bounds.max.x, bounds.max.y, bounds.min.z), bounds.max
                };
                foreach (var c in corners) {
                    var v = playerCam.WorldToViewportPoint(c);
                    if (v.z > 0 && v.x >= -0.3f && v.x <= 1.3f && v.y >= -0.3f && v.y <= 1.3f) { inPlayerView = true; break; }
                }
            }
        }
        else {
            var toNPC = (bounds.center - camPos).normalized;
            var angle = Vector3.Angle(playerCam.transform.forward, toNPC);
            var v = playerCam.WorldToViewportPoint(bounds.center);
            inPlayerView = angle < viewAngle && v.z > 0 && v.x > 0 && v.x < 1 && v.y > 0 && v.y < 1;
        }

        isVisible = inPlayerView;
        if (!isVisible) { hasDirectLineOfSight = false; return; }

        Vector3[] checkPoints = {
            bounds.center,
            bounds.center + Vector3.up * bounds.size.y * 0.25f,
            bounds.center - Vector3.up * bounds.size.y * 0.25f,
            bounds.center + Vector3.right * bounds.size.x * 0.25f,
            bounds.center - Vector3.right * bounds.size.x * 0.25f
        };
        foreach (var p in checkPoints) {
            var dir = (p - camPos).normalized;
            var dist = Vector3.Distance(camPos, p);
            if (dist <= 0.5f || !Physics.Raycast(camPos, dir, dist - 0.1f, obstructionLayers)) {
                hasDirectLineOfSight = true;
                if (!hasPlayedHorrorSound && horrorSource && horrorSounds.Length > 0) {
                    hasPlayedHorrorSound = true;
                    horrorSource.clip = horrorSounds[Random.Range(0, horrorSounds.Length)];
                    horrorSource.Play();
                }
                return;
            }
        }
        hasDirectLineOfSight = false;
    }

    void UpdateMovementSound() {
        if (movementSource) movementSource.enabled = agent.velocity.magnitude > movementSoundThreshold && !isVisible;
    }

    void UpdateMovementParameters() {
        agent.speed = isChasing ? chaseSpeed : roamSpeed;
        agent.acceleration = isChasing ? acceleration * 2f : acceleration;
        agent.stoppingDistance = isChasing ? 0.5f : originalStoppingDistance;
    }

    void HandleMovement() {
        if (isVisible && hasDirectLineOfSight) { StopMoving(); return; }
        float moveInterval = isChasing ? chaseMovementInterval : roamMovementInterval;
        if (Time.time - lastMovementTime >= moveInterval) {
            if (isChasing) agent.SetDestination(player.transform.position);
            else if (canRoam) {
                if (Vector3.Distance(transform.position, currentRoamDestination) < agent.stoppingDistance || Time.time - lastMovementTime >= roamMovementInterval * 3f)
                    currentRoamDestination = GetRandomNavMeshPosition(transform.position, freeRoamRadius);
                agent.SetDestination(currentRoamDestination);
            }
            lastMovementTime = Time.time;
        }
    }

    void StopMoving() {
        if (agent.hasPath) {
            agent.SetDestination(transform.position);
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }

    Vector3 GetRandomNavMeshPosition(Vector3 origin, float radius) {
        var randomDir = Random.insideUnitSphere * radius + origin;
        NavMesh.SamplePosition(randomDir, out var hit, radius, NavMesh.AllAreas);
        return hit.position;
    }

    void OnEnable() => InvokeRepeating(nameof(CheckForDoors), doorCheckInterval, doorCheckInterval);
    void OnDisable() => CancelInvoke(nameof(CheckForDoors));

    void CheckForDoors() {
        var colliders = Physics.OverlapSphere(transform.position, doorCheckRadius);
        Door nearest = null;
        float minDist = float.MaxValue;
        foreach (var col in colliders)
            if (col.TryGetComponent(out Door door)) {
                var dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDist) { minDist = dist; nearest = door; }
            }
        if (nearest && Random.value < doorOpenChance) nearest.OpenDoor();
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, doorCheckRadius);
        if (!isChasing && canRoam) {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, currentRoamDestination);
            Gizmos.DrawWireSphere(currentRoamDestination, 0.5f);
        }
        if (playerCam && npcRenderer) {
            Gizmos.color = isVisible ? (hasDirectLineOfSight ? Color.green : Color.yellow) : Color.red;
            Gizmos.DrawLine(playerCam.transform.position, npcRenderer.bounds.center);
        }
    }
}