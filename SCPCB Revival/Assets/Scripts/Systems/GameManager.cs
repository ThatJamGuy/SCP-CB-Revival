using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance { get; private set; }

    public bool isGamePaused = false;

    public void Awake() {
        instance = this;
    }

    public void PauseGame() {
        isGamePaused = true;
        Time.timeScale = 0f;
        AudioManager.instance.PauseGameAudio();
    }

    public void UnpauseGame() {
        isGamePaused = false;
        Time.timeScale = 1f;
        AudioManager.instance.UnpauseGameAudio();
    }
}