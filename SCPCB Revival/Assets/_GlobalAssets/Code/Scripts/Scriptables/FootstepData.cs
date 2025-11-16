using UnityEngine;
using FMOD.Studio;
using FMODUnity;

[CreateAssetMenu(fileName = "NewFootstepData", menuName = "SCPCBR/Footstep Data")]
public class FootstepData : ScriptableObject {
    [field: SerializeField] public EventReference assocatedWalkEvent { get; private set; }
    [field: SerializeField] public EventReference assocatedRunEvent { get; private set; }
    public Texture[] textures;
}