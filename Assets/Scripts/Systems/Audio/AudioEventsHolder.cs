using UnityEngine;
using FMODUnity;

public class AudioEventsHolder : MonoBehaviour {
    public static AudioEventsHolder Instance { get; private set; }

    [Header("Player Sounds")] 
    public EventReference crouchFoley;

    [Header("UI Sounds")] 
    public EventReference quicksave01;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}