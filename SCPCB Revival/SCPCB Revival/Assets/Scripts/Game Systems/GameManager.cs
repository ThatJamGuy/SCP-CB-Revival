using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Global Values")]
    public bool disablePlayerInputs;
    public bool inventoryPausesGame;
    public bool skipIntro;
    public bool playerIsDead = false;

    [Header("Controls")]
    public KeyCode noclipUp = KeyCode.E;
    public KeyCode noclipDown = KeyCode.LeftControl;

    [Header("Music")]
    public AudioClip introMusic;
    public AudioClip scp173ChamberMusic;
    public AudioClip zone1Music;
    public AudioClip scp173Music;

    [Header("References")]
    [SerializeField] private EVNT_Intro introEventScript;

    private bool isNewGame = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (GameSettings.Instance != null)
        {
            skipIntro = GameSettings.Instance.skipIntro;

            if (isNewGame && MusicPlayer.Instance != null)
            {
                if (skipIntro)
                {
                    ChangeMusic(scp173ChamberMusic);
                    introEventScript.SkipIntro();
                }
                else
                {
                    ChangeMusic(introMusic);
                }
            }
        }
        else
        {
            ChangeMusic(introMusic);
        }
    }

    public void ChangeMusic(AudioClip music)
    {
        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.ChangeMusic(music);
    }

    public void PauseGame()
    {
        bool isPaused = Time.timeScale == 0.0f;

        AudioListener.pause = !isPaused;
        Time.timeScale = isPaused ? 1.0f : 0.0f;
    }

    public void TogglePlayerInput(bool alsoToggleMouse)
    {
        disablePlayerInputs = !disablePlayerInputs;

        if (alsoToggleMouse)
        {
            if (disablePlayerInputs)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    public void KillPlayer()
    {
        playerIsDead = true;
        MenuManager.Instance.ToggleMenu(1);
    }
}