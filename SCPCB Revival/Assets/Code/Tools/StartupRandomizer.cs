using UnityEngine;
using NaughtyAttributes;

public enum RandomizerType { RandomAudio, RandomAnimatorSpeed, RandomAnimation }

public class StartupRandomizer : MonoBehaviour {
    public RandomizerType type;

    [ShowIf("type", RandomizerType.RandomAudio)] public AudioClip[] audioClips;

    [ShowIf("type", RandomizerType.RandomAnimatorSpeed)] public float minSpeed = 0.5f;
    [ShowIf("type", RandomizerType.RandomAnimatorSpeed)] public float maxSpeed = 1.5f;

    [ShowIf("type", RandomizerType.RandomAnimation)] public AnimationClip[] animations;

    #region Default Methods
    private void Start() {
        switch (type) {
            case RandomizerType.RandomAudio:
                RandomizeAudio();
                break;
            case RandomizerType.RandomAnimatorSpeed:
                RandomizeAnimatorSpeed();
                break;
            case RandomizerType.RandomAnimation:
                RandomizeAnimation();
                break;
        }
    }
    #endregion

    #region Private Methods
    private void RandomizeAudio() { 
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioClips.Length > 0) {
            AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];
            audioSource.clip = randomClip;
            audioSource.Play();
        }
    }

    private void RandomizeAnimatorSpeed() {
        Animator animator = GetComponent<Animator>();
        if (animator != null) {
            float randomSpeed = Random.Range(minSpeed, maxSpeed);
            animator.speed = randomSpeed;
        }
    }

    private void RandomizeAnimation() {
        Animator animator = GetComponent<Animator>();
        if (animator != null && animations.Length > 0) {
            AnimationClip randomAnimation = animations[Random.Range(0, animations.Length)];
            animator.Play(randomAnimation.name);
        }
    }
    #endregion
}