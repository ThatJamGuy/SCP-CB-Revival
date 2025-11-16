using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FMODEvents : MonoBehaviour {
    [field: Header("Music")]
    [field: SerializeField] public EventReference musicRevival { get; private set; }

    [field: Header("Step Sounds")]
    [field: SerializeField] public EventReference stepConcreteWalk { get; private set; }

    [field: Header("Alarms")]
    [field: SerializeField] public EventReference alarm2 { get; private set; }

    [field: Header("Zone Ambience")]
    [field: SerializeField] public EventReference zone1Ambience { get; private set; }

    [field: Header("Events")]
    [field: SerializeField] public EventReference commotionSounds { get; private set; }

    public static FMODEvents instance { get; private set; }

    private void Awake() {
        if (instance != null) {
            Debug.LogError("Found more than one FMOD Events instance in the scene.");
        }
        instance = this;
    }
}