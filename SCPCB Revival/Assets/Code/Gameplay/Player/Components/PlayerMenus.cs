using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMenus : MonoBehaviour {
    [Header("Input Actions")]
    public InputActionAsset playerControls;

    [Header("References")]
    [SerializeField] private PlayerBase playerBase;
    [SerializeField] private GameObject inventoryScreen;

    private InputAction inventoryAction;

    #region Enable/Disable
    private void OnEnable() {
        playerControls.Enable();
        inventoryAction = playerControls.FindAction("Inventory", true);
        inventoryAction.performed += ToggleInventory;
    }

    private void OnDisable() {
        if (inventoryAction != null) {
            inventoryAction.performed -= ToggleInventory;
        }
        playerControls.Disable();
    }
    #endregion

    #region Public Methods
    public void ToggleInventory(InputAction.CallbackContext context) {
        playerBase.TogglePlayerInputs();
        inventoryScreen.SetActive(!inventoryScreen.activeSelf);

        if (GameManager.Instance.inventoryPausesGame)
            GameManager.Instance.PauseGame();
    }
    #endregion
}