using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMenus : MonoBehaviour {
    [Header("Input Actions")]
    public InputActionAsset playerControls;

    [Header("References")]
    [SerializeField] private PlayerBase playerBase;
    [SerializeField] private GameObject inventoryScreen;
    [SerializeField] private GameObject pauseMenuScreen;

    private InputAction inventoryAction, pauseAction;

    private bool inventoryOpen, pauseOpen = false;

    #region Enable/Disable
    private void OnEnable() {
        playerControls.Enable();
        inventoryAction = playerControls.FindAction("Inventory", true);
        pauseAction = playerControls.FindAction("Pause", true);
        inventoryAction.performed += ToggleInventory;
        pauseAction.performed += TogglePauseMenu;
    }

    private void OnDisable() {
        inventoryAction.performed -= ToggleInventory;
        pauseAction.performed -= TogglePauseMenu;
        playerControls.Disable();
    }
    #endregion

    #region Public Methods
    public void ToggleInventory(InputAction.CallbackContext context) {
        if (pauseOpen) return;

        inventoryOpen = !inventoryOpen;

        playerBase.TogglePlayerInputs();
        inventoryScreen.SetActive(!inventoryScreen.activeSelf);

        if (GameManager.Instance.menusPauseGame)
            GameManager.Instance.PauseGameToggle();
    }

    public void TogglePauseMenu(InputAction.CallbackContext context) {
        if (inventoryOpen) return;

        pauseOpen = !pauseOpen;

        playerBase.TogglePlayerInputs();
        pauseMenuScreen.SetActive(!pauseMenuScreen.activeSelf);

        if (GameManager.Instance.menusPauseGame)
            GameManager.Instance.PauseGameToggle();
    }

    public void ResumeGame() {
        if (pauseOpen)
            TogglePauseMenu(default);
    }
    #endregion
}