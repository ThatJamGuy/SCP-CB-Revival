using PixeLadder.EasyTooltip;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour {
    public static MenuManager Instance { get; private set; }

    [System.Serializable]
    public class Menu {
        public InputActionReference openCloseKey; // Action to open and close the menu
        public GameObject associatedMenuScreen; // The parent gameObject for the screen to open, leave empty for dummy menu
        public bool pausesOnSafe; // Should this menu pause the game on Safe mode?
        public bool pausesOnEuclid; // Should this menu pause the game on Euclid mode?
        public bool pausesOnKeter; // Should this menu pause the game on Keter mode?
        public bool requireConsoleToggle; // Hard coded value for the console window where it can only be opened if console is enabled

        [HideInInspector] public bool isOpen;
    }

    [SerializeField] private Menu[] menuList;

    public bool disableMenuToggling;

    private readonly Dictionary<InputAction, System.Action<InputAction.CallbackContext>> callbacks = new();
    private RevivalSessionEngine gameManager;

    private bool cantFunction;
    private bool unregisteredMenuOpen;
    public bool anyMenuCurrentlyOpen => unregisteredMenuOpen || System.Array.Exists(menuList, m => m.isOpen);
    private int currentDifficulty;

    #region Unity Lifecycle

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable() {
        // For every menu in the list, register each action callback to the ToggleMenu method
        foreach (Menu menu in menuList) {
            InputAction action = menu.openCloseKey?.action;
            if (action == null) continue;

            void Callback(InputAction.CallbackContext ctx) => ToggleMenu(menu);
            callbacks[action] = Callback;
            action.performed += Callback;
        }
    }

    private void OnDisable() {
        // For every action callbacks variables, unsubscribe from them and clear the callbacks dictionary
        foreach (var (action, callback) in callbacks)
            action.performed -= callback;
        callbacks.Clear();
    }

    private void Start() {
        // Attempt to locate the GameManager instance at the start of this scripts lifecycle
        if (RevivalSessionEngine.Instance != null) gameManager = RevivalSessionEngine.Instance;

        // If there is no GameManager available at the start, disallow functionality and print a warning in console
        if (gameManager == null) {
            cantFunction = true;
            Debug.Log("<color=red>[RevivalSessionEngine]</color> RevivalSessionEngine was not found, menu stuff will not work!");

            return;
        }
    }

    #endregion

    #region Private Methods
    private void ToggleMenu(Menu menu, bool? forceState = null) {
        // Allow closing an open menu, but block opening when another is already open
        bool open = forceState ?? !menu.isOpen;
        if (open == menu.isOpen || cantFunction) return;
        if (open && anyMenuCurrentlyOpen) return;
        if (menu.requireConsoleToggle && !RevivalRuntimeEngine.SettingsData.consoleEnabled) return; // Prevent menu from opening if "Enable Console" is disabled

        // Hardcoded gate to prevent the player from closing the pause menu if the options or achievements menu is open
        // Hopefully will not have any unintended side effects later
        if (RevivalRuntimeEngine.Instance.anyMenuOpen) return;

        if (disableMenuToggling) return;

        // Set the menus local isOpen value to the opposite itself for toggling
        // Then set its screen to enabled or disabled based on that isOpen value
        // Refresh the pause state to make sure stuff is paused respectively
        menu.isOpen = open;
        if (menu.associatedMenuScreen != null) menu.associatedMenuScreen.SetActive(open);
        RefreshPauseState();

        // Cleanup for the tooltip so it's not sticking around on the screen
        TooltipManager.Instance.HideTooltip();
    }

    private void RefreshPauseState() {
        if (cantFunction) return;

        bool shouldPause = unregisteredMenuOpen;

        foreach (Menu menu in menuList) {
            if (!menu.isOpen) continue;
            shouldPause = true;

            // Temporarily removing this because pausing is misbehaving on different gamemodes
            /*if ((menu.pausesOnSafe && currentDifficulty == 0) || (menu.pausesOnEuclid && currentDifficulty == 1) || (menu.pausesOnKeter && currentDifficulty == 2)) {
                shouldPause = true;
                break;
            }*/
        }

        // Welcome to this abhorrent section of code that allows Euclid and Keter modes to pause (or not pause)
        // properly, and it's all hard coded, which is not good by any means. But does it work? Yes. For now...
        // One day this might end up in a code comments video, so hello from 5/27/2026. Are gas prices down yet?
        // If I'm not in a video, hello to the random guy reading my code, and I ask you the same question ^^^

        // Removed hardcoded nonsense and just having everything pause everything for now
        if (shouldPause) RevivalSessionEngine.PauseGame();
        else RevivalSessionEngine.ResumeGame();
    }
    #endregion

    /// <summary>
    /// Publicly available method to toggle any menu via it's index and optionally force what state to toggle too
    /// </summary>
    /// <param name="menuIndex">Array index of the menu to control</param>
    /// <param name="forceState">Set this to force a menu on or off, but leave empty to just toggle it</param>
    public void ToggleMenu(int menuIndex, bool? forceState = null) {
        if (menuIndex < 0 || menuIndex >= menuList.Length) return;
        ToggleMenu(menuList[menuIndex], forceState);
    }

    /// <summary>
    /// Method to toggle the same functions the other menus have as well as keep track of it's open state despite it
    /// not having a special menu in the menu list
    /// </summary>
    /// <param name="forceState">Optional state to force. Leave null to toggle.</param>
    public void ToggleUnregisteredMenu(bool? forceState = null) {
        if (cantFunction) return;

        var open = forceState ?? !unregisteredMenuOpen;

        if (open && !unregisteredMenuOpen && System.Array.Exists(menuList, m => m.isOpen)) return;
        if (open == unregisteredMenuOpen) return;

        unregisteredMenuOpen = open;

        RefreshPauseState();

        TooltipManager.Instance.HideTooltip();
    }
}