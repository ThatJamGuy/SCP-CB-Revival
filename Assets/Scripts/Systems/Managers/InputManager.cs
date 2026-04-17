using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Globally accessible Input manager that will allow scripts to access defined inputs.
/// I am not a fan of this new input system because there's so many ways to do the same thing bruh
/// </summary>
public sealed class InputManager : MonoBehaviour {
    public static InputManager Instance;

    [SerializeField] private InputActionAsset inputActionAsset;

    private void Awake() {
        // If there's no InputManager already around, set the instance to this guy
        // Otherwise, if one already exists then we kill ourselves
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Keep him with us for the journey assuming the check passes
        DontDestroyOnLoad(gameObject);
    }
    
    // Setup and Cleanup for the input action asset
    private void OnEnable() => inputActionAsset.Enable();
    private void OnDisable() => inputActionAsset.Disable();
    
    // Gets an action for a given map. Called from external scripts
    public InputAction GetAction(string mapName, string actionName) {
        var action = inputActionAsset.FindActionMap(mapName)?.FindAction(actionName);
        
        if (action == null) Debug.LogWarning($"Action {actionName} not found in Map {mapName}.");
        
        return action;
    }
}