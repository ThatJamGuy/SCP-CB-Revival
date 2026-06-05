using UnityEngine;
using FMODUnity;
using EditorAttributes;

[CreateAssetMenu(fileName = "FootstepData", menuName = "SCP:CBR/Footstep Data")]
public class FootstepData : ScriptableObject {
    [field: SerializeField] public EventReference associatedWalkEvent { get; private set; }
    [field: SerializeField] public EventReference associatedRunEvent { get; private set; }
    [TagDropdown] public string surfaceTag;
}