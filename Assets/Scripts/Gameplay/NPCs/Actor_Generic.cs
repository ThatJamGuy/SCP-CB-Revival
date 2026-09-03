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
    [SerializeField, ShowField(nameof(randomAnimSpeedOnStart))] private float minAnimSpeed;
    [SerializeField, ShowField(nameof(randomAnimSpeedOnStart))] private float maxAnimSpeed;

    [Header("References")]
    [SerializeField] private Animator actorAnimator;
    [SerializeField] private NavMeshAgent actorAgent;
    [SerializeField] private Transform voiceSource;

    #region Unity Lifecycle

    private void Start() {
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

    public void SetAnimTrigger(string animTriggerName) {
        if (actorAnimator == null) {
            Debug.Log("<color=red>[Actor_Generic]</color> The actorAnimator reference of this actor was left null, animation related tasks will not work.");
            return;
        }

        actorAnimator.speed = 1;
        actorAnimator.SetTrigger(animTriggerName);
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