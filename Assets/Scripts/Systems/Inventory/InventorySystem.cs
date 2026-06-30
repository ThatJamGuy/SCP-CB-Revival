using PixeLadder.EasyTooltip;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Inventory system script to handle the current items in the inventory and communicate with other scripts needing access to inventory values
/// </summary>
public class InventorySystem : MonoBehaviour {
    public static InventorySystem Instance { get; private set; }

    [SerializeField] private MenuManager menuManager;

    //[System.Serializable] public readonly List<ItemData> itemsInInventory = new List<ItemData>();
    [SerializeField] private GameObject inventoryItemTemplate;
    [SerializeField] private GameObject[] inventorySlotObjects;

    [Header("Inventory States")]
    // Two equipped item layers to allow the player to wear stuff and hold things at the same time
    public ItemData currentlyHeldItem; // Layer 1 for regular items
    public ItemData currentlyWornItem; // Layer 2 for worn items

    [Header("UI Things")]
    [SerializeField] private Image itemDisplay;
    [SerializeField] private Image documentDisplay;

    [Header("Debug")]
    public ItemData[] itemDataList;

    private Camera playerCamera;

    // Tracks all the items in the itemDataList for referencing when needed, like spawning/adding items
    private Dictionary<string, ItemData> itemDataLookup;
    private readonly Dictionary<string, int> inventoryContents = new();
    private readonly Dictionary<GameObject, string> slotContents = new();

    private const int MAX_SLOTS = 10;

    private InputAction unequipItemAction;

    #region Unity Callbacks

    private void Awake() {
        // Ensure that there is only one instance of the Inventory system
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        unequipItemAction = InputManager.Instance.GetAction("Player", "RightClick");
    }

    private void Start() {
        itemDataLookup = itemDataList.ToDictionary(item => item.itemIdentifier);
        playerCamera = Player.Instance.playerCamera;

        DebugConsole.AddCommand<string>("spawnitem", "Spawns an item with the given identifier.", DebugSpawnItem);
        DebugConsole.AddCommand("listitems", "Lists available item identifiers.", DebugListItems);
    }

    private void OnEnable() {
        unequipItemAction.performed += OnRightClick;
        unequipItemAction.Enable();
    }

    private void OnDisable() {
        unequipItemAction.performed -= OnRightClick;
        unequipItemAction.Disable();
    }

    #endregion

    #region Private Methods

    private GameObject CheckForEmptySlots() {
        // For every slot in the inventorySlotObjects array...
        foreach (var slot in inventorySlotObjects) {
            // If the child count is 2 (Only the outlines) then return the current slot as available
            if (slot.transform.childCount == 2) return slot;
        }

        // Otherwise return null because the slot has something
        return null;
    }

    private void RegisterItem(GameObject slot, string itemIdentifier) {
        slotContents[slot] = itemIdentifier;
        inventoryContents[itemIdentifier] = inventoryContents.TryGetValue(itemIdentifier, out var count) ? count + 1 : 1;
    }

    private void UnregisterItem(GameObject slot) {
        if (!slotContents.Remove(slot, out var itemIdentifier)) return;

        if (!inventoryContents.TryGetValue(itemIdentifier, out var count)) return;
        if (count <= 1) inventoryContents.Remove(itemIdentifier);
        else inventoryContents[itemIdentifier] = count - 1;
    }

    private void OnRightClick(InputAction.CallbackContext context) {
        if (context.performed && currentlyHeldItem != null) UnequipCurrentItem();
    }
    #endregion

    #region Public Methods

    public int GetItemCount(string itemIdentifier) => inventoryContents.GetValueOrDefault(itemIdentifier, 0);

    public bool IsFull() => inventorySlotObjects.Count(slot => slot.transform.childCount > 2) >= MAX_SLOTS;
    public bool HasItem(string itemIdentifier) => GetItemCount(itemIdentifier) > 0;

    public void AddItemToInventory(string itemIdentifier, int amount = 1) {
        if (!itemDataLookup.TryGetValue(itemIdentifier, out var itemData)) return;

        for (var i = 0; i < amount; i++) {
            var slot = CheckForEmptySlots();
            if (!slot) {
                InfoTextManager.Instance.NotifyPlayer("You cannot pick up any more items.");
                return;
            }

            var itemObj = Instantiate(inventoryItemTemplate, slot.transform);
            itemObj.GetComponent<Image>().sprite = itemData.itemInventoryIcon;

            var tooltip = itemObj.GetComponent<TooltipTrigger>();
            tooltip.Title = itemData.itemName;
            tooltip.Content = itemData.itemDescription;

            var invItem = itemObj.GetComponent<InventoryItem>();
            invItem.itemData = itemData;

            RegisterItem(slot, itemIdentifier);
        }
    }

    public void RemoveItemFromInventory(string itemIdentifier, bool alsoSpawnItemInWorld) {
        // Make sure item still isn't being held when dropped
        if (currentlyHeldItem != null && currentlyHeldItem.itemIdentifier == itemIdentifier) UnequipCurrentItem();

        // Find the first slot holding this item without modifying the collection
        GameObject targetSlot = null;
        foreach (var (slot, id) in slotContents) {
            if (id != itemIdentifier) continue;
            targetSlot = slot;
            break; // Stop as soon as the first match is found
        }

        if (targetSlot == null) return;
        UnregisterItem(targetSlot);

        if (alsoSpawnItemInWorld) DebugSpawnItem(itemIdentifier);
    }

    public void CloseInventory() {
        GameManager.ResumeGame();
        menuManager.ToggleMenu(1, false);
    }
    #endregion

    #region Debug Methods

    private void DebugListItems() => Debug.Log(string.Join(", ", itemDataLookup.Keys));

    private void DebugSpawnItem(string itemIdentifier) {
        if (!itemDataLookup.TryGetValue(itemIdentifier, out var item)) return;

        var spawnPos = playerCamera.transform.position + playerCamera.transform.forward * 0.7f;
        var randomRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        Instantiate(item.itemWorldPrefab, spawnPos, randomRot);

        Debug.Log($"<color=aqua>Spawned an item with id: {itemIdentifier}</color>");
    }
    #endregion

    #region Equipment Methods (Move Somewhere Else Later Maybe)

    public void EquipItem(ItemData itemToEquip) {
        if (itemToEquip == currentlyHeldItem) { UnequipCurrentItem(); return; }
        currentlyHeldItem = itemToEquip;

        UpdateDisplay();
    }

    public void UnequipCurrentItem() {
        AudioManager.PlayOneShot(currentlyHeldItem.useItemSound);
        currentlyHeldItem = null;

        UpdateDisplay();
    }

    public void UpdateDisplay() {
        // Disable all this stuff immediately to reset the display or just hide things
        itemDisplay.gameObject.SetActive(false);
        documentDisplay.gameObject.SetActive(false);

        // After reset check to see if something was unequipped or new item was equipped
        if (currentlyHeldItem == null) return;

        // Display key item if set to do so (Other items displayed via Custom)
        if (currentlyHeldItem.chosenBehavior == PresetBehavior.Key) {
            itemDisplay.sprite = currentlyHeldItem.itemInventoryIcon;
            itemDisplay.gameObject.SetActive(true);

            return;
        }

        // Display a document if set to do so
        if (currentlyHeldItem.chosenBehavior == PresetBehavior.Document) {
            documentDisplay.sprite = currentlyHeldItem.documentDisplaySprite;
            documentDisplay.gameObject.SetActive(true);

            return;
        }

        //TODO: Figure out what to do if it's a custom item, as it could go multiple ways
    }

    #endregion
}