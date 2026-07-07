using UnityEngine;

public class Actor_Guard : MonoBehaviour {
    private enum State {
        Nothing = 0,
        Animating = 1,
        Navigating = 2
    }

    [SerializeField] private State currentState = State.Nothing;

    [Header("Generic Settings")]
    [SerializeField] private string actorName;
    [SerializeField] private int test;

    [Header("Animation Settings")]
    [SerializeField] private string initialAnimationName;

    [Header("References")]
    public Transform voiceSource;

    [SerializeField] private Animator animator;

    #region Unity Callbacks

    private void Start() {
        if (initialAnimationName != null) animator.Play(initialAnimationName);
    }

    private void Update() {

    }

    #endregion
}