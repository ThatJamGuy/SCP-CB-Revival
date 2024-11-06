using UnityEngine;

[CreateAssetMenu(fileName = "NewFootstepData", menuName = "Footstep Data")]
public class FootstepData : ScriptableObject
{
    public Texture[] textures; // Array of textures
    public AudioClip[] walkingFootstepAudio;
    public AudioClip[] runningFootstepAudio;
}