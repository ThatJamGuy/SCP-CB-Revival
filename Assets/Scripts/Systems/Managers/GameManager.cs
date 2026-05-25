using UnityEngine;
using EditorAttributes;

/// <summary>
/// Globally accessible script to handle most things related to the state of the game
/// </summary>
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [ReadOnly] public int currentDifficulty;
    [ReadOnly] public int currentZone;

    [HideInInspector] public SaveData currentSaveData;

    public void Awake() {
        // Ensure only one GameManager exists in the scene to prevent issues
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Set the current save data to the save.json file for future reference. Will later support multiple saves
        currentSaveData = DataSaver.Load<SaveData>("save.json");
        
        // Set some save data values to the ones in settings.json
        currentDifficulty = currentSaveData.difficulty;
        currentZone = currentSaveData.currentZone;
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
}