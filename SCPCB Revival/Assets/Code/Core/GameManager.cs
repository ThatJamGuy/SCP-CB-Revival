using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Global Values")]
    public bool disablePlayerInputs;
    public bool inventoryPausesGame;

    [Header("Player References")]
    public GameObject playerPrefab;

    [Header("InGame Menus")]
    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private GameObject pauseMenu;

    private bool isInventoryOpen = false;

    private InputAction inventoryAction;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() {
        // Just for now
        //PlacePlayerInWorld(new Vector3(7.3f, 1, 0));
    }

    private void Update() {
        if (inventoryAction.WasPressedThisFrame()) ToggleInventory();
    }

    public void PlacePlayerInWorld(Vector3 spawnPos) {
        if (playerPrefab == null) return;
        Instance.playerPrefab = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }

    public void PauseGame() {
        bool isPaused = Time.timeScale == 0.0f;

        AudioListener.pause = !isPaused;
        Time.timeScale = isPaused ? 1.0f : 0.0f;
    }

    public void ToggleInventory() {
        if (inventoryPausesGame) PauseGame();
        TogglePlayerInput(true);

        isInventoryOpen = !isInventoryOpen;
        inventoryMenu.SetActive(isInventoryOpen);
    }

    public void TogglePlayerInput(bool alsoToggleMouse) {
        disablePlayerInputs = !disablePlayerInputs;

        if (alsoToggleMouse) {
            if (disablePlayerInputs) {
                UpdateCursorState();
            }
            else {
                UpdateCursorState();
            }
        }
    }

    public void UpdateCursorState() {
        Cursor.lockState = disablePlayerInputs ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = disablePlayerInputs;
    }
}