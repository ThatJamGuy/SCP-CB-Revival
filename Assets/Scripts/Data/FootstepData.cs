using UnityEngine;
using FMODUnity;
using EditorAttributes;

[CreateAssetMenu(fileName = "NewFootstepData", menuName = "SCPCBR/Footstep Data")]
public class FootstepData : ScriptableObject {
    [field: SerializeField] public EventReference assocatedWalkEvent { get; private set; }
    [field: SerializeField] public EventReference assocatedRunEvent { get; private set; }
    [TagDropdown] public string surfaceTag;
}