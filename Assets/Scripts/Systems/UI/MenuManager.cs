using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Script to handle the opening and closing of various in game menus.
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
    private bool anyMenuCurrentlyOpen => System.Array.Exists(menuList, m => m.isOpen);
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
    }

    private void RefreshPauseState() {
        if (cantFunction) return;
        
        var shouldPause = false;

        foreach (var menu in menuList) {
            if (!menu.isOpen) continue;
            if ((menu.pausesOnSafe && currentDifficulty == 0) || (menu.pausesOnEuclid && currentDifficulty == 1)) {
                shouldPause = true;
                break;
            }
        }

        // If mode is Keter or higher, only disable inputs. If it's Safe or Euclid, pause and resume respectively
        if (gameManager.currentDifficulty >= 2) {
            Player.Instance.disableInput = !Player.Instance.disableInput;
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
}