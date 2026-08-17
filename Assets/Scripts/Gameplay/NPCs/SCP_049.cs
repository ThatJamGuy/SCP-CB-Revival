using FMODUnity;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class SCP_049 : MonoBehaviour {
    private enum State { None, Roaming, Chasing, Checking };

    [SerializeField] private State state = State.None;
    [SerializeField] private LayerMask obstructionLayers;
    [SerializeField] private LayerMask playerLayer;

    [Header("AI Settings")]
    [SerializeField] private float wanderingRadius;
    [SerializeField] private float wanderingTime;
    [SerializeField] private float predictionUpdateInterval = 0.4f;
    [SerializeField] private float maxPredictionLookahead = 2f;
    [SerializeField] private float maxPredictedSpeed = 8f;
    [SerializeField] private float killRadius;
    [SerializeField] private int minVoiceTime;
    [SerializeField] private int maxVoiceTime;

    [Header("Detection")]
    [SerializeField] private float visibilityRange;
    [SerializeField, Range(1, 90)] private int visibilityConeAngle;
    [SerializeField] private Transform headTransform;
    [SerializeField] private float plagueDoctoringRange = 25;
    [SerializeField] private float maxLostTargetTime = 5;

    [Header("Door Manip Settings")]
    [SerializeField] private LayerMask doorLayer;
    [SerializeField] private float doorCheckInterval;
    [SerializeField] private float doorOpenRadius;

    [Header("Audio")]
    [SerializeField] private EventReference spottedPlayerStinger;
    [SerializeField] private EventReference killedPlayerStinger;
    [SerializeField] private EventReference spottedSpeech;
    [SerializeField] private EventReference searchingSpeech;

    [Header("Animation")]
    [SerializeField] private string rmotionWalkingBool = "walking_rmotion";
    [SerializeField] private string checkingBool = "checking";
    [SerializeField] private float checkAnimationTimer;

    [Header("References")]
    [SerializeField] private Transform voiceSource;
    [SerializeField] private TwoBoneIKConstraint handIKConstraint;
    [SerializeField] private Transform ikHandTarget;

    private const float IK_DISTANCE = 5f;
    private const float IK_DISTANCE_SQR = IK_DISTANCE * IK_DISTANCE;
    private const float IK_BLEND_SPEED = 5f;
    private const int MAX_COLLIDERS = 5;

    private Animator animator;
    private NavMeshAgent agent;
    private IK_MasterComponent masterIKComponent;
    private Transform currentTarget;
    private Collider[] hitColliders;
    private Camera playerCamera;

    private Vector3 lastKnownTargetPos;
    private Vector3 previousTargetPos;
    private Vector3 lastKnownVelocity;

    private bool isMoving;
    private bool predicting;
    private bool hasRootMotionBinding;
    private int rmotionWalkingBoolHash;
    private int checkingBoolHash;
    private float doorCheckElapsedTime;
    private float wanderTimer;
    private float lostSightTime;
    private float checkElapsedTime;
    private float predictionElapsedTime;
    private float sqrDistanceToPlayer;
    private float visibilityRangeSqr;
    private float plagueDoctoringRangeSqr;
    private float visibilityConeHalfAngleCos;

    #region Unity Callbacks

    private void Awake() {
        hitColliders = new Collider[MAX_COLLIDERS];

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        masterIKComponent = GetComponent<IK_MasterComponent>();
    }

    private void Start() {
        // hash some animator bools for optimization I guess
        rmotionWalkingBoolHash = Animator.StringToHash(rmotionWalkingBool);
        checkingBoolHash = Animator.StringToHash(checkingBool);
        hasRootMotionBinding = animator != null && agent != null;

        // setup some visibility and range values
        visibilityRangeSqr = visibilityRange * visibilityRange;
        plagueDoctoringRangeSqr = plagueDoctoringRange * plagueDoctoringRange;
        visibilityConeHalfAngleCos = Mathf.Cos(visibilityConeAngle * 0.5f * Mathf.Deg2Rad);

        // necessary for allow rmotion to work
        agent.updatePosition = false;
        agent.updateRotation = false;

        // grab the player camera
        if (playerCamera == null && Player.Instance != null)
            playerCamera = Player.Instance.playerCamera;
    }

    private void Update() {
        if (playerCamera == null || agent == null) return;

        if (state != State.None)
            sqrDistanceToPlayer = (playerCamera.transform.position - transform.position).sqrMagnitude;

        masterIKComponent.enableHeadIK = state == State.Chasing;

        CheckForDoors();

        switch (state) {
            case State.Roaming:
                RoamingState();
                break;

            case State.Chasing:
                ChasingState();
                if (state == State.None) return;
                UpdateHandIK();
                break;

            case State.Checking:
                CheckingState();
                break;
        }

        if (!agent.enabled) return;

        isMoving = agent.remainingDistance > agent.stoppingDistance;

        animator.SetBool(rmotionWalkingBoolHash, isMoving);

        if (!isMoving) return;

        Vector3 direction = agent.desiredVelocity.normalized;
        if (direction.sqrMagnitude > 0.01f) {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        agent.nextPosition = transform.position;
    }

    private void LateUpdate() {
        if (hasRootMotionBinding) {
            animator.transform.rotation = animator.rootRotation;
        }
    }

    private void OnAnimatorMove() {
        if (hasRootMotionBinding) {
            if (!agent.enabled) return;

            Vector3 rootMotion = animator.deltaPosition;
            transform.position += new Vector3(rootMotion.x, 0f, rootMotion.z);
            var pos = transform.position;
            pos.y = agent.nextPosition.y;

            transform.position = pos;
            agent.nextPosition = pos;
        }
    }

    #endregion

    #region Private Methods

    private void UpdateHandIK() {
        if (currentTarget == null) return;

        bool isWithinRange = (currentTarget.position - transform.position).sqrMagnitude < IK_DISTANCE_SQR;

        handIKConstraint.weight = Mathf.MoveTowards(handIKConstraint.weight, isWithinRange ? 1f : 0f, IK_BLEND_SPEED * Time.deltaTime);
    }

    private void ChasingState() {
        if (CanSeePlayer()) {
            UpdateTargetTracking();
            lostSightTime = 0;
            WalkTo(lastKnownTargetPos);
            CheckKillPlayer();
            return;
        }

        lostSightTime += Time.deltaTime;

        if (lostSightTime < maxLostTargetTime) {
            UpdateTargetTracking();
            WalkTo(lastKnownTargetPos);
            CheckKillPlayer();
            return;
        }

        EnterCheckingState();
    }

    private void UpdateTargetTracking() {
        Vector3 targetPos = currentTarget.position;
        Vector3 instantVelocity = (targetPos - previousTargetPos) / Mathf.Max(Time.deltaTime, 0.0001f);
        instantVelocity = Vector3.ClampMagnitude(instantVelocity, maxPredictedSpeed);
        lastKnownVelocity = Vector3.Lerp(lastKnownVelocity, instantVelocity, 0.3f);
        previousTargetPos = targetPos;
        lastKnownTargetPos = targetPos;
    }

    private void CheckKillPlayer() {
        if (sqrDistanceToPlayer / 2 <= killRadius) {
            if (Player.Instance.isDead) {
                state = State.None;
                return;
            }

            state = State.None;
            AudioManager.PlayOneShot(killedPlayerStinger);
            Player.Instance.KillPlayer(1, 0.3f, 0, "An active instance of SCP-049-2 was discovered in [REDACTED]. Terminated by Nine-Tailed Fox.");
            Destroy(gameObject);
        }
    }

    private void EnterCheckingState() {
        state = State.Checking;
        predicting = true;
        predictionElapsedTime = 0;

        if (!GameManager.Instance.scp096pursuing && !GameManager.Instance.scp106pursuing)
            MusicManager.Instance.SetTrack(MusicManager.MusicTrack.SCP_049, 0);
    }

    private void CheckingState() {
        if (sqrDistanceToPlayer > plagueDoctoringRangeSqr) {
            MusicManager.Instance.SetTrack(MusicManager.MusicTrack.LCZ, 0);

            predicting = false;
            animator.SetBool(checkingBoolHash, false);
            wanderTimer = 0;
            state = State.Roaming;
            return;
        }

        if (CanSeePlayer()) {
            predicting = false;
            animator.SetBool(checkingBoolHash, false);
            lostSightTime = 0;
            state = State.Chasing;
            On049SawPlayer();
            return;
        }

        if (predicting) {
            PredictTargetPosition();
            return;
        }

        checkElapsedTime += Time.deltaTime;

        if (checkElapsedTime >= checkAnimationTimer) {
            animator.SetBool(checkingBoolHash, false);
            wanderTimer = 0;
            state = State.Roaming;
        }
    }

    private void PredictTargetPosition() {
        if (agent == null) return;

        lostSightTime += Time.deltaTime;
        predictionElapsedTime += Time.deltaTime;

        if (predictionElapsedTime >= predictionUpdateInterval) {
            predictionElapsedTime = 0;

            float lookahead = Mathf.Min(lostSightTime - maxLostTargetTime, maxPredictionLookahead);
            Vector3 predictedPos = lastKnownTargetPos + lastKnownVelocity * lookahead;

            if (NavMesh.SamplePosition(predictedPos, out NavMeshHit navHit, 3f, NavMesh.AllAreas)) {
                lastKnownTargetPos = navHit.position;
                WalkTo(lastKnownTargetPos);
            }
        }

        if (!agent.pathPending && agent.remainingDistance <= 1f) {
            predicting = false;
            checkElapsedTime = 0;
            animator.SetBool(checkingBoolHash, true);
        }
    }

    private void RoamingState() {
        if (CanSeePlayer()) {
            state = State.Chasing;
            On049SawPlayer();
            return;
        }

        wanderTimer += Time.deltaTime;

        if (wanderTimer >= wanderingTime) {
            Vector3 newPos = RandomNavSphere(transform.position, wanderingRadius, -1);
            WalkTo(newPos);
            wanderTimer = 0;
        }
    }

    private void CheckForDoors() {
        doorCheckElapsedTime += Time.deltaTime;

        if (doorCheckElapsedTime >= doorCheckInterval) {
            doorCheckElapsedTime = 0;

            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, doorOpenRadius, hitColliders, doorLayer);

            for (int i = 0; i < numColliders; i++) {
                if (hitColliders[i].TryGetComponent<Door>(out Door door)) {
                    door.OpenDoor();
                    break;
                }
            }
        }
    }

    private void On049SawPlayer() {
        currentTarget = Player.Instance.transform;
        lostSightTime = 0;
        predicting = false;
        previousTargetPos = currentTarget.position;
        lastKnownVelocity = Vector3.zero;
        predictionElapsedTime = 0;

        AudioManager.PlayOneShot(spottedPlayerStinger);

        if (!GameManager.Instance.scp096pursuing && !GameManager.Instance.scp106pursuing)
            MusicManager.Instance.SetTrack(MusicManager.MusicTrack.SCP_049, 1);

        RevivalRuntimeEngine.Instance.GiveAchievement("achv_049");
    }

    #endregion

    #region Private Utilities

    public static Vector3 RandomNavSphere(Vector3 origin, float distance, int layerMask) {
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, distance, layerMask);
        return navHit.position;
    }

    private bool CanSeePlayer() {
        if (sqrDistanceToPlayer > visibilityRangeSqr) return false;

        Vector3 toPlayer = playerCamera.transform.position - headTransform.position;
        float sqrDist = toPlayer.sqrMagnitude;
        if (sqrDist > visibilityRangeSqr) return false;

        Vector3 dirToPlayer = toPlayer.normalized;
        float dot = Vector3.Dot(transform.forward, dirToPlayer);
        if (dot < visibilityConeHalfAngleCos) return false;

        float dist = Mathf.Sqrt(sqrDist);
        if (Physics.Raycast(headTransform.position, dirToPlayer, dist, obstructionLayers))
            return false;

        return true;
    }

    #endregion

    #region Public Methods

    public void WalkTo(Vector3 position) {
        if (agent == null) return;

        agent.SetDestination(position);
    }

    public void Speak(EventReference toSpeak) {
        AudioManager.PlayOneShot(toSpeak, voiceSource.position);
    }

    #endregion
}