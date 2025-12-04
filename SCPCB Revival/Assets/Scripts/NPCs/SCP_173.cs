using FMODUnity;
using UnityEngine;
using UnityEngine.AI;

public class SCP_173 : MonoBehaviour {
    [Header("Status")]
    [SerializeField] private bool canRoam = true;
    [SerializeField] private bool isVisibleByPlayer = false;
    [SerializeField] private bool isVisibleByAnyNPC = false;
    [SerializeField] private bool alreadySeenByPlayer = false;
    [SerializeField] private bool hasTarget = false;

    [Header("Behavior")]
    [SerializeField, Range(0f, 1f)] float doorOpenChance = 0.3f;
    [SerializeField, Range(5f, 10f)] float doorCheckInterval = 10;
    [Header("Posing")]
    [SerializeField] private string[] poseAnimations;

    [Header("Target Abandonment")]
    [SerializeField] private float maxTargetDistance = 50f;

    [Header("Detection")]
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private float visibilityCheckInterval = 0.1f;
    [SerializeField] private Vector3 eyeOffset = new Vector3(0, 1.5f, 0);

    [Header("Audio")]
    [SerializeField] private GameObject movementSource;
    [SerializeField] private StudioEventEmitter tensionEmitter;
    [SerializeField] private float horrorSoundResetDelay = 1f;

    private Camera playerCamera;
    private NavMeshAgent navMeshAgent;
    private SkinnedMeshRenderer meshRenderer;
    private Transform playerTransform;
    private Animator animator;
    private Plane[] frustumPlanes;
    private PlayerBlink playerBlink;

    private Vector3 roamDestination;
    private Transform target;
    private float roamTimer;
    private float visibilityTimer;
    private float notVisibleTimer;
    private bool horrorSoundReady = true;
    private bool wasVisibleLastFrame = false;
    private bool hasPlayedDistanceHorrorSound = false;

    private const float ROAM_SPEED = 5f;
    private const float ROAM_INTERVAL = 3f;
    private const float ROAM_RADIUS = 10f;
    private const float DOOR_CHECK_RADIUS = 2f;
    private const float CHASE_SPEED = 100f;
    private const float HORROR_SOUND_DISTANCE_THRESHOLD = 5f;

    #region Unity Callbacks
    private void Start() {
        playerCamera = PlayerAccessor.instance.playerCamera;
        playerTransform = PlayerAccessor.instance.transform;
        navMeshAgent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        animator = GetComponent<Animator>();
        roamTimer = ROAM_INTERVAL;

        InvokeRepeating(nameof(CheckForDoors), doorCheckInterval, doorCheckInterval);
        InvokeRepeating(nameof(PlayRandomPose), 0.1f, 0.1f);
    }

    private void Update() {
        visibilityTimer -= Time.deltaTime;
        if (visibilityTimer <= 0f) {
            CheckPlayerVisibility();
            visibilityTimer = visibilityCheckInterval;
        }
        HandleHorrorSoundReset();
        HandleMoving();
    }
    #endregion

    #region Movement Control
    private void HandleMoving() {
        if (isVisibleByPlayer || isVisibleByAnyNPC) {
            if (!PlayerAccessor.instance.isBlinking) {
                movementSource.SetActive(false);
                StopCompletely();
                return;
            }
        }

        if (hasTarget && target != null) {
            if (ShouldAbandonTarget()) {
                AbandonTarget();
                return;
            }
            float currentChaseSpeed = GetChaseSpeed();
            navMeshAgent.speed = currentChaseSpeed;
            navMeshAgent.acceleration = currentChaseSpeed;
            navMeshAgent.SetDestination(target.position);
        }
        else {
            navMeshAgent.speed = ROAM_SPEED;
            navMeshAgent.acceleration = ROAM_SPEED;
            canRoam = true;
            Roam();
        }

        movementSource.SetActive(alreadySeenByPlayer && navMeshAgent.velocity.sqrMagnitude > 0.01f);
    }

    private float GetChaseSpeed() {
        float speed = CHASE_SPEED;
        if (PlayerAccessor.instance.isBlinking) {
            speed *= 2.5f;
        }
        return speed;
    }

