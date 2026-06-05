using EditorAttributes;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Generic NPC actor class for scripted events
/// Handles animation, ai navigation, and interactions
/// </summary>
public class NPC_Actor : MonoBehaviour {
    #region Editor Organization
    [Title("<b>NPC ACTOR</b>", 40, lineThickness:10f, alignment:TextAnchor.MiddleCenter)]
    [TabGroup(nameof(settings), nameof(headIK), nameof(audioControls), nameof(animControls))]
    [SerializeField] private Void groupHolder;

    [VerticalGroup(nameof(useNavigationAgent), nameof(useHeadIk), nameof(useSpeech), nameof(advancedAnimation), nameof(useRootMotionAgent), nameof(useStepSounds))]
    [SerializeField, HideInInspector] private Void settings;

    [VerticalGroup(nameof(headIKTarget), nameof(headRig), nameof(lookBehindCompensation), nameof(Radius), nameof(retargetSpeed), nameof(maxAngle), nameof(PointsOfInterest))]
    [SerializeField, HideInInspector] private Void headIK;

    [VerticalGroup(nameof(allowIdleChatter), nameof(speechAudioSource), nameof(stepAudioSource), nameof(scriptedVoiceLines), nameof(idleVoiceLines), nameof(stepSounds))]
    [SerializeField, HideInInspector] private Void audioControls;

    [VerticalGroup(nameof(animator), nameof(startingAnimation), nameof(availableStartAnims), nameof(randomStartingAnim))]
    [SerializeField, HideInInspector] private Void animControls;
    #endregion

    #region Editor Variables
    [Title("Actor Settings")]
    [SerializeField, HideProperty] private bool useNavigationAgent = false;
    [SerializeField, HideProperty] private bool useHeadIk = false;
    [SerializeField, HideProperty] private bool useSpeech = false;
    [SerializeField, HideProperty] private bool advancedAnimation = false;
    [SerializeField, HideProperty] private bool useRootMotionAgent = false;
    [SerializeField, HideProperty] private bool useStepSounds = false;

    [SerializeField, HelpBox("Not required, but necessary for basic movement and root motion based movement.", MessageMode.None)]
    private NavMeshAgent navAgent;

    [Title("Head IK")]
    [SerializeField, HideProperty] private Transform headIKTarget;
    [SerializeField, HideProperty] private Rig headRig;
    [SerializeField, HideProperty] private bool lookBehindCompensation = false;
    [SerializeField, HideProperty] private float Radius = 10f;
    [SerializeField, HideProperty] private float retargetSpeed = 5f;
    [SerializeField, HideProperty] private float maxAngle = 85f;
    [HideProperty] public List<IK_PointOfInterest> PointsOfInterest;

    [Title("Audio")]
    [SerializeField, HideProperty] private bool allowIdleChatter;
    [SerializeField, HideProperty] private AudioSource speechAudioSource;
    [SerializeField, HideProperty] private AudioSource stepAudioSource;
    [SerializeField, HideProperty] private AudioClip[] scriptedVoiceLines;
    [SerializeField, HideProperty] private AudioClip[] idleVoiceLines;
    [SerializeField, HideProperty] private AudioClip[] stepSounds;

    [Title("Animation")]
    [SerializeField, HideProperty] private Animator animator;
    [SerializeField, HideProperty] private AnimationClip startingAnimation;
    [SerializeField, HideProperty] private AnimationClip[] availableStartAnims;
    [SerializeField, HideProperty] private bool randomStartingAnim;

    [Button("Look At Player", 20f)]
    public void TallButton() => Debug_LookAtPlayer();
    #endregion

    #region Private Variables
    private Transform manualLookTarget;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private float minDeltaTime = 0.0001f;
    private float RadiusSqr;
    #endregion

    #region Default Methods
    private void Start() {
        // Configure private stuff based on settings
        if (navAgent != null) navAgent.enabled = useNavigationAgent;
        //if (animator != null) animator.enabled = advancedAnimation;
        if (speechAudioSource != null) speechAudioSource.enabled = useSpeech;
        if (stepAudioSource != null) stepAudioSource.enabled = useStepSounds;

        RadiusSqr = Radius * Radius;

        if (useRootMotionAgent) {
            navAgent.updatePosition = false;
            navAgent.updateRotation = false;
        }

        if (useSpeech && allowIdleChatter) StartCoroutine(IdleChatter());

        if (advancedAnimation) {
            if(startingAnimation != null) animator.Play(startingAnimation.name);
            if (availableStartAnims != null) {
                AnimationClip clip = availableStartAnims[Random.Range(0, availableStartAnims.Length)];
                animator.Play(clip.name);
            }
        }
    }

