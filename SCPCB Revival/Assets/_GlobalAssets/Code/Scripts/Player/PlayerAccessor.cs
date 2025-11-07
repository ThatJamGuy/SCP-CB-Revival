using UnityEngine;
using SickDev.CommandSystem;

public class PlayerAccessor : MonoBehaviour {
    public static PlayerAccessor playerAccessor;

    public PlayerMovement playerMovement;

    public void EnablePlayerInputs() {
        playerMovement.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void DisablePlayerInputs(bool showMouse) {
        playerMovement.enabled = false;

        if (showMouse) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}