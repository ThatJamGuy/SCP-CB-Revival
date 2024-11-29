using UnityEngine;
using UnityEngine.UI;

public class InventoryScreen : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject inventoryScreen;

    public bool isOpen;

    public void ToggleInventory()
    {
        isOpen = !isOpen;
        GameManager.Instance.TogglePlayerInput(true);
        inventoryScreen.SetActive(isOpen);

        if (GameManager.Instance.inventoryPausesGame)
            GameManager.Instance.PauseGame();
    }
}