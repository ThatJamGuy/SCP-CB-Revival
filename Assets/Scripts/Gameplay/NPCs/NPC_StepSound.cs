using FMODUnity;
using UnityEngine;

/// <summary>
/// Really easy to use script for NPC step sounds, just attach it to an NPC with an animator and call Step() on anims
/// </summary>
public class NPC_StepSound : MonoBehaviour {
    [SerializeField] private EventReference stepSoundSet;
    
    public void Step() {
        AudioManager.PlayOneShot(stepSoundSet, transform.position);
    }
}