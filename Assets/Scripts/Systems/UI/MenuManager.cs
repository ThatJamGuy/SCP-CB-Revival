using PixeLadder.EasyTooltip;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Script to handle the opening and closing of various in game menus.
/// Now supports unregistered menus for less detailed toggling of said menus.
/// </summary>
public class MenuManager : MonoBehaviour {
    [System.Serializable]
    public class Menu {
        public InputActionReference openCloseKey; // Action to open and close the menu
        public GameObject associatedMenuScreen; // The parent gameObject for the screen to open
        public bool pausesOnSafe; // Should this menu pause the game on Safe mode?
        public bool pausesOnEuclid; // Should this menu pause the game on Euclid mode?

        [HideInInspector] public bool isOpen;
    }

    [SerializeField] private Menu[] menuList;

    private readonly Dictionary<InputAction, System.Action<InputAction.CallbackContext>> callbacks = new();
    private GameManager gameManager;

    private bool cantFunction;
    private bool unregisteredMenuOpen;
    private bool anyMenuCurrentlyOpen => unregisteredMenuOpen || System.Array.Exists(menuList, m => m.isOpen);
    private int currentDifficulty;

    #region Unity Callbacks

    private void OnEnable() {
        // For every menu in the list, register each action callback to the ToggleMenu method
        foreach (var menu in menuList) {
            var action = menu.openCloseKey?.action;
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
        if (GameManager.Instance != null) gameManager = GameManager.Instance;

        // If there is no GameManager available at the start, disallow functionality and print a warning in console
        if (gameManager == null) {
            cantFunction = true;
            Debug.Log("<color=red>[MenuManager]</color> GameManager was not found, menu stuff will not work!");

            return;
        }

        if (cantFunction) return;

        // Set the local current difficulty to the one in the GameManager script for easy access
        currentDifficulty = gameManager.currentDifficulty;
    }

    #endregion

    #region Private Methods
    private void ToggleMenu(Menu menu, bool? forceState = null) {
        // Allow closing an open menu, but block opening when another is already open
        var open = forceState ?? !menu.isOpen;
        if (open == menu.isOpen || cantFunction) return;
        if (open && anyMenuCurrentlyOpen) return;

        // Set the menus local isOpen value to the opposite itself for toggling
        // Then set its screen to enabled or disabled based on that isOpen value
        // Refresh the pause state to make sure stuff is paused respectively
        menu.isOpen = open;
        menu.associatedMenuScreen.SetActive(open);
        RefreshPauseState();

        // Cleanup for the tooltip so it's not sticking around on the screen
        TooltipManager.Instance.HideTooltip();
    }

    private void RefreshPauseState() {
        if (cantFunction) return;

        var shouldPause = unregisteredMenuOpen;

        foreach (var menu in menuList) {
            if (!menu.isOpen) continue;
            if ((menu.pausesOnSafe && currentDifficulty == 0) || (menu.pausesOnEuclid && currentDifficulty == 1)) {
                shouldPause = true;
                break;
            }
        }

        // Welcome to this abhorrent section of code that allows Euclid and Keter modes to pause (or not pause)
        // properly, and it's all hard coded, which is not good by any means. But does it work? Yes. For now...
        // One day this might end up in a code comments video, so hello from 5/27/2026. Are gas prices down yet?
        // If I'm not in a video, hello to the random guy reading my code, and I ask you the same question ^^^

        // If the mode is Euclid specifically, do this pausing stuff anyway because inventory menu needs it
        if (gameManager.currentDifficulty == 1) {
            Player.Instance.disableInput = !Player.Instance.disableInput;
            Player.Instance.isMoving = false;
            Player.SetCursorState(Player.Instance.disableInput);
            return;
        }

        // If mode is Keter or higher, only disable inputs. If it's Safe or Euclid, pause and resume respectively
        if (gameManager.currentDifficulty >= 2) {
            Player.Instance.disableInput = !Player.Instance.disableInput;
            Player.Instance.isMoving = false;
            Player.SetCursorState(Player.Instance.disableInput);
            return;
        }

        if (shouldPause) GameManager.PauseGame();
        else GameManager.ResumeGame();
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