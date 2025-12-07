using UnityEngine;

public class IngameMenuManager : MonoBehaviour {
    [SerializeField] private bool menusPauseGame = true;
    [SerializeField] private GameObject[] menus;

    private bool anyMenuAlreadyOpen = false;
    private int openMenuID = -1;

    private void Update() {
        if (InputManager.Instance != null && InputManager.Instance.inventoryAction.triggered) {
            ToggleMenuByID(0);
        }
        if (InputManager.Instance != null && InputManager.Instance.consoleAction.triggered) {
            ToggleMenuByID(1);
            DevConsole.Instance.SelectInputField();
        }
    }

    public void ToggleMenuByID(int menuID) {
        if (menus == null || menuID < 0 || menuID >= menus.Length) return;

        if (anyMenuAlreadyOpen && openMenuID != menuID) return;

        bool isMenuActive = menus[menuID].activeSelf;
        menus[menuID].SetActive(!isMenuActive);

        anyMenuAlreadyOpen = !isMenuActive;
        openMenuID = anyMenuAlreadyOpen ? menuID : -1;

        PlayerAccessor.instance.TogglePlayerInputs(!isMenuActive);

        if (menusPauseGame && GameManager.instance != null) {
            if (anyMenuAlreadyOpen) {
                GameManager.instance.PauseGame();
            }
            else {
                GameManager.instance.UnpauseGame();
                if (PixeLadder.SimpleTooltip.TooltipManager.Instance != null)
                    PixeLadder.SimpleTooltip.TooltipManager.Instance.HideTooltip();
            }
        }
    }
}