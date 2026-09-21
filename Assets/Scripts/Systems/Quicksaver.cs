using UnityEngine;
using UnityEngine.InputSystem;

public class Quicksaver : MonoBehaviour {
    [SerializeField] private InputActionReference quicksaveAction;
    [SerializeField] private SaveGameServiceBehvaior saveService;

    private void OnEnable() {
        if (quicksaveAction == null) {
            Debug.LogError("Quicksave action is not assigned.", this);
            return;
        }

        quicksaveAction.action.performed += OnQuicksavePerformed;
        quicksaveAction.action.Enable();
    }

    private void OnDisable() {
        if (quicksaveAction == null) return;

        quicksaveAction.action.performed -= OnQuicksavePerformed;
        quicksaveAction.action.Disable();
    }

    private void OnQuicksavePerformed(InputAction.CallbackContext context) {
        if (saveService == null) {
            Debug.LogError("SaveGameServiceBehaviour is not assigned to QuicksaveController.", this);
            return;
        }

        if (!saveService.CanSave) return;

        saveService.Save();
    }
}