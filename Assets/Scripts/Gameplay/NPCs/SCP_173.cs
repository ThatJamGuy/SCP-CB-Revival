using FMODUnity;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A rather poorly written script to handle SCP-173 logic but it took forever to get here
/// and it otherwise works fine so I'm afraid to touch it beyond adding on to it.
/// TODO: Refactor this one day.
/// </summary>
public class SCP_173 : MonoBehaviour {
    // Values pertaining to the max amount of failed door open attempts per difficulty
    // before a door is allowed to be downed on the next failure
    // Will probably later make each door have it's own health value per difficulty instead
    private const int SAFE_DOOR_BASH_AMOUNT = 20;
    private const int EUCLID_DOOR_BASH_AMOUNT = 10;
    private const int KETER_DOOR_BASH_AMOUNT = 5;

    private const float ROAM_SPEED = 5f;
    private const float ROAM_INTERVAL = 3f;
    private const float ROAM_RADIUS = 10f;
    private const float DOOR_CHECK_RADIUS = 2f;
    private const float CHASE_SPEED = 200f;
    private const float HORROR_SOUND_DISTANCE_THRESHOLD = 5f;
    private const float REPATH_DISTANCE_SQR = 0.25f;
    private const int MAX_DOOR_COLLIDERS = 32;
    private const int SAMPLE_COUNT = 5;

    [Header("Status")]
    public bool isVisibleByPlayer;
    public bool disableKilling;

    [SerializeField] private bool isVisibleByAnyNPC;
    [SerializeField] private bool alreadySeenByPlayer;
    [SerializeField] private bool hasTarget;

    [Header("Behavior")]
    [SerializeField, Range(0f, 1f)] float doorOpenChance = 0.3f;
    [SerializeField, Range(1f, 10f)] float doorCheckInterval = 10;
    [SerializeField] private float maxTargetDistance;

    [Header("Posing")]
    [SerializeField] private string[] poseAnimations;

