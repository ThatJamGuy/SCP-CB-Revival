using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Global Values")]
    public bool disablePlayerInputs;
    public bool inventoryPausesGame;

    [Header("Controls")]
    [SerializeField] private KeyCode pauseGameKey = KeyCode.Escape;
    [SerializeField] private KeyCode openInventoryKey = KeyCode.Tab;

    [Header("Music")]
    public AudioClip zone1Music;
    public AudioClip scp173Music;

    [Header("References")]
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private InventoryScreen inventoryScreen;
    [SerializeField] private DeathScreen deathScreen;

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
            //Debug.LogWarning("MusicPlayer instance is not found!");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseGameKey))
        {
            if (inventoryScreen.isOpen)
                return;

            pauseMenu.TogglePauseMenu();
        }

        if (Input.GetKeyDown(openInventoryKey))
        {
            if (pauseMenu.isOpen)
                return;

            inventoryScreen.ToggleInventory();
        }
    }

    public void PauseGame()
    {
        bool isPaused = Time.timeScale == 0.0f;

        AudioListener.pause = !isPaused;
        Time.timeScale = isPaused ? 1.0f : 0.0f;
    }

    public void ShowDeathScreen()
    {
        deathScreen.ToggleDeathMenu();
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
}