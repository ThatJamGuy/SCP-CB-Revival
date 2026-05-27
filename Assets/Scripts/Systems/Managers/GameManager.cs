using System;
using System.Globalization;
using UnityEngine;
using EditorAttributes;
using UnityEngine.InputSystem;

/// <summary>
/// Globally accessible script to handle most things related to the state of the game
/// </summary>
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [Header("Main Game State")] 
    [ReadOnly] public bool lczLockdownLifted;
    
    [Header("Other Save States")]
    [ReadOnly] public int currentDifficulty;
    [ReadOnly] public int currentZone;

    [HideInInspector] public SaveData currentSaveData;
    
    private static readonly int Quicksave = Animator.StringToHash("Quicksave");

    private InputAction quicksaveAction;

    private void Awake() {
        // Ensure only one GameManager exists in the scene to prevent issues
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Set the current save data to the save.json file for future reference. Will later support multiple saves
        currentSaveData = DataSaver.Load<SaveData>("save.json");
        
        // Set some save data values to the ones in settings.json
        currentDifficulty = currentSaveData.difficulty;
        currentZone = currentSaveData.currentZone;
    }

    private void Start() {
        quicksaveAction = InputManager.Instance.GetAction("Player", "Quicksave");
    }

    private void Update() {
        // Check for quicksave action
        if (quicksaveAction.triggered) {
            SaveGame(true);
        }
    }

    public static void PauseGame() {
        // Set the game's timescale to 0 (Pausing Time.deltaTime) and pause FMOD via the AudioManager
        Time.timeScale = 0f;
        Player.SetCursorState(true);
        Player.Instance.disableInput = true;
        AudioManager.Instance.PauseAllSFX();
    }
    
    public static void ResumeGame() {
        // Set the game's timescale to 1 (Resuming Time.deltaTime to normal) and resume FMOD via the AudioManager
        Time.timeScale = 1f;
        Player.SetCursorState(false);
        Player.Instance.disableInput = false;
        AudioManager.Instance.ResumeAllSFX();
    }

    public void SaveGame(bool playSound = true) {
        CanvasInstance.Instance.HUD_QuickSave.SetTrigger(Quicksave);

        currentSaveData.currentDateTime = DateTime.Now.ToString(CultureInfo.CurrentCulture);
        currentSaveData.currentGameVersion = "v" + Application.version;
        currentSaveData.playerPos = Player.Instance.transform.position;
        currentSaveData.playerRot = Player.Instance.transform.rotation;
        currentSaveData.lczLockdownLifted = lczLockdownLifted;
        
        // List of things that should be saved on file for v0.0.6. Map seed & name are already saved on save creation
        //TODO: Save player inventory
        //TODO: Save player blink and stamina stats
        //TODO: Save SCP-173 location
        //TODO: Save SCP-173 state (Is he chasing the player right now?)
        //TODO: Save SCP-106 spawn counter
        //TODO: Save SCP-106 state (Is he chasing the player right now?)
        //TODO: Save SCP-106 location (IF HE IS CHASING THE PLAYER OR OTHER TARGET ONLY)
        //TODO: Save currently playing music (Even if a chase is happening, the triggers don't account for loading)
        //TODO: Save if LCZ has been lifted from lockdown (Find way to flip the lever properly on load???)
        //TODO: Save door states. Might put this one off for now, not as important to save open/close states (Idk how)
        
        DataSaver.Save(currentSaveData, "save.json");
        
        if (playSound) {
            AudioManager.PlayOneShot(AudioEventsHolder.Instance.quicksave01, Player.Instance.transform.position);
        }
    }
}