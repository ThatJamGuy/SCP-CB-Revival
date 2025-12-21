using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance { get; private set; }

    public bool isGamePaused = false;
    public bool scp106Active = false;
    public bool scp173ChasingPlayer = false;
    public bool scp173currentVisibleToPlayer = false;

    public void Awake() {
        instance = this;
    }

    private void Start() {
        
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

    public void ShowDeathScreen(string causeOfDeath) {
        PlayerAccessor.instance.DisablePlayerInputs(true);
        PlayerAccessor.instance.isDead = true;
        PlayerAccessor.instance.isMoving = false;
        AudioManager.instance.StopMusic();
        CanvasInstance.instance.deathMenu.SetActive(true);
        CanvasInstance.instance.deathMenuDeathCauseText.text = causeOfDeath;
    }
}