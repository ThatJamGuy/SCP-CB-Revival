using FMODUnity;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SCP_096 : MonoBehaviour {
    public enum State { Puppet, Roaming, Chasing }
    public State currentState = State.Roaming;

    [SerializeField] private float doorOpenRadius;
    [SerializeField] private float doorCheckInterval = 3;
    [SerializeField] private float playerNearbyRadius;

    [Header("Navigation Settings")]
    [SerializeField] private float roamRadius = 15f;
    [SerializeField] private float minIdlePauseTime = 5f;
    [SerializeField] private float maxIdlePauseTime = 10f;
    [SerializeField] private float reachedDestinationThreshold = 0.5f;

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private StudioEventEmitter spatialEventEmitter;
    [SerializeField] private GameObject screamEmitter;

    private RevivalSessionEngine gm;

    private const float KILL_RADIUS = 3;
    private const float REPATH_DISTANCE_SQR = 0.25f;
    private const int MAX_DOOR_COLLIDERS = 32;

    private readonly Collider[] doorHitColliders = new Collider[MAX_DOOR_COLLIDERS];
    private Vector3 lastChaseDestination;

    private static readonly int WalkingHash = Animator.StringToHash("walking");
    private static readonly int DistressHash = Animator.StringToHash("distress");

    private bool isCurrentlyIdling;
    private bool playerInRange;
    private bool playerSawFace;
    private bool chaseMusicStarted;
    private float doorCheckElapsedTime;
    private float idleWaitTimer;

    #region Unity Callbacks

    private void Start() {
        if (RevivalSessionEngine.Instance != null) gm = RevivalSessionEngine.Instance;
    }

    private void Update() {
        switch (currentState) {
            case State.Puppet:
                break;
            case State.Roaming:
                PerformRoamingActivities();
                UpdateRangeBasedMusic();
                break;
            case State.Chasing:
                PerformChasingActivities();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        CheckForDoors();
    }

    #endregion

    #region Private Methods

    private void PerformRoamingActivities() {
        if (isCurrentlyIdling) {
            idleWaitTimer -= Time.deltaTime;

            if (idleWaitTimer <= 0f) {
                if (!TryGetNavMeshPoint(out Vector3 destination))
                    return;

                isCurrentlyIdling = false;
                WalkTo(destination);
            }

            return;
        }

        // Check if destination is reached then begin idle wait
        if (!agent.pathPending && agent.remainingDistance <= reachedDestinationThreshold)
            BeginIdling();

        animator.SetBool(WalkingHash, !isCurrentlyIdling && agent.velocity.sqrMagnitude > 0.01f);
    }

    private void UpdateRangeBasedMusic() {
        float sqrDistanceToPlayer = (Player.Instance.transform.position - transform.position).sqrMagnitude;
        float sqrNearbyRadius = playerNearbyRadius * playerNearbyRadius;

        // Set music track based on range state
        if (!playerInRange) {
            if (sqrDistanceToPlayer < sqrNearbyRadius) {
                RevivalSessionEngine.Instance.PlayChaseTrack(1, MusicManager.MusicTrack.SCP_096, 0);
                gm.playerNear096 = true;
                playerInRange = true;
            }
        } else if (playerInRange) {
            if (sqrDistanceToPlayer > sqrNearbyRadius) {
                RevivalSessionEngine.Instance.PlayChaseTrack(-1, MusicManager.MusicTrack.SCP_096, 0);
                gm.playerNear096 = false;
                playerInRange = false;
            }
        }
    }

    private void BeginIdling() {
        isCurrentlyIdling = true;
        idleWaitTimer = UnityEngine.Random.Range(minIdlePauseTime, maxIdlePauseTime);
        animator.SetBool(WalkingHash, false);
    }

    private void PerformRage() {
        if (!playerSawFace) {
            playerSawFace = true;

            animator.SetTrigger(DistressHash);
            spatialEventEmitter.Stop();

            MusicManager.Instance.StopAllMusic();
            AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.scp096Triggered);
            AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.scp096ChargeUp, transform.position);
        }
    }

    private void PerformChasingActivities() {
        Vector3 playerPosition = Player.Instance.transform.position;
        float sqrDistanceToPlayer = (playerPosition - transform.position).sqrMagnitude;

        // Temp; Later set to possible NPC targets if necessary
        if (!(agent.hasPath || agent.pathPending) ||
            (playerPosition - lastChaseDestination).sqrMagnitude >= REPATH_DISTANCE_SQR) {
            lastChaseDestination = playerPosition;
            agent.SetDestination(playerPosition);
        }

        if (!chaseMusicStarted) {
            RevivalSessionEngine.Instance.PlayChaseTrack(6, MusicManager.MusicTrack.SCP_096, 1);
            chaseMusicStarted = true;
        }

        if (sqrDistanceToPlayer < KILL_RADIUS * KILL_RADIUS && !Player.Instance.isDead) {
            Player.Instance.KillPlayer(3, 0.5f, 0, "A large amount of blood found in [DATA REDACTED]. DNA indentified as Subject D-9341. Most likely [DATA REDACTED] by SCP-096.");
            AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.scp096KillPlayer);
            Destroy(gameObject);
        }
    }

    private bool TryGetNavMeshPoint(out Vector3 result) {
        // Sample random points until a valid NavMesh position is found
        for (var i = 0; i < 10; i++) {
            var randomPoint = transform.position + UnityEngine.Random.insideUnitSphere * roamRadius;
            randomPoint.y = transform.position.y;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, roamRadius, NavMesh.AllAreas)) {
                result = hit.position;
                return true;
            }
        }

        result = transform.position;
        return false;
    }

    private void CheckForDoors() {
        doorCheckElapsedTime += Time.deltaTime;

        // Check for nearby doors every 3 seconds and open them if found
        if (doorCheckElapsedTime >= doorCheckInterval) {
            doorCheckElapsedTime = 0;

            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, doorOpenRadius, doorHitColliders);

            for (int i = 0; i < hitCount; i++) {
                if (doorHitColliders[i].TryGetComponent<Door>(out Door door)) {
                    if (!chaseMusicStarted) {
                        door.OpenDoor();
                        return;
                    } else {
                        if (door.isOpen) return;

                        bool hasBustedDoor = false;

                        if (!hasBustedDoor && !door.isBroken) {
                            hasBustedDoor = true;

                            // Calculate direction away from SCP-096
                            Vector3 explosionDirection = (door.transform.position - transform.position).normalized;
                            door.EnableGravityOnDoors(explosionDirection, 50f);

                            AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.doorExplode, transform.position);
                            return;
                        }
                    }
                }
            }
        }
    }

    private IEnumerator DistressCoroutine() {
        yield return new WaitForSeconds(31);
        agent.isStopped = false;
        agent.speed = 15;
        agent.acceleration = 60;
        agent.angularSpeed = 360;
        currentState = State.Chasing;
        screamEmitter.SetActive(true);
        doorCheckInterval = 0.1f;
    }
    #endregion

    #region Public Methods

    public void WalkTo(Vector3 position) {
        agent.SetDestination(position);
        isCurrentlyIdling = false;

        PerformRoamingActivities();
    }

    public void Teleport(Vector3 position) {
        gameObject.transform.position = position;
        agent.Warp(position);
    }

    public void TriggerDistress() {
        agent.SetDestination(transform.position);
        agent.isStopped = true;
        currentState = State.Puppet;

        PerformRage();
        StartCoroutine(DistressCoroutine());
    }
    #endregion
}