using EditorAttributes;
using FMODUnity;
using UnityEngine;
using UnityEngine.AI;

public class Actor_Generic : MonoBehaviour {
    [Header("Animation Settings")]
    [SerializeField] private bool playAnimOnStart;
    [SerializeField, ShowField(nameof(playAnimOnStart))] private bool randomAnimOnStart;
    [SerializeField, ShowField(nameof(playAnimOnStart))] private bool randomAnimSpeedOnStart;
    [SerializeField, ShowField(nameof(playAnimOnStart))] private string[] startingAnimList;
    [SerializeField, ShowField(nameof(playAnimOnStart))] private bool startWithRootMotion;
    [SerializeField, ShowField(nameof(randomAnimSpeedOnStart))] private float minAnimSpeed;
    [SerializeField, ShowField(nameof(randomAnimSpeedOnStart))] private float maxAnimSpeed;

    [Header("References")]
    [SerializeField] private NPC_Locomotion locomotion;
    [SerializeField] private Animator actorAnimator;
    public NavMeshAgent actorAgent;
    [SerializeField] private Transform voiceSource;

    #region Unity Lifecycle

    private void Start() {
        if (!playAnimOnStart) return;

        if (randomAnimSpeedOnStart) {
            actorAnimator.speed = Random.Range(minAnimSpeed, maxAnimSpeed);
        }

        string animName = randomAnimOnStart
            ? startingAnimList[Random.Range(0, startingAnimList.Length)]
            : startingAnimList[0];

        PlayAnimation(animName, startWithRootMotion);
    }

    #endregion

    private void ApplyRootMotion(bool enabled) {
        if (locomotion != null) locomotion.SetScriptedRootMotion(enabled);
    }

    #region Public Methods

    public void PlayAnimation(string animationName, bool rootMotion = false) {
        if (actorAnimator == null) {
            Debug.Log("<color=red>[Actor_Generic]</color> The actorAnimator reference of this actor was left null, animation related tasks will not work.");
            return;
        }

        ApplyRootMotion(rootMotion);
        actorAnimator.speed = 1;
        actorAnimator.Play(animationName);
    }


    public void SetAnimTrigger(string animTriggerName, bool rootMotion = false) {
        if (actorAnimator == null) {
            Debug.Log("<color=red>[Actor_Generic]</color> The actorAnimator reference of this actor was left null, animation related tasks will not work.");
            return;
        }

        ApplyRootMotion(rootMotion);
        actorAnimator.speed = 1;
        actorAnimator.SetTrigger(animTriggerName);
    }

    public void SetAnimBool(string boolName, bool value, bool rootMotion = false) {
        if (actorAnimator == null) {
            Debug.Log("<color=red>[Actor_Generic]</color> The actorAnimator reference of this actor was left null, animation related tasks will not work.");
            return;
        }

        ApplyRootMotion(rootMotion);
        actorAnimator.speed = 1;
        actorAnimator.SetBool(boolName, value);
    }

    public void Speak(EventReference toSpeak) {
        AudioManager.PlayOneShot(toSpeak, voiceSource.position);
    }

    public void SpeakWithParameter(EventReference toSpeak, string paramName, float paramValue) {
        AudioManager.PlayOneShot(toSpeak, voiceSource.position, paramName, paramValue);
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
        ApplyRootMotion(false);

        if (actorAgent != null && actorAgent.hasPath)
            actorAgent.SetDestination(transform.position);
    }

    public void ToggleAgent() => actorAgent.enabled = !actorAgent.enabled;

    #endregion
}