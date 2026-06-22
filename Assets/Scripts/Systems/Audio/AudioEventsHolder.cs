using UnityEngine;
using FMODUnity;

public class AudioEventsHolder : MonoBehaviour {
    public static AudioEventsHolder Instance { get; private set; }

    public EventReference doorExplode;

    [Header("Player Sounds")] 
    public EventReference crouchFoley;

    [Header("UI Sounds")] 
    public EventReference quicksave01;

    [Header("NPC Sounds")]
    public EventReference scp096Triggered;
    public EventReference scp096ChargeUp;
    public EventReference scp096KillPlayer;
    public EventReference doorOpen173;
    public EventReference statueHorrorNear;
    public EventReference statueHorrorFar;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}