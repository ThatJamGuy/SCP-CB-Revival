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

    [Header("References")]
    [SerializeField] private Renderer npcRenderer;

    [Header("Movement")]
    [SerializeField] private float roamSpeed = 3.5f;
    [SerializeField] private float chaseSpeed = 7f;
    [SerializeField] private float acceleration = 8f;

    [Header("Timing")]
    [SerializeField] private float visibilityCheckInterval = 0.1f;
    [SerializeField] private float chaseMovementInterval = 0.1f;
    [SerializeField] private float roamMovementInterval = 1f;

    [Header("Audio")]
    [SerializeField] private AudioSource horrorSource;
    [SerializeField] private AudioClip[] horrorSounds;
    [SerializeField] private AudioSource movementSource;
    [SerializeField] private float movementSoundThreshold = 0.1f;

    private NavMeshAgent agent;
    private PlayerController player;
    private Camera playerCam;

    private bool isVisible;
    private bool hasDirectLineOfSight;
    private bool hasPlayedHorrorSound = false;
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

        if (npcRenderer == null) {
            npcRenderer = GetComponentInChildren<Renderer>();
            if (npcRenderer == null) {
                Debug.LogError("NPC Renderer not found!");
                enabled = false;
                return;
            }
        }

        currentRoamDestination = GetRandomNavMeshPosition(transform.position, freeRoamRadius);
        UpdateMovementParameters();
    }

    private void Update() {
        if (player == null) return;

        if (Time.time - lastVisibilityCheckTime >= visibilityCheckInterval) {
            UpdateDistanceFromPlayer();
            CheckVisibility();
            lastVisibilityCheckTime = Time.time;
        }

        UpdateMovementSound();
        HandleMovement();
    }

    private void UpdateDistanceFromPlayer() {
        distanceFromPlayer = Vector3.Distance(transform.position, player.transform.position);

        bool shouldChase = (hasDirectLineOfSight && isVisible) || (distanceFromPlayer <= chaseDistanceThreshold);

        if (isChasing && !shouldChase && distanceFromPlayer > chaseDistanceThreshold) {
            isChasing = false;
            UpdateMovementParameters();
        }
        else if (!isChasing && shouldChase) {
            isChasing = true;
            UpdateMovementParameters();
        }
    }

    private void CheckVisibility() {
        // First check if mesh is visible to any camera (efficient pre-filter)
        if (!npcRenderer.isVisible) {
            isVisible = false;
            hasDirectLineOfSight = false;
            return;
        }

        // Check if within player's FOV using mesh bounds center
        Vector3 meshCenter = npcRenderer.bounds.center;
        Vector3 toNPC = (meshCenter - playerCam.transform.position).normalized;
        float angle = Vector3.Angle(playerCam.transform.forward, toNPC);

        Vector3 viewportPos = playerCam.WorldToViewportPoint(meshCenter);
        bool inPlayerView = viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;

        isVisible = angle < viewAngle && inPlayerView;

        if (!isVisible) {
            hasDirectLineOfSight = false;
            return;
        }

        // Check for obstructions using multiple raycast points
        Vector3 rayOrigin = playerCam.transform.position;
        Bounds bounds = npcRenderer.bounds;

        Vector3[] checkPoints = {
            bounds.center,
            bounds.center + Vector3.up * (bounds.size.y * 0.25f),
            bounds.center - Vector3.up * (bounds.size.y * 0.25f),
            bounds.center + Vector3.right * (bounds.size.x * 0.25f),
            bounds.center + Vector3.left * (bounds.size.x * 0.25f)
        };

        bool anyPointVisible = false;
        foreach (Vector3 point in checkPoints) {
            Vector3 direction = (point - rayOrigin).normalized;
            float distance = Vector3.Distance(rayOrigin, point) - 0.1f;

            if (!Physics.Raycast(rayOrigin, direction, distance, obstructionLayers)) {
                anyPointVisible = true;
                break;
            }
        }

        hasDirectLineOfSight = anyPointVisible;

        if (hasDirectLineOfSight && !hasPlayedHorrorSound) {
            hasPlayedHorrorSound = true;
            if (horrorSource != null && horrorSounds.Length > 0) {
                horrorSource.clip = horrorSounds[Random.Range(0, horrorSounds.Length)];
                horrorSource.Play();
            }
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
                if (Vector3.Distance(transform.position, currentRoamDestination) < agent.stoppingDistance || Time.time - lastMovementTime >= roamMovementInterval * 3f) {
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

        if (playerCam != null && npcRenderer != null) {
            Gizmos.color = isVisible ? (hasDirectLineOfSight ? Color.green : Color.yellow) : Color.red;
            Gizmos.DrawLine(playerCam.transform.position, npcRenderer.bounds.center);

            // Draw raycast check points
            if (isVisible) {
                Bounds bounds = npcRenderer.bounds;
                Vector3[] points = {
                    bounds.center,
                    bounds.center + Vector3.up * (bounds.size.y * 0.25f),
                    bounds.center + Vector3.up * (bounds.size.y * 1f),
                    bounds.center - Vector3.up * (bounds.size.y * 0.25f),
                    bounds.center + Vector3.right * (bounds.size.x * 0.25f),
                    bounds.center + Vector3.left * (bounds.size.x * 0.25f)
                };

                Gizmos.color = Color.blue;
                foreach (Vector3 point in points) {
                    Gizmos.DrawWireSphere(point, 0.1f);
                }
            }
        }
    }
}