using FMODUnity;
using UnityEngine;

public class NPC_StepSound : MonoBehaviour {
    [SerializeField] private EventReference stepSoundSet;

    /// <summary>
    /// Plays a random step sound at the NPC's position based on whatever I set in it's event reference thing
    /// </summary>
    public void Step() {
        AudioManager.instance.PlaySound(stepSoundSet, transform.position);
    }
}