    private void StopCompletely() {
        canRoam = false;
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

    private void AbandonTarget() {
        target = null;
        hasTarget = false;
        navMeshAgent.ResetPath();

        if (alreadySeenByPlayer) PlayerAccessor.instance.GetComponentInChildren<PlayerBlink>().StopBlink();
        if (alreadySeenByPlayer) alreadySeenByPlayer = false;

        movementSource.SetActive(false);
        hasPlayedDistanceHorrorSound = false;

        MusicManager.instance.SetMusicState(MusicState.LCZ);
        tensionEmitter.Stop();
    }
    #endregion

    #region Free Roaming
    private void Roam() {
        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0f) {
            roamDestination = GetRandomNavMeshPosition(transform.position, ROAM_RADIUS);
            roamTimer = ROAM_INTERVAL;
        }
        navMeshAgent.SetDestination(roamDestination);
    }

    private Vector3 GetRandomNavMeshPosition(Vector3 origin, float radius) {
        var randomDir = Random.insideUnitSphere * radius + origin;
        return NavMesh.SamplePosition(randomDir, out var hit, radius, NavMesh.AllAreas) ?
            hit.position : origin;
    }
    #endregion

    #region Door Hijacking
    private void CheckForDoors() {
        var colliders = Physics.OverlapSphere(transform.position, DOOR_CHECK_RADIUS);
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

        if (nearest && Random.value < doorOpenChance && !nearest.isOpen && !nearest.requiresKeycard && !nearest.isLocked && !isVisibleByPlayer) {
            nearest.OpenDoor();
            AudioManager.instance.PlaySound(FMODEvents.instance.doorOpen173, nearest.transform.position);
        }
    }
    #endregion

    #region Visibility Detection
    private void CheckPlayerVisibility() {
        bool wasVisible = isVisibleByPlayer;
        isVisibleByPlayer = false;

        if (meshRenderer.isVisible && !PlayerAccessor.instance.isBlinking) {
            frustumPlanes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
            if (GeometryUtility.TestPlanesAABB(frustumPlanes, meshRenderer.bounds)) {
                Vector3 origin = playerCamera.transform.position;
                Vector3 targetPoint = transform.position + eyeOffset;
                Vector3 dir = targetPoint - origin;
                float dist = dir.magnitude;

                if (!Physics.Raycast(origin, dir.normalized, out var hit, dist, obstructionMask) ||
                    hit.transform == transform || hit.transform.IsChildOf(transform)) {
                    isVisibleByPlayer = true;
                }
            }
        }

        if (isVisibleByPlayer && !wasVisible) {
            OnBecameVisibleToPlayer();
        }

        wasVisibleLastFrame = isVisibleByPlayer;
    }

    private void OnBecameVisibleToPlayer() {
        if (!alreadySeenByPlayer) {
            alreadySeenByPlayer = true;
            AcquireTarget(playerTransform);

            MusicManager.instance.SetMusicState(MusicState.scp173);
            tensionEmitter.Play();

            playerBlink = PlayerAccessor.instance.GetComponentInChildren<PlayerBlink>();
            playerBlink.StartBlink();
        }

        if (horrorSoundReady) {
            PlayHorrorSound();
            horrorSoundReady = false;
        }
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
        if (poseAnimations.Length == 0 && isVisibleByPlayer) return;
        string anim = poseAnimations[Random.Range(0, poseAnimations.Length)];
        if (animator != null && !isVisibleByPlayer) {
            animator.Play(anim);
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
        }
        else {
            notVisibleTimer = 0f;
        }
    }

    private void PlayHorrorSound() {
        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist < HORROR_SOUND_DISTANCE_THRESHOLD) {
            AudioManager.instance.PlaySound(FMODEvents.instance.statueHorrorNear, transform.position);
        }
        else {
            if (!hasPlayedDistanceHorrorSound) {
                AudioManager.instance.PlaySound(FMODEvents.instance.statueHorrorFar, transform.position);
                hasPlayedDistanceHorrorSound = true;
            }
        }
    }
    #endregion
}