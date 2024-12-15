using UnityEngine;

[CreateAssetMenu(fileName = "NewFootstepData", menuName = "Footstep Data")]
public class FootstepData : ScriptableObject
{
    public Texture[] textures;
    public AudioClip[] walkingFootstepAudio;
    public AudioClip[] runningFootstepAudio;
}