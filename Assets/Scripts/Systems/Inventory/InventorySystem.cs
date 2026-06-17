using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using IngameDebugConsole;
using PixeLadder.EasyTooltip;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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
 
        DebugLogConsole.AddCommand<string>("spawnitem", "Spawns an item with the given ID", DebugSpawnItem);
        DebugLogConsole.AddCommand("listitems", "Lists available item IDs", DebugListItems);
        DebugLogConsole.AddCommand<string, int>("additem", "Adds an item with the given ID to the inventory", AddItemToInventory);
        DebugLogConsole.AddCommand<string, bool>("removeitem", "Removes one instance of an item by ID", RemoveItemFromInventory);
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
        // Find the first slot holding this item without modifying the collection
        GameObject targetSlot = null;
        foreach (var (slot, id) in slotContents) {
            if (id != itemIdentifier) continue;
            targetSlot = slot;
            break; // Stop as soon as the first match is found
        }
    
        if (targetSlot == null) return;
        UnregisterItem(targetSlot);
    
        // Spawn the item in the world if defined to do so
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
    }
    #endregion

    #region Equipment Methods (Move Somewhere Else Later)

    public void EquipItem(ItemData itemToEquip) {
        if (itemToEquip == currentlyHeldItem) { UnequipCurrentItem(); return; }
        currentlyHeldItem = itemToEquip;
    }

    public void UnequipCurrentItem() {
        AudioManager.PlayOneShot(currentlyHeldItem.useItemSound);
        currentlyHeldItem = null;
    }

    public void DisplayItem() {

    }

    public void DisplayDocument() {

    }

    #endregion
}