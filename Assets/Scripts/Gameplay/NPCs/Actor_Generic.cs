using EditorAttributes;
using FMODUnity;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Master script for puppeteering fellas on the field.
/// Yes this is an absolute mess but I have a habit of trying to future proof things.
/// 
/// WARNING: UNFINISHED AND BUGGY
/// </summary>
public class Actor_Generic : MonoBehaviour {
    [Header("General Settings")]
    [SerializeField] private string actorName = "Generic Actor";

    [Header("Animation Settings")]
    [SerializeField] private bool useRootMotion;
    [SerializeField, ShowField(nameof(useRootMotion))] private string rmotionWalkingBool = "walking_rmotion";
    [SerializeField, ShowField(nameof(useRootMotion))] private bool useStopWalkingAnim;
    [SerializeField, ShowField(nameof(useStopWalkingAnim))] private string stopWalkingTrigger = "stop_walking";
    [SerializeField, ShowField(nameof(useStopWalkingAnim))] private float stopWalkingDistance;
    [SerializeField] private bool playAnimOnStart;
    [SerializeField, ShowField(nameof(playAnimOnStart))] private bool randomAnimOnStart;
    [SerializeField, ShowField(nameof(playAnimOnStart))] private bool randomAnimSpeedOnStart;
    [SerializeField, ShowField(nameof(playAnimOnStart))] private string[] startingAnimList;
    [SerializeField, ShowField(nameof(randomAnimSpeedOnStart))] private int minAnimSpeed;
    [SerializeField, ShowField(nameof(randomAnimSpeedOnStart))] private int maxAnimSpeed;

    [Header("AI Settings")]
    [SerializeField] private bool wanderRandomly;
    [SerializeField, ShowField(nameof(wanderRandomly))] private float wanderingRadius;
    [SerializeField, ShowField(nameof(wanderRandomly))] private float wanderingTime;

    [Header("References")]
    [SerializeField] private Animator actorAnimator;
    [SerializeField] private NavMeshAgent actorAgent;
    [SerializeField] private Transform voiceSource;

    private int rmotionWalkingBoolHash;
    private int rmotionStopWalkingTriggerHash;
    private bool useRootMotionNav;
    private bool isMoving;
    private float wanderTimer;

    #region Unity Callbacks

    private void OnEnable() {
        if (wanderRandomly)
            wanderTimer = wanderingTime;
    }

    private void Start() {
        if (useRootMotion) {
            actorAgent.updatePosition = false;
            actorAgent.updateRotation = false;
        }

        useRootMotionNav = useRootMotion && actorAnimator != null && actorAgent != null && rmotionWalkingBool != null;

        if (useRootMotionNav) {
            rmotionWalkingBoolHash = Animator.StringToHash(rmotionWalkingBool);

            if (useStopWalkingAnim)
                rmotionStopWalkingTriggerHash = Animator.StringToHash(stopWalkingTrigger);
        }

        // Play specific animation on start if applicable
        if (playAnimOnStart) {
            if (randomAnimSpeedOnStart) {
                actorAnimator.speed = Random.Range(minAnimSpeed, maxAnimSpeed);
            }

            if (randomAnimOnStart) {
                PlayAnimation(startingAnimList[Random.Range(0, startingAnimList.Length)]);
                return;
            }

            // Random anims off so play the first one
            PlayAnimation(startingAnimList[0]);
        }
    }

    private void Update() {
        if (actorAgent != null && wanderRandomly) {
            wanderTimer += Time.deltaTime;

            if (wanderTimer >= wanderingTime) {
                Vector3 newPos = RandomNavSphere(transform.position, wanderingRadius, -1);
                WalkTo(newPos);
                wanderTimer = 0;
            }
        }

        // If root motion navigation is to be utilized then manage that stuff
        if (useRootMotionNav) {
            isMoving = actorAgent.remainingDistance > actorAgent.stoppingDistance;

            actorAnimator.SetBool(rmotionWalkingBoolHash, isMoving);

            if (!isMoving) return;

            if (actorAgent.remainingDistance <= stopWalkingDistance && actorAgent.remainingDistance > actorAgent.stoppingDistance && useStopWalkingAnim) {
                actorAnimator.SetTrigger(rmotionStopWalkingTriggerHash);
            }

            Vector3 direction = actorAgent.desiredVelocity.normalized;
            if (direction.sqrMagnitude > 0.01f) {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }

            actorAgent.nextPosition = transform.position;
        }
    }

    private void LateUpdate() {
        // If root motion navigation is to be utilized then manage that stuff
        if (actorAnimator != null && actorAgent != null && rmotionWalkingBool != null && useRootMotion) {
            var targetRotation = actorAnimator.rootRotation;
            actorAnimator.transform.rotation = targetRotation;
        }
    }

    private void OnAnimatorMove() {
        // If root motion navigation is to be utilized then manage that stuff
        if (actorAnimator != null && actorAgent != null && rmotionWalkingBool != null && useRootMotion) {
            if (!actorAgent.enabled) return;

            Vector3 rootMotion = actorAnimator.deltaPosition;
            transform.position += new Vector3(rootMotion.x, 0f, rootMotion.z);
            var pos = transform.position;
            pos.y = actorAgent.nextPosition.y;

            transform.position = pos;
            actorAgent.nextPosition = pos;
        }
    }

    #endregion

    #region Private Helpers

    public static Vector3 RandomNavSphere(Vector3 origin, float distance, int layerMask) {
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, distance, layerMask);
        return navHit.position;
    }

    #endregion

    #region Public Methods

    public void PlayAnimation(string animationName) {
        if (actorAnimator == null) {
            Debug.Log("<color=red>[Actor_Generic]</color> The actorAnimator reference of this actor was left null, animation related tasks will not work.");
            return;
        }

        actorAnimator.speed = 1;
        actorAnimator.Play(animationName);
    }

    public void Speak(EventReference toSpeak) {
        AudioManager.PlayOneShot(toSpeak, voiceSource.position);
    }

    public void WalkTo(Vector3 position) {
        if (actorAgent == null || actorAnimator == null) return;

        actorAnimator.speed = 1;
        actorAgent.SetDestination(position);
    }

    public void Warp(Vector3 position) {
        gameObject.transform.position = position;
        actorAgent.Warp(position);
    }

    public void StopCurrentTask() {
        if (actorAgent != null && actorAgent.hasPath)
            actorAgent.SetDestination(transform.position);
    }

    public void ToggleAgent() => actorAgent.enabled = !actorAgent.enabled;

    #endregion
}