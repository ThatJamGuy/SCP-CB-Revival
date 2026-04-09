using UnityEngine;

public enum RandomizerType { RandomAnimatorSpeed, RandomAnimation }

public class StartupRandomizer : MonoBehaviour {
    public RandomizerType type;

    public float minSpeed = 0.5f;
    public float maxSpeed = 1.5f;

    public AnimationClip[] animations;
    public string[] animationNames;

    #region Default Methods
    private void Start() {
        switch (type) {
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
            return;
        }
        if (animator != null && animationNames.Length > 0) {
            string randomName = animationNames[Random.Range(0, animationNames.Length)];
            animator.Play(randomName);
            return;
        }
    }
    #endregion
}