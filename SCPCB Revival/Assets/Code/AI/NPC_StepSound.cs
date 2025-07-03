using UnityEngine;

public class NPC_StepSound : MonoBehaviour
{
    [SerializeField] private bool enableStepSounds = true;
    [SerializeField] private AudioClip[] stepSounds;
    [SerializeField] private AudioSource audioSource;

    public void Step()
    {
        if (enableStepSounds && stepSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, stepSounds.Length);
            audioSource.clip = stepSounds[randomIndex];
            audioSource.Play();
        }
    }
}