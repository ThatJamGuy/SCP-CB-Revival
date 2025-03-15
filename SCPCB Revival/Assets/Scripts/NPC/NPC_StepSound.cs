using UnityEngine;

public class NPC_StepSound : MonoBehaviour
{
    [SerializeField] private bool enableStepSounds;
    [SerializeField] private AudioClip[] stepSounds;
    [SerializeField] private AudioSource audioSource;

    public void StepSound()
    {
        if (enableStepSounds)
        {
            int randomIndex = Random.Range(0, stepSounds.Length);
            audioSource.clip = stepSounds[randomIndex];
            audioSource.Play();
        }
    }
}