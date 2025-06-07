using UnityEngine;
using UnityEngine.AI;

public class NPC_173 : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private bool canRoam = true;
    [SerializeField][Range(0f, 1f)] private float doorOpenChance = 0.3f;
    [SerializeField][Range(0f, 180f)] private float viewAngle = 60f;
    [SerializeField] private float chaseDistanceThreshold = 10f;
    [SerializeField] private float doorCheckRadius = 5f;
    [SerializeField] private float freeRoamRadius = 10f;
    [SerializeField] private LayerMask obstructionLayers;

    [Header("Movement")]
    [SerializeField] private float roamSpeed = 3.5f;
    [SerializeField] private float chaseSpeed = 7f;
    [SerializeField] private float acceleration = 8f;

    [Header("Timing")]
    [SerializeField] private float visibilityCheckInterval = 0.1f;
    [SerializeField] private float chaseMovementInterval = 0.1f;
    [SerializeField] private float roamMovementInterval = 1f;

    [Header("Audio")]
    [SerializeField] private AudioSource movementSource;
    [SerializeField] private float movementSoundThreshold = 0.1f;

    private NavMeshAgent agent;
    private PlayerController player;
    private Camera playerCam;

    private bool isVisible;
    private bool hasDirectLineOfSight;
    private float distanceFromPlayer;
    private bool isChasing = false;
    private float lastVisibilityCheckTime;
    private float lastMovementTime;
    private Vector3 currentRoamDestination;
    private const float doorCheckInterval = 10f;
    private float originalStoppingDistance;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        originalStoppingDistance = agent.stoppingDistance;
    }

    private void Start() {
        player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        playerCam = GameObject.FindWithTag("PlayerCam")?.GetComponent<Camera>();

        if (player == null || playerCam == null) {
            Debug.LogError("Player or PlayerCam not found!");
            enabled = false;
            return;
        }

        currentRoamDestination = GetRandomNavMeshPosition(transform.position, freeRoamRadius);
        UpdateMovementParameters();
    }

    private void Update() {
        if (player == null) return;

        // Handle visibility checks with high frequency
        if (Time.time - lastVisibilityCheckTime >= visibilityCheckInterval) {
            UpdateDistanceFromPlayer();
            UpdateVisibility();
            CheckLineOfSight();
            lastVisibilityCheckTime = Time.time;
        }

        UpdateMovementSound();
        HandleMovement();
    }

    private void UpdateDistanceFromPlayer() {
        distanceFromPlayer = Vector3.Distance(transform.position, player.transform.position);

        // Start chasing if player sees 173 (direct line of sight) or enters radius
        bool shouldChase = (hasDirectLineOfSight && isVisible) ||
                         (distanceFromPlayer <= chaseDistanceThreshold);

        if (isChasing && !shouldChase && distanceFromPlayer > chaseDistanceThreshold) {
            isChasing = false;
            Debug.Log("NPC-173 stopped chasing the player, now free roaming.");
            UpdateMovementParameters();
        }
        else if (!isChasing && shouldChase) {
            isChasing = true;
            Debug.Log("NPC-173 started chasing the player!");
            UpdateMovementParameters();
        }
    }

    private void UpdateVisibility() {
        Vector3 toNPC = (transform.position - playerCam.transform.position).normalized;
        float angle = Vector3.Angle(playerCam.transform.forward, toNPC);

        Vector3 viewportPos = playerCam.WorldToViewportPoint(transform.position);
        bool inView = viewportPos.z > 0 &&
                     viewportPos.x > 0 && viewportPos.x < 1 &&
                     viewportPos.y > 0 && viewportPos.y < 1;

        isVisible = angle < viewAngle && inView;
    }

    private void CheckLineOfSight() {
        if (!isVisible) {
            hasDirectLineOfSight = false;
            return;
        }

        RaycastHit hit;
        Vector3 rayOrigin = playerCam.transform.position;
        Vector3 direction = (transform.position - rayOrigin).normalized;
        float distance = Vector3.Distance(rayOrigin, transform.position);

        if (Physics.Raycast(rayOrigin, direction, out hit, distance, obstructionLayers)) {
            // Check if the hit object is the NPC itself or something else
            hasDirectLineOfSight = hit.collider.transform == transform;
        }
        else {
            hasDirectLineOfSight = true;
        }
    }

    private void UpdateMovementSound() {
        if (movementSource != null) {
            bool shouldPlay = agent.velocity.magnitude > movementSoundThreshold && !isVisible;
            if (movementSource.enabled != shouldPlay) {
                movementSource.enabled = shouldPlay;
            }
        }
    }

    private void UpdateMovementParameters() {
        if (isChasing) {
            agent.speed = chaseSpeed;
            agent.acceleration = acceleration * 2f;
            agent.stoppingDistance = 0.5f;
        }
        else {
            agent.speed = roamSpeed;
            agent.acceleration = acceleration;
            agent.stoppingDistance = originalStoppingDistance;
        }
    }

    private void HandleMovement() {
        // Freeze movement when visible to player, regardless of chase state
        if (isVisible && hasDirectLineOfSight) {
            StopMoving();
            return;
        }

        float movementInterval = isChasing ? chaseMovementInterval : roamMovementInterval;

        if (Time.time - lastMovementTime >= movementInterval) {
            if (isChasing) {
                agent.SetDestination(player.transform.position);
            }
            else if (canRoam) {
                // Only get new destination when close to current one or after interval
                if (Vector3.Distance(transform.position, currentRoamDestination) < agent.stoppingDistance ||
                    Time.time - lastMovementTime >= roamMovementInterval * 3f) {
                    currentRoamDestination = GetRandomNavMeshPosition(transform.position, freeRoamRadius);
                }
                agent.SetDestination(currentRoamDestination);
            }
            lastMovementTime = Time.time;
        }
    }

    private void StopMoving() {
        if (agent.hasPath) {
            agent.SetDestination(transform.position);
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }

    private Vector3 GetRandomNavMeshPosition(Vector3 origin, float radius) {
        Vector3 randomDirection = Random.insideUnitSphere * radius + origin;
        NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, radius, NavMesh.AllAreas);
        return navHit.position;
    }

    private void OnEnable() {
        InvokeRepeating(nameof(CheckForDoors), doorCheckInterval, doorCheckInterval);
    }

    private void OnDisable() {
        CancelInvoke(nameof(CheckForDoors));
    }

    private void CheckForDoors() {
        Collider[] colliders = Physics.OverlapSphere(transform.position, doorCheckRadius);
        Door closestDoor = null;
        float minDist = Mathf.Infinity;

        foreach (var col in colliders) {
            if (col.TryGetComponent(out Door door)) {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDist) {
                    minDist = dist;
                    closestDoor = door;
                }
            }
        }

        if (closestDoor != null && Random.value < doorOpenChance) {
            closestDoor.OpenDoor();
            Debug.Log($"NPC-173 opened a door: {closestDoor.name}");
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, doorCheckRadius);

        if (!isChasing && canRoam) {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, currentRoamDestination);
            Gizmos.DrawWireSphere(currentRoamDestination, 0.5f);
        }

        // Draw line of sight debug
        if (playerCam != null) {
            Gizmos.color = isVisible ?
                (hasDirectLineOfSight ? Color.green : Color.yellow) :
                Color.red;
            Gizmos.DrawLine(playerCam.transform.position, transform.position);
        }
    }
}