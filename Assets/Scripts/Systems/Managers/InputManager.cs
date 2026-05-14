using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

/// <summary>
/// Globally accessible Input manager that will allow scripts to access defined inputs.
/// I am not a fan of this new input system because there's so many ways to do the same thing bruh
/// </summary>
public sealed class InputManager : MonoBehaviour {
    public static InputManager Instance;
    public event Action<bool> OnInputDeviceChanged;
    public bool UsingController { get; private set; }
    
    [SerializeField] private InputActionAsset inputActionAsset;
    private IDisposable _buttonListener;

    #region Unity Callbacks
    private void Awake() {
        // If there's no InputManager already around, set the instance to this guy
        // Otherwise, if one already exists then we kill ourselves
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Keep him with us for the journey assuming the check passes
        DontDestroyOnLoad(gameObject);
    }
    
    // Setup and Cleanup for the input action asset
    private void OnEnable() {
        inputActionAsset.Enable();
        _buttonListener = InputSystem.onAnyButtonPress.Call(OnAnyButtonPress);
    }

    private void OnDisable() {
        inputActionAsset.Disable();
        _buttonListener?.Dispose();
    }

    private void Update() {
        if (!UsingController || Mouse.current == null) return;
        if (Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
            SetInputDevice(false);
    }
    #endregion
    
    #region Private Methods
    private void OnAnyButtonPress(InputControl control) => SetInputDevice(control.device is Gamepad);
    
    private void SetInputDevice(bool controller) {
        if (UsingController == controller) return;
        UsingController = controller;
        OnInputDeviceChanged?.Invoke(controller);
    }
    #endregion

    // Gets an action for a given map. Called from external scripts
    public InputAction GetAction(string mapName, string actionName) {
        var action = inputActionAsset.FindActionMap(mapName)?.FindAction(actionName);
        
        if (action == null) Debug.LogWarning($"Action {actionName} not found in Map {mapName}.");
        
        return action;
    }
}