    [Header("Detection")]
    [SerializeField] private float maxVisibilityRange = 20f;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private float visibilityCheckInterval = 0.1f;
    [SerializeField] private Vector3 eyeOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] private float frustumPadding = 0.3f;      // widens the "seen" area on screen
    [SerializeField] private float lateralSampleRadius = 0.35f; // how far left/right of the body to sample
    [SerializeField] private float lowSampleHeight = 0.3f;      // lower body sample height

    [Header("Audio")]
    [SerializeField] private GameObject movementSource;
    [SerializeField] private StudioEventEmitter tensionEmitter;
    [SerializeField] private EventReference neckBreakSound;
    [SerializeField] private float horrorSoundResetDelay = 1f;

    private readonly Plane[] frustumPlanes = new Plane[6];
    private readonly Collider[] doorColliders = new Collider[MAX_DOOR_COLLIDERS];
    private readonly Vector3[] samplePoints = new Vector3[SAMPLE_COUNT];
    private int[] poseAnimationHashes;
    private Vector3 lastRequestedDestination;

    private Camera playerCamera;
    private NavMeshAgent navMeshAgent;
    private SkinnedMeshRenderer meshRenderer;
    private Transform playerTransform;
    private Animator animator;

    private Vector3 roamDestination;
    private Transform target;
    private float roamTimer;
    private float visibilityTimer;
    private float notVisibleTimer;
    private bool horrorSoundReady = true;
    private bool hasPlayedDistanceHorrorSound = false;
    private int currentDoorBang;

    #region Unity Lifecycle

    private void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        animator = GetComponent<Animator>();

        poseAnimationHashes = new int[poseAnimations.Length];
        for (int i = 0; i < poseAnimations.Length; i++)
            poseAnimationHashes[i] = Animator.StringToHash(poseAnimations[i]);
    }

    private void OnDestroy() {
        MusicManager music = MusicManager.Instance;
        if (music != null) music.ReleaseChaseTrack(this);

        EntitySystem system = EntitySystem.Instance;
        if (system == null) return;

        Entity entity = system.cached173Entity;
        if (entity == null) return;

        if (hasTarget) entity.chasingPlayer = false;
        system.UnregisterEntity(entity, gameObject);
    }

    private void Start() {
        playerCamera = Player.Instance.playerCamera;
        playerTransform = Player.Instance.transform;
        roamTimer = ROAM_INTERVAL;

        InvokeRepeating(nameof(CheckForDoors), doorCheckInterval, doorCheckInterval);
        InvokeRepeating(nameof(PlayRandomPose), 0.05f, 0.05f);
    }

    private void Update() {
        visibilityTimer -= Time.deltaTime;
        if (visibilityTimer <= 0f) {
            CheckPlayerVisibility();
            visibilityTimer = visibilityCheckInterval;
        }

        HandleHorrorSoundReset();
        HandleMoving();

        if (hasTarget && !Player.Instance.isDead) AttemptToKillPlayer();
    }

    #endregion

    #region Movement Control

    private void HandleMoving() {
        if (isVisibleByPlayer || isVisibleByAnyNPC) {
            if (!Player.isBlinking) {
                movementSource.SetActive(false);
                StopCompletely();
                return;
            }
        }

        if (hasTarget && target != null) {
            if (target == null || ShouldAbandonTarget()) {
                AbandonTarget();
                return;
            }

            float currentChaseSpeed = GetChaseSpeed();
            navMeshAgent.speed = currentChaseSpeed;
            navMeshAgent.acceleration = currentChaseSpeed;
            SetDestinationIfChanged(target.position);
        } else {
            navMeshAgent.speed = ROAM_SPEED;
            navMeshAgent.acceleration = ROAM_SPEED;
            Roam();
        }

        movementSource.SetActive(alreadySeenByPlayer && navMeshAgent.velocity.sqrMagnitude > 0.1f);
    }

    private float GetChaseSpeed() {
        float speed = CHASE_SPEED;
        if (Player.isBlinking) speed *= 2.7f;
        return speed;
    }

    private void SetDestinationIfChanged(Vector3 destination) {
        if ((navMeshAgent.hasPath || navMeshAgent.pathPending) &&
            (destination - lastRequestedDestination).sqrMagnitude < REPATH_DISTANCE_SQR) return;

        lastRequestedDestination = destination;
        navMeshAgent.SetDestination(destination);
    }

    private void StopCompletely() {
        if (!navMeshAgent.hasPath) return;
        navMeshAgent.ResetPath();
        navMeshAgent.velocity = Vector3.zero;
    }
    #endregion

    #region Target Abandonment

    private bool ShouldAbandonTarget() {
        if (target == null) return true;
        return Vector3.Distance(transform.position, target.position) > maxTargetDistance;
    }

    public void AbandonTarget() {
        target = null;
        hasTarget = false;
        alreadySeenByPlayer = false;
        hasPlayedDistanceHorrorSound = false;
        currentDoorBang = 0;

        MusicManager music = MusicManager.Instance;
        if (music != null) music.ReleaseChaseTrack(this);

        if (navMeshAgent.isOnNavMesh) navMeshAgent.ResetPath();
        movementSource.SetActive(false);
        if (tensionEmitter != null) tensionEmitter.Stop();

        EntitySystem system = EntitySystem.Instance;
        if (system != null && system.cached173Entity != null)
            system.cached173Entity.chasingPlayer = false;
    }

    #endregion

    #region Free Roaming

    private void Roam() {
        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0f) {
            roamDestination = GetRandomNavMeshPosition(transform.position, ROAM_RADIUS);
            roamTimer = ROAM_INTERVAL;
        }
        SetDestinationIfChanged(roamDestination);
    }

    private Vector3 GetRandomNavMeshPosition(Vector3 origin, float radius) {
        var randomDir = Random.insideUnitSphere * radius + origin;
        return NavMesh.SamplePosition(randomDir, out var hit, radius, NavMesh.AllAreas) ?
            hit.position : origin;
    }

    #endregion

    #region Door Hijacking

    //TODO: Revisit this later to optimize, might make door opening a modular component in the future
    private void CheckForDoors() {
        Vector3 origin = transform.position;
        int count = Physics.OverlapSphereNonAlloc(origin, DOOR_CHECK_RADIUS, doorColliders);
        Door nearestDoor = null;
        float minSqrDist = float.MaxValue;

        for (int i = 0; i < count; i++) {
            Collider col = doorColliders[i];
            if (!col.TryGetComponent(out Door door)) continue;
            float sqrDist = (col.transform.position - origin).sqrMagnitude;
            if (sqrDist < minSqrDist) {
                minSqrDist = sqrDist;
                nearestDoor = door;
            }
        }

        // Randomly be able to either open the nearest door or fail to do so and bang on it a bit
        if (nearestDoor && Random.value < doorOpenChance && !nearestDoor.isOpen && !nearestDoor.requiresKeycard && !nearestDoor.isLocked && !isVisibleByPlayer && !nearestDoor.disableNPCUseage) {
            nearestDoor.OpenDoor();
            AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.doorOpen173, nearestDoor.transform.position);
        } else if (nearestDoor && !nearestDoor.isOpen && EntitySystem.Instance.cached173Entity.chasingPlayer && !isVisibleByPlayer && !nearestDoor.disableNPCUseage) {

            currentDoorBang++;

            // Difficulty is Euclid and the door was banged on 10 times during the chase, so bust it down OR
            // Difficulty is Keter and the door was banged on 5 times during the chase, so bust it down
            if (RevivalSessionEngine.currentDifficulty == 0 && currentDoorBang >= SAFE_DOOR_BASH_AMOUNT ||
                (RevivalSessionEngine.currentDifficulty == 1 && currentDoorBang >= EUCLID_DOOR_BASH_AMOUNT) ||
                (RevivalSessionEngine.currentDifficulty == 2 && currentDoorBang >= KETER_DOOR_BASH_AMOUNT)) {

                nearestDoor.ExplodeDoor(transform, 80);

                // Reset currentDoorBang and return to restart the count and prevent another bang from happening during the explode
                currentDoorBang = 0;
                return;
            }

            AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.doorBangEvent, nearestDoor.transform.position);
        }
    }

    #endregion

    #region Visibility Detection

    private void CheckPlayerVisibility() {
        bool wasVisible = isVisibleByPlayer;
        isVisibleByPlayer = false;

        if (!Player.isBlinking) {
            Bounds bounds = meshRenderer.bounds;
            bounds.Expand(frustumPadding);

            Matrix4x4 worldToProjectionMatrix = playerCamera.projectionMatrix * playerCamera.worldToCameraMatrix;
            GeometryUtility.CalculateFrustumPlanes(worldToProjectionMatrix, frustumPlanes);

            if (GeometryUtility.TestPlanesAABB(frustumPlanes, bounds)) {
                Transform camTransform = playerCamera.transform;
                Vector3 origin = camTransform.position;
                Vector3 pos = transform.position;
                Vector3 toCenter = pos + eyeOffset - origin;
                if (toCenter.sqrMagnitude > maxVisibilityRange * maxVisibilityRange) return;

                Vector3 right = camTransform.right * lateralSampleRadius;
                Vector3 mid = pos + eyeOffset * 0.5f;
                samplePoints[0] = pos + eyeOffset;
                samplePoints[1] = mid;
                samplePoints[2] = pos + new Vector3(0f, lowSampleHeight, 0f);
                samplePoints[3] = mid + right;
                samplePoints[4] = mid - right;

                for (int i = 0; i < SAMPLE_COUNT; i++) {
                    Vector3 dir = samplePoints[i] - origin;
                    float dist = dir.magnitude;
                    if (dist < 0.0001f) { isVisibleByPlayer = true; break; }

                    if (!Physics.Raycast(origin, dir / dist, out RaycastHit hit, dist, obstructionMask) ||
                        hit.transform.IsChildOf(transform)) {
                        isVisibleByPlayer = true;
                        break;
                    }
                }
            }
        }

        if (isVisibleByPlayer && !wasVisible) OnBecameVisibleToPlayer();
    }

    private void OnBecameVisibleToPlayer() {
        if (!alreadySeenByPlayer) {
            alreadySeenByPlayer = true;
            AcquireTarget(playerTransform);

            // Let the entity system know that 173 is now actively chasing the player and shouldn't be messed with
            EntitySystem.Instance.cached173Entity.chasingPlayer = true;

            // Make a request for SCP-173 chase music
            MusicManager.Instance.RequestChaseTrack(this, 1, MusicManager.MusicTrack.SCP_173, 1);

            tensionEmitter.Play();
        }

        if (horrorSoundReady) {
            PlayHorrorSound();
            horrorSoundReady = false;
        }

        RevivalRuntimeEngine.Instance.GiveAchievement("achv_173");
    }

    private void AcquireTarget(Transform newTarget) {
        target = newTarget;
        hasTarget = true;
        navMeshAgent.speed = CHASE_SPEED;
    }

    public void SetVisibleByNPC(bool visible) => isVisibleByAnyNPC = visible;

    #endregion

    #region Posing

    public void PlayRandomPose() {
        // Use the animator to change 173 poses
        if (poseAnimationHashes.Length == 0 || isVisibleByPlayer || animator == null) return;
        animator.Play(poseAnimationHashes[Random.Range(0, poseAnimationHashes.Length)]);
    }

    #endregion

    #region Target Killing

    private void AttemptToKillPlayer() {
        if (!hasTarget || target == null || disableKilling) return;
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= 1.5f && !isVisibleByPlayer) {
            Player.Instance.KillPlayer(2, 0.5f, 0, "Subject D-9341. Cause of death: Fatal cervical fracture. Assumed to be attacked by SCP-173.");
            AudioManager.PlayOneShot(neckBreakSound, transform.position);
            AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.statueHorrorNear, transform.position);
            Destroy(gameObject);
        }

    }

    #endregion

    #region Audio

    private void HandleHorrorSoundReset() {
        if (!isVisibleByPlayer) {
            notVisibleTimer += Time.deltaTime;
            if (notVisibleTimer >= horrorSoundResetDelay) {
                horrorSoundReady = true;
            }
        } else {
            notVisibleTimer = 0f;
        }
    }

    private void PlayHorrorSound() {
        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist < HORROR_SOUND_DISTANCE_THRESHOLD) {
            AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.statueHorrorNear);
        } else {
            if (!hasPlayedDistanceHorrorSound) {
                AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.statueHorrorFar);
                hasPlayedDistanceHorrorSound = true;
            }
        }
    }

    #endregion
}