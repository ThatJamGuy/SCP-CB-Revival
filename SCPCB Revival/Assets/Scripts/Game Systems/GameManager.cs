using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Global Values")]
    public bool disablePlayerInputs;

    [Header("Controls")]
    [SerializeField] private KeyCode pauseGameKey = KeyCode.Escape;

    [Header("Music")]
    [SerializeField] private AudioClip zone1Music;

    [Header("References")]
    [SerializeField] private PauseMenu pauseMenu;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (MusicPlayer.Instance != null)
        {
            MusicPlayer.Instance.ChangeMusic(zone1Music);
        }
        else
        {
            Debug.LogWarning("MusicPlayer instance is not found!");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseGameKey))
        {
            pauseMenu.TogglePauseMenu();
        }
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

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}