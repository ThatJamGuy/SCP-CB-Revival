using EditorAttributes;
using FMODUnity;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Master script for puppeteering fellas on the field.
/// Yes this is an absolute mess but I have a habit of trying to future proof things.
/// </summary>
public class Actor_Generic : MonoBehaviour {

    // -----------------------------------------------------------------------------
    [TabGroup(nameof(animationSettings), nameof(references))]
    [SerializeField] private Void groupHolder;

    [VerticalGroup(nameof(useRootMotionWalking), nameof(rmotionWalkingBool), nameof(playAnimOnStart), nameof(randomAnimSpeedOnStart),
        nameof(randomAnimOnStart), nameof(startingAnimList), nameof(minAnimSpeed), nameof(maxAnimSpeed))]
    [SerializeField, HideInInspector] private Void animationSettings;

    [VerticalGroup(nameof(actorAnimator), nameof(actorAgent), nameof(voiceSource))]
    [SerializeField, HideInInspector] private Void references;

    // -----------------------------------------------------------------------------

    // General Settings

    // Animation Settings
    [SerializeField, HideProperty] private bool useRootMotionWalking;
    [SerializeField, HideProperty, ShowField(nameof(useRootMotionWalking))] private string rmotionWalkingBool = "walking_rmotion";
    [SerializeField, HideProperty] private bool playAnimOnStart;
    [SerializeField, HideProperty, ShowField(nameof(playAnimOnStart))] private bool randomAnimOnStart;
    [SerializeField, HideProperty, ShowField(nameof(playAnimOnStart))] private bool randomAnimSpeedOnStart;
    [SerializeField, HideProperty, ShowField(nameof(playAnimOnStart))] private string[] startingAnimList;
    [SerializeField, HideProperty, ShowField(nameof(randomAnimSpeedOnStart))] private int minAnimSpeed;
    [SerializeField, HideProperty, ShowField(nameof(randomAnimSpeedOnStart))] private int maxAnimSpeed;

    // AI Settings

    // References
    [SerializeField, HideProperty] private Animator actorAnimator;
    [SerializeField, HideProperty] private NavMeshAgent actorAgent;
    [SerializeField, HideProperty] private Transform voiceSource;

    #region Unity Callbacks

    private void Start() {
        if (useRootMotionWalking && rmotionWalkingBool != null) {
            actorAgent.updatePosition = false;
            actorAgent.updateRotation = false;
        }

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
        // If root motion navigation is to be utilized then manage that stuff
        if (actorAnimator != null && actorAgent != null && rmotionWalkingBool != null && useRootMotionWalking) {
            var isMoving = actorAgent.remainingDistance > actorAgent.stoppingDistance;

            actorAnimator.SetBool(rmotionWalkingBool, isMoving);

            if (!isMoving) return;

            var direction = actorAgent.desiredVelocity.normalized;

            if (direction.sqrMagnitude > 0.01f) {
                var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }

            actorAgent.nextPosition = transform.position;
        }
    }

    private void LateUpdate() {
        // If root motion navigation is to be utilized then manage that stuff
        if (actorAnimator != null && actorAgent != null && rmotionWalkingBool != null && useRootMotionWalking) {
            var targetRotation = actorAnimator.rootRotation;
            actorAnimator.transform.rotation = targetRotation;
        }
    }

    private void OnAnimatorMove() {
        // If root motion navigation is to be utilized then manage that stuff
        if (actorAnimator != null && actorAgent != null && rmotionWalkingBool != null && useRootMotionWalking) {
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