    private void Update() {
        #region Root Motion
        if (useRootMotionAgent) {
            if (Time.timeScale < minDeltaTime) return;

            bool isWalking = IsAgentMoving();
            animator.SetBool(IsMoving, isWalking);

            if (isWalking) {
                RotateToward(navAgent.desiredVelocity.normalized);
            }
        }
        #endregion

        #region Head IK
        if (!useHeadIk) return;

        Transform tracking = manualLookTarget;
        if (tracking == null && PointsOfInterest != null) {
            foreach (var poi in PointsOfInterest) {
                Vector3 delta = poi.transform.position - transform.position;
                if (delta.sqrMagnitude > RadiusSqr) continue;
                float angle = Vector3.Angle(transform.forward, delta);
                if (angle < maxAngle || (lookBehindCompensation && angle < 160f)) {
                    tracking = poi.transform;
                    break;
                }
            }
        }

        Vector3 targetPos = transform.position + transform.forward * 2f;
        float rigWeight = 0f;

        if (tracking != null) {
            Vector3 dir = tracking.position - transform.position;
            float angle = Vector3.Angle(transform.forward, dir);

            if (angle > maxAngle && lookBehindCompensation) {
                Vector3 cross = Vector3.Cross(transform.forward, dir);
                Vector3 sideDir = Quaternion.AngleAxis(cross.y > 0 ? maxAngle : -maxAngle, transform.up) * transform.forward;
                targetPos = transform.position + sideDir * 2f + transform.up * 1.5f;
            }
            else targetPos = tracking.position;

            rigWeight = 1f;
        }

        headIKTarget.position = Vector3.Lerp(headIKTarget.position, targetPos, Time.deltaTime * retargetSpeed);
        headRig.weight = Mathf.Lerp(headRig.weight, rigWeight, Time.deltaTime * 2f);
        #endregion
    }

    private void LateUpdate() {
        if (useRootMotionAgent) {
            if (Time.timeScale < minDeltaTime) return;
            transform.rotation = animator.rootRotation;
        }
    }

    private void OnAnimatorMove() {
        if (useRootMotionAgent) {
            if (Time.timeScale < minDeltaTime) return;

            Vector3 delta = animator.deltaPosition;
            float deltaTime = Mathf.Max(Time.deltaTime, minDeltaTime);

            transform.position += delta;
            navAgent.nextPosition = transform.position;

            if (IsAgentMoving()) {
                navAgent.velocity = delta / deltaTime;
            }
            else {
                navAgent.velocity = Vector3.zero;
            }
        }
    }
    #endregion

    #region Public Methods
    // Prioritize looking at the transform instead of available POIs if any
    public void IKLookAt(Transform t) => manualLookTarget = t;

    // If IKLookAt was used, stop looking at that and return to POIs
    public void IKLookAway() => manualLookTarget = null;

    // Speak a pre-defined voice line by its index in the array
    public void Speak(int lineID) {
        if (!useSpeech) return;
        speechAudioSource.clip = scriptedVoiceLines[lineID];
        speechAudioSource.Play();
    }

    // Play an animation via trigger (Meant to be one way)
    public void PlayAnimationByTrigger(string triggerName) {
        if (!advancedAnimation) return;
        animator.SetTrigger(triggerName);
    }

    // Walk to a specific position using the nav mesh agent. FORWARD SUPPORT ONLY RIGHT NOW!!!
    public void WalkToPosition(Vector3 destination) {
        if (!useNavigationAgent) return;
        navAgent.SetDestination(destination);
    }

    // Toggle the navigation agent. Useful for switching to root motion animations mid-game or general use.
    public void ToggleAgent() => navAgent.enabled = !navAgent.enabled;

    // Play a random step sound. Meant to be called via animation event.
    public void Step() {
        if (!useStepSounds) return;
        stepAudioSource.clip = stepSounds[Random.Range(0, stepSounds.Length)];
        stepAudioSource.Play();
    }
    #endregion

    #region Private Methods
    // For the root motion stuff
    private void RotateToward(Vector3 direction) {
        if (direction.sqrMagnitude < 0.01f) return;
        Quaternion target = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 10f);
    }
    #endregion

    #region Debug Methods
    [ContextMenu("Test Look At Player")]
    private void Debug_LookAtPlayer() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) IKLookAt(player.transform);
    }
    #endregion

    #region Coroutines
    private IEnumerator IdleChatter() {
        while (allowIdleChatter) {
            yield return new WaitForSeconds(Random.Range(10f, 30f));
            speechAudioSource.clip = idleVoiceLines[Random.Range(0, idleVoiceLines.Length)];
            speechAudioSource.Play();
        }
    }
    #endregion

    #region Advanced Variables
    private bool IsAgentMoving() {
        return navAgent.enabled && !navAgent.pathPending && navAgent.remainingDistance > navAgent.stoppingDistance;
    }
    #endregion
}