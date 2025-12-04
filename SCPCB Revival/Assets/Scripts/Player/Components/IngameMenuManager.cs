using UnityEngine;

public class IngameMenuManager : MonoBehaviour {
    [SerializeField] private bool menusPauseGame = true;
    [SerializeField] private GameObject[] menus;

    private bool anyMenuAlreadyOpen = false;

    private void Update() {
        if (InputManager.Instance != null && InputManager.Instance.inventoryAction.triggered) {
            ToggleMenuByID(0);
        }
    }

    public void ToggleMenuByID(int menuID) {
        if (menus == null || menuID < 0 || menuID >= menus.Length) return;

        bool isMenuActive = menus[menuID].activeSelf;
        menus[menuID].SetActive(!isMenuActive);
        anyMenuAlreadyOpen = !isMenuActive;
        PlayerAccessor.instance.TogglePlayerInputs(!isMenuActive);

        if (menusPauseGame && GameManager.instance != null) {
            if (anyMenuAlreadyOpen) {
                GameManager.instance.PauseGame();
            } else {
                GameManager.instance.UnpauseGame();
                if (PixeLadder.SimpleTooltip.TooltipManager.Instance != null)
                    PixeLadder.SimpleTooltip.TooltipManager.Instance.HideTooltip();
            }
        }
    }
}