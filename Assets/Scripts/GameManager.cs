using UnityEngine;

/// <summary>
/// Globally accessible script to handle most things related to the state of the game
/// </summary>
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    //public static SaveData currentSaveData { get; private set; }

    //[Header("Main Game State")]
    //[ReadOnly] public bool lczLockdownLifted;

    //TODO: IMPLEMENT PRIORITY SYSTEM INSTEAD OF THIS NONSENSE
    // IE. Assign scp-106 a value of 5, 173 is 1, 049 is 2, etc. (Either in respective npc code, EntityManager, etc.)
    // When a music call is made compare the requested priority to the current one
    // If higher play it, if lower dont, if the same then play since none are more important than the other

    //[Header("SCP States")]
    //[ReadOnly] public bool playerNear096;
    //[ReadOnly] public bool scp049pursuing;
    //[ReadOnly] public bool scp096pursuing;
    //[ReadOnly] public bool scp106pursuing;
    //[ReadOnly] public bool scp173pursuing;

    //[Header("Other Save States")]
    //[ReadOnly] public int currentDifficulty;
    //[ReadOnly] public int otherDifficultyFactor;

    //private static readonly int Quicksave = Animator.StringToHash("Quicksave");
    //public static int currentZone = 1;

    //private InputAction quicksaveAction;

    private void Awake() {
        // Ensure only one GameManager exists in the scene to prevent issues
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Set the current save data to the save.json file for future reference. Will later support multiple saves
        //currentSaveData = DataSaver.Load<SaveData>("save.json");

        // Set some save data values to the ones in settings.json
        //currentDifficulty = currentSaveData.difficulty;
        //currentZone = currentSaveData.currentZone;
    }

    //private void Start() {
    //    quicksaveAction = InputManager.Instance.GetAction("Player", "Quicksave");
    //}
}