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

    [Header("Audio")]
    [SerializeField] private AudioSource movementSource;
    [SerializeField] private float movementSoundThreshold = 0.1f;

    private NavMeshAgent agent;
    private PlayerController player;
    private Camera playerCam;

    private bool isVisible;
    private float distanceFromPlayer;
    private bool isChasing = true;
    private float lastMovementTime;
    private const float movementUpdateInterval = 0.1f;
    private const float doorCheckInterval = 10f;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start() {
        player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        playerCam = GameObject.FindWithTag("PlayerCam")?.GetComponent<Camera>();

        if (player == null || playerCam == null) {
            Debug.LogError("Player or PlayerCam not found!");
            enabled = false;
            return;
        }
    }

    private void Update() {
        if (player == null) return;

        UpdateDistanceFromPlayer();
        UpdateVisibility();
        UpdateMovementSound();

        // Handle movement updates in Update instead of coroutine for more responsive behavior
        if (Time.time - lastMovementTime >= movementUpdateInterval) {
            HandleMovement();
            lastMovementTime = Time.time;
        }
    }

    private void UpdateDistanceFromPlayer() {
        distanceFromPlayer = Vector3.Distance(transform.position, player.transform.position);

        // Toggle between chase and roam based on distance
        if (isChasing && distanceFromPlayer > chaseDistanceThreshold) {
            isChasing = false;
            Debug.Log("NPC-173 stopped chasing the player, now free roaming.");
        }
        else if (!isChasing && distanceFromPlayer <= chaseDistanceThreshold) {
            isChasing = true;
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

    private void UpdateMovementSound() {
        if (movementSource != null) {
            bool shouldPlay = agent.velocity.magnitude > movementSoundThreshold;
            if (movementSource.enabled != shouldPlay) {
                movementSource.enabled = shouldPlay;
            }
        }
    }

    private void HandleMovement() {
        if (isVisible) {
            StopMoving();
            return;
        }

        if (isChasing) {
            agent.SetDestination(player.transform.position);
        }
        else if (canRoam) {
            agent.SetDestination(GetRandomNavMeshPosition(transform.position, freeRoamRadius));
        }
    }

    private void StopMoving() {
        if (agent.hasPath) {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }

    private Vector3 GetRandomNavMeshPosition(Vector3 origin, float radius) {
        Vector3 randomDirection = Random.insideUnitSphere * radius + origin;
        NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, radius, NavMesh.AllAreas);
        return navHit.position;
    }

    // Using InvokeRepeating instead of coroutine for door checks
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

    // Visualize door check radius in editor
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, doorCheckRadius);
    }
}