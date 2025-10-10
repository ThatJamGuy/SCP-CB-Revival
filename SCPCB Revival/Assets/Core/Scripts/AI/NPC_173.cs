using UnityEngine;
using UnityEngine.AI;

namespace scpcbr {
    /*public class NPC_173 : MonoBehaviour {
        [Header("Behavior")]
        [SerializeField] bool canRoam = true;
        [SerializeField, Range(0f, 1f)] float doorOpenChance = 0.3f;
        [SerializeField, Range(0f, 180f)] float viewAngle = 60f;

        [Header("Movement")]
        [SerializeField] float roamSpeed = 3.5f;
        [SerializeField] float chaseSpeed = 7f;
        [SerializeField] float acceleration = 8f;

        [Header("Detection")]
        [SerializeField] float chaseStartDistance = 10f;
        [SerializeField] float chaseStopDistance = 20f;
        [SerializeField] float closeDistanceThreshold = 2f;
        [SerializeField] float freeRoamRadius = 10f;
        [SerializeField] float doorCheckRadius = 5f;
        [SerializeField] LayerMask obstructionLayers;
        [SerializeField] float doorwayStopDistance = 3f;
        [SerializeField] LayerMask doorLayers = -1;

        [Header("Audio")]
        [SerializeField] AudioSource horrorSource, movementSource;
        [SerializeField] AudioClip[] closeHorrorSounds, distantHorrorSounds;
        [SerializeField] float horrorSoundDistanceThreshold = 5f;
        [SerializeField] float movementSoundThreshold = 0.1f;

        [Header("References")]
        [SerializeField] Renderer npcRenderer;

        NavMeshAgent agent;
        PlayerController player;
        Camera playerCam;

        bool isVisible, hasDirectLineOfSight, hasPlayedHorrorSound, isChasing;
        float distanceFromPlayer, lastSeenTime = -Mathf.Infinity, originalStoppingDistance;
        float lastVisibilityCheck, lastMovement;
        Vector3 currentRoamDestination;

        const float VISIBILITY_INTERVAL = 0.1f;
        const float CHASE_MOVE_INTERVAL = 0.1f;
        const float ROAM_MOVE_INTERVAL = 1f;
        const float DOOR_CHECK_INTERVAL = 10f;
        const float HORROR_COOLDOWN = 5f;

        void Awake() {
            agent = GetComponent<NavMeshAgent>();
            originalStoppingDistance = agent.stoppingDistance;
        }

        void Start() {
            if (!InitializeReferences()) return;
            currentRoamDestination = GetRandomNavMeshPosition(transform.position, freeRoamRadius);
            UpdateMovementParams();
            InvokeRepeating(nameof(CheckForDoors), DOOR_CHECK_INTERVAL, DOOR_CHECK_INTERVAL);
        }

        bool InitializeReferences() {
            player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
            playerCam = GameObject.FindWithTag("PlayerCam")?.GetComponent<Camera>();
            npcRenderer ??= GetComponentInChildren<Renderer>();

            if (!player || !playerCam || !npcRenderer) {
                Debug.LogError($"{name}: Missing required components");
                enabled = false;
                return false;
            }
            return true;
        }

        void Update() {
            if (!player) return;

            if (Time.time - lastVisibilityCheck >= VISIBILITY_INTERVAL) {
                UpdateDistanceAndChaseState();
                CheckVisibility();
                lastVisibilityCheck = Time.time;
            }

            UpdateAudio();
            HandleMovement();
        }

        void UpdateDistanceAndChaseState() {
            distanceFromPlayer = Vector3.Distance(transform.position, player.transform.position);
            bool shouldChase = (hasDirectLineOfSight && isVisible) ||
                (isChasing ? distanceFromPlayer <= chaseStopDistance : distanceFromPlayer <= chaseStartDistance);

            if (isChasing != shouldChase) {
                isChasing = shouldChase;
                UpdateMovementParams();
            }
        }

        void CheckVisibility() {
            if (!npcRenderer.isVisible) {
                isVisible = hasDirectLineOfSight = false;
                return;
            }

            Vector3 camPos = playerCam.transform.position;
            Bounds bounds = npcRenderer.bounds;
            float distToNPC = Vector3.Distance(camPos, bounds.center);

            isVisible = distToNPC <= closeDistanceThreshold ?
                CheckCloseVisibility(camPos, bounds) :
                CheckDistantVisibility(camPos, bounds);

            if (!isVisible) {
                hasDirectLineOfSight = false;
                return;
            }

            lastSeenTime = Time.time;
            hasDirectLineOfSight = CheckLineOfSight(camPos, bounds);

            if (hasDirectLineOfSight) TryPlayHorrorSound();
        }

        bool CheckCloseVisibility(Vector3 camPos, Bounds bounds) {
            Vector3[] points = {
            bounds.center, bounds.min + Vector3.up * bounds.size.y * 0.8f,
            bounds.center + Vector3.up * bounds.size.y * 0.3f,
            bounds.center - Vector3.up * bounds.size.y * 0.3f,
            bounds.center + playerCam.transform.right * bounds.size.x * 0.3f,
            bounds.center - playerCam.transform.right * bounds.size.x * 0.3f
        };

            foreach (var p in points) {
                var v = playerCam.WorldToViewportPoint(p);
                if (v.z > 0 && v.x >= -0.8f && v.x <= 1.8f && v.y >= -0.8f && v.y <= 1.8f)
                    return true;
            }

            Vector3[] corners = {
            bounds.min, new(bounds.min.x, bounds.min.y, bounds.max.z),
            new(bounds.min.x, bounds.max.y, bounds.min.z), new(bounds.min.x, bounds.max.y, bounds.max.z),
            new(bounds.max.x, bounds.min.y, bounds.min.z), new(bounds.max.x, bounds.min.y, bounds.max.z),
            new(bounds.max.x, bounds.max.y, bounds.min.z), bounds.max
        };

            foreach (var c in corners) {
                var v = playerCam.WorldToViewportPoint(c);
                if (v.z > 0 && v.x >= -0.3f && v.x <= 1.3f && v.y >= -0.3f && v.y <= 1.3f)
                    return true;
            }
            return false;
        }

        bool CheckDistantVisibility(Vector3 camPos, Bounds bounds) {
            var toNPC = (bounds.center - camPos).normalized;
            var angle = Vector3.Angle(playerCam.transform.forward, toNPC);
            var v = playerCam.WorldToViewportPoint(bounds.center);
            return angle < viewAngle && v.z > 0 && v.x > 0 && v.x < 1 && v.y > 0 && v.y < 1;
        }

        bool CheckLineOfSight(Vector3 camPos, Bounds bounds) {
            Vector3[] checkPoints = {
            bounds.center, bounds.center + Vector3.up * bounds.size.y * 0.25f,
            bounds.center - Vector3.up * bounds.size.y * 0.25f,
            bounds.center + Vector3.right * bounds.size.x * 0.25f,
            bounds.center - Vector3.right * bounds.size.x * 0.25f
        };

            foreach (var p in checkPoints) {
                var dir = (p - camPos).normalized;
                var dist = Vector3.Distance(camPos, p);
                if (dist <= 0.5f || !Physics.Raycast(camPos, dir, dist - 0.1f, obstructionLayers))
                    return true;
            }
            return false;
        }

        bool IsNearDoorway() {
            Vector3 toPlayer = (player.transform.position - transform.position).normalized;
            return Physics.Raycast(transform.position, toPlayer, doorwayStopDistance, doorLayers) ||
                   Physics.Raycast(transform.position + Vector3.up * 0.5f, toPlayer, doorwayStopDistance, doorLayers);
        }

        bool ShouldStopMovement() {
            if (!isVisible || !hasDirectLineOfSight) return false;
            if (player.isBlinking) return false;

            // Enhanced doorway detection - stop if near doorway and player visible
            if (isChasing && IsNearDoorway()) {
                float playerDistance = Vector3.Distance(transform.position, player.transform.position);
                // More conservative stopping when near doorways
                return playerDistance <= doorwayStopDistance * 1.5f;
            }

            return true;
        }

        void TryPlayHorrorSound() {
            if (hasPlayedHorrorSound || !horrorSource) return;

            var clips = distanceFromPlayer <= horrorSoundDistanceThreshold ? closeHorrorSounds : distantHorrorSounds;
            if (clips?.Length > 0) {
                hasPlayedHorrorSound = true;
                horrorSource.clip = clips[Random.Range(0, clips.Length)];
                horrorSource.Play();
            }
        }

        void UpdateAudio() {
            if (!isVisible && hasPlayedHorrorSound && (Time.time - lastSeenTime) >= HORROR_COOLDOWN)
                hasPlayedHorrorSound = false;

            if (movementSource) {
                bool playerBlinking = player.isBlinking;
                movementSource.enabled = agent.velocity.magnitude > movementSoundThreshold && (!isVisible || playerBlinking);
            }
        }

        void UpdateMovementParams() {
            agent.speed = isChasing ? chaseSpeed : roamSpeed;
            agent.acceleration = isChasing ? acceleration * 2f : acceleration;
            agent.stoppingDistance = isChasing ? 0.5f : originalStoppingDistance;
        }

        void HandleMovement() {
            if (ShouldStopMovement()) {
                StopMoving();
                return;
            }

            float moveInterval = isChasing ? CHASE_MOVE_INTERVAL : ROAM_MOVE_INTERVAL;
            if (Time.time - lastMovement < moveInterval) return;

            if (isChasing) {
                agent.SetDestination(player.transform.position);
            }
            else if (canRoam) {
                if (Vector3.Distance(transform.position, currentRoamDestination) < agent.stoppingDistance ||
                    Time.time - lastMovement >= ROAM_MOVE_INTERVAL * 3f) {
                    currentRoamDestination = GetRandomNavMeshPosition(transform.position, freeRoamRadius);
                }
                agent.SetDestination(currentRoamDestination);
            }

            lastMovement = Time.time;
        }

        void StopMoving() {
            if (!agent.hasPath) return;
            agent.SetDestination(transform.position);
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        Vector3 GetRandomNavMeshPosition(Vector3 origin, float radius) {
            var randomDir = Random.insideUnitSphere * radius + origin;
            return NavMesh.SamplePosition(randomDir, out var hit, radius, NavMesh.AllAreas) ?
                hit.position : origin;
        }

        void CheckForDoors() {
            var colliders = Physics.OverlapSphere(transform.position, doorCheckRadius);
            Door nearest = null;
            float minDist = float.MaxValue;

            foreach (var col in colliders) {
                if (!col.TryGetComponent(out Door door)) continue;
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDist) {
                    minDist = dist;
                    nearest = door;
                }
            }

            if (nearest && Random.value < doorOpenChance)
                nearest.OpenDoor();
        }

        void OnDisable() => CancelInvoke();

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

                // Draw doorway detection range
                if (isChasing) {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireSphere(transform.position, doorwayStopDistance);
                }
            }
        }
    }*/
}