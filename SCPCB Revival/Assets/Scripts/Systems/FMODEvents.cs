using FMODUnity;
using UnityEngine;

public class FMODEvents : MonoBehaviour {
    [field: Header("Music")]
    [field: SerializeField] public EventReference musicRevival { get; private set; }

    [field: Header("Step Sounds")]
    [field: SerializeField] public EventReference stepConcreteWalk { get; private set; }

    [field: Header("Alarms")]
    [field: SerializeField] public EventReference alarm2 { get; private set; }

    [field: Header("Doors")]
    [field: SerializeField] public EventReference doorOpen173 { get; private set; }

    [field: Header("Horror")]
    [field: SerializeField] public EventReference statueHorrorFar { get; private set; }
    [field: SerializeField] public EventReference statueHorrorNear { get; private set; }

    [field: Header("Zone Ambience")]
    [field: SerializeField] public EventReference zone1Ambience { get; private set; }

    [field: Header("Room")]
    [field: SerializeField] public EventReference teslaShock { get; private set; }

    [field: Header("Events")]
    [field: SerializeField] public EventReference commotionSounds { get; private set; }

    [field: Header("Interact")]
    [field: SerializeField] public EventReference buttonPress { get; private set; }
    [field: SerializeField] public EventReference buttonPress2 { get; private set; }

    public static FMODEvents instance { get; private set; }

    private void Awake() {
        if (instance != null) {
            Debug.LogError("Found more than one FMOD Events instance in the scene.");
        }
        instance = this;
    }
}