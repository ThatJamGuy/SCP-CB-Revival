using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Global Values")]
    public bool disablePlayerInputs;
    public bool inventoryPausesGame;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ChangeMusic(string trackName) {
        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.ChangeMusic(trackName);
    }

    public void PauseGame() {
        bool isPaused = Time.timeScale == 0.0f;

        AudioListener.pause = !isPaused;
        Time.timeScale = isPaused ? 1.0f : 0.0f;
    }

    public void TogglePlayerInput(bool alsoToggleMouse) {
        disablePlayerInputs = !disablePlayerInputs;

        if (alsoToggleMouse)
        {
            if (disablePlayerInputs)
            {
                UpdateCursorState();
            }
            else
            {
                UpdateCursorState();
            }
        }
    }

    public void UpdateCursorState() {
        Cursor.lockState = disablePlayerInputs ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = disablePlayerInputs;
    }
}