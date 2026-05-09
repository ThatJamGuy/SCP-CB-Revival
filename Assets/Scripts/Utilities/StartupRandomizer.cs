using System;
using UnityEngine;

/// <summary>
/// Script that can be attatched to various objects that allow for multiple possible starting values.
/// </summary>
public class StartupRandomizer : MonoBehaviour {
    private enum RandomizerType { RandomAnimatorSpeed, RandomAnimation }

    [Header("Randomizer Settings")]
    [SerializeField] private RandomizerType type;
    
    [Header("Random Animator Speed Settings")]
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 1.5f;
    
    [Header("Random Animation Settings")]
    public AnimationClip[] animations;
    public string[] animationNames;

    #region Unity Callbacks
    private void Start() {
        // Tell the script what to do based on the RandomizerType defined in the editor for the current object
        switch (type) {
            case RandomizerType.RandomAnimatorSpeed:
                RandomizeAnimatorSpeed();
                break;
            case RandomizerType.RandomAnimation:
                RandomizeAnimation();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    #endregion

    #region Private Methods
    private void RandomizeAnimatorSpeed() {
        // Grab the animator attached to this object
        var animator = GetComponent<Animator>();
        
        // If the animator doesn't exist then do nothing
        if (animator == null) return;
        
        // Set a random speed float value to a random number between the defined minSpeed and maxSpeed
        // Then set the speed of the animator to that random value
        var randomSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        animator.speed = randomSpeed;
    }

    private void RandomizeAnimation() {
        // Grab the animator attached to this object
        var animator = GetComponent<Animator>();
        
        // Play a random animation via direct .anim files in the animations array, assuming the animator exists
        if (animator != null && animations.Length > 0) {
            var randomAnimation = animations[UnityEngine.Random.Range(0, animations.Length)];
            animator.Play(randomAnimation.name);
            return;
        }
        
        // Play a random animation via animation names in the animations array, assuming the animator exists
        if (animator != null && animationNames.Length > 0) {
            var randomName = animationNames[UnityEngine.Random.Range(0, animationNames.Length)];
            animator.Play(randomName);
            return;
        }
    }
    #endregion
}