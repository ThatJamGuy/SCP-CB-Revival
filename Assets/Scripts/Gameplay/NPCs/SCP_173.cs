using FMODUnity;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A rather poorly written script to handle SCP-173 logic but it took forever to get here
/// and it otherwise works fine so I'm afraid to touch it beyond adding on to it.
/// TODO: Refactor this one day or pray a really smart person does it for me.
/// </summary>
public class SCP_173 : MonoBehaviour {
    [Header("Status")]
    public bool isVisibleByPlayer = false;
    [SerializeField] private bool isVisibleByAnyNPC = false;
    [SerializeField] private bool alreadySeenByPlayer = false;
    [SerializeField] private bool hasTarget = false;

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

    [Header("Audio")]
    [SerializeField] private GameObject movementSource;
    [SerializeField] private StudioEventEmitter tensionEmitter;
    [SerializeField] private EventReference neckBreakSound;
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

    private void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Start() {
        playerCamera = Player.Instance.playerCamera;
        playerTransform = Player.Instance.transform;
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
            if (ShouldAbandonTarget()) {
                AbandonTarget();
                return;
            }
            float currentChaseSpeed = GetChaseSpeed();
            navMeshAgent.speed = currentChaseSpeed;
            navMeshAgent.acceleration = currentChaseSpeed;
            navMeshAgent.SetDestination(target.position);
        } else {
            navMeshAgent.speed = ROAM_SPEED;
            navMeshAgent.acceleration = ROAM_SPEED;
            //canRoam = true;
            Roam();
        }

        movementSource.SetActive(alreadySeenByPlayer && navMeshAgent.velocity.sqrMagnitude > 0.01f);
    }

    private float GetChaseSpeed() {
        float speed = CHASE_SPEED;
        if (Player.isBlinking) {
            speed *= 2.5f;
        }
        return speed;
    }

    private void StopCompletely() {
        //canRoam = false;
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
        navMeshAgent.ResetPath();

        //if (alreadySeenByPlayer) PlayerAccessor.instance.GetComponentInChildren<PlayerBlink>().StopBlink();
        if (alreadySeenByPlayer) alreadySeenByPlayer = false;
        if (alreadySeenByPlayer) GameManager.Instance.scp173pursuing = false;

        movementSource.SetActive(false);
        hasPlayedDistanceHorrorSound = false;

        if (!GameManager.Instance.scp106pursuing && !GameManager.Instance.scp049pursuing && !GameManager.Instance.scp096pursuing)
            MusicManager.Instance.SetTrack(MusicManager.MusicTrack.LCZ);

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

    //TODO: Revisit this later to optimize, might make door opening a modular component in the future
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

        // Randomly be able to either open the nearest door or fail to do so and bang on it a bit
        if (nearest && Random.value < doorOpenChance && !nearest.isOpen && !nearest.requiresKeycard && !nearest.isLocked && !isVisibleByPlayer) {
            nearest.OpenDoor();
            AudioManager.PlayOneShot(AudioEventsHolder.Instance.doorOpen173, nearest.transform.position);
        } else if (nearest && !nearest.isOpen) {
            AudioManager.PlayOneShot(AudioEventsHolder.Instance.doorBangEvent, nearest.transform.position);
        }
    }

    #endregion

    #region Visibility Detection

    private void CheckPlayerVisibility() {
        bool wasVisible = isVisibleByPlayer;
        isVisibleByPlayer = false;

        if (meshRenderer.isVisible && !Player.isBlinking) {
            frustumPlanes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
            if (GeometryUtility.TestPlanesAABB(frustumPlanes, meshRenderer.bounds)) {
                Vector3 origin = playerCamera.transform.position;
                Vector3 targetPoint = transform.position + eyeOffset;
                Vector3 dir = targetPoint - origin;
                float dist = dir.magnitude;

                if (dist > maxVisibilityRange) return;

                if (!Physics.Raycast(origin, dir.normalized, out var hit, dist, obstructionMask) ||
                    hit.transform == transform || hit.transform.IsChildOf(transform)) {
                    isVisibleByPlayer = true;
                }
            }
        }

        if (isVisibleByPlayer && !wasVisible) OnBecameVisibleToPlayer();

        wasVisibleLastFrame = isVisibleByPlayer;
    }

    private void OnBecameVisibleToPlayer() {
        if (!alreadySeenByPlayer) {
            alreadySeenByPlayer = true;
            GameManager.Instance.scp173pursuing = true;
            AcquireTarget(playerTransform);

            if (!GameManager.Instance.scp106pursuing && !GameManager.Instance.scp096pursuing && !GameManager.Instance.scp049pursuing)
                MusicManager.Instance.SetTrack(MusicManager.MusicTrack.SCP_173);

            tensionEmitter.Play();

            //playerBlink = PlayerAccessor.instance.GetComponentInChildren<PlayerBlink>();
            //playerBlink.StartBlink();
        }

        if (horrorSoundReady) {
            PlayHorrorSound();
            horrorSoundReady = false;
        }

        AchievementSystem.Instance.GiveAchievement("achv_173");
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
        if (poseAnimations.Length == 0 && isVisibleByPlayer) return;
        string anim = poseAnimations[Random.Range(0, poseAnimations.Length)];
        if (animator != null && !isVisibleByPlayer) {
            animator.Play(anim);
        }
    }

    #endregion

    #region Target Killing

    private void AttemptToKillPlayer() {
        if (!hasTarget || target == null) return;
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= 1.5f && !isVisibleByPlayer) {
            Player.Instance.KillPlayer(2, 0.5f, 0, "Subject D-9341. Cause of death: Fatal cervical fracture. Assumed to be attacked by SCP-173.");
            AudioManager.PlayOneShot(neckBreakSound, transform.position);
            AudioManager.PlayOneShot(AudioEventsHolder.Instance.statueHorrorNear, transform.position);
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
            AudioManager.PlayOneShot(AudioEventsHolder.Instance.statueHorrorNear);
        } else {
            if (!hasPlayedDistanceHorrorSound) {
                AudioManager.PlayOneShot(AudioEventsHolder.Instance.statueHorrorFar);
                hasPlayedDistanceHorrorSound = true;
            }
        }
    }

    #endregion
}