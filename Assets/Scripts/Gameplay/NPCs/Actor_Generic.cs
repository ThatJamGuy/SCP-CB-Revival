using FMODUnity;
using UnityEngine;
using UnityEngine.AI;

public class Actor_Generic : MonoBehaviour {


    [Header("Local References")]
    [SerializeField] private Animator actorAnimator;
    [SerializeField] private NavMeshAgent actorAgent;
    [SerializeField] private Transform voiceSource;

    #region Public Methods

    public void PlayAnimation(string animationName) {
        actorAnimator.Play(animationName);
    }

    public void Speak(EventReference toSpeak) {
        AudioManager.PlayOneShot(toSpeak, voiceSource.position);
    }

    #endregion
}