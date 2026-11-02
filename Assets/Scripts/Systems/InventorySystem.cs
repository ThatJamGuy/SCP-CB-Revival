using PixeLadder.EasyTooltip;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Inventory system script to handle the current items in the inventory and communicate with other scripts needing access to inventory values
/// </summary>
public class InventorySystem : MonoBehaviour, ISaveable {
    public static InventorySystem Instance { get; private set; }

    [SerializeField] private MenuManager menuManager;

    [SerializeField] private GameObject inventoryItemTemplate;
    [SerializeField] private GameObject[] inventorySlotObjects;

    [Header("Inventory States")]
    public ItemData currentlyHeldItem;

    [Header("UI Things")]
    [SerializeField] private Image itemDisplay;
    [SerializeField] private Image documentDisplay;

    [Header("Debug")]
    public ItemData[] itemDataList;

    public Camera playerCamera;

    // Tracks all the items in the itemDataList for referencing when needed, like spawning/adding items
    private Dictionary<string, ItemData> itemDataLookup;
    private readonly Dictionary<string, int> inventoryContents = new();
    private readonly Dictionary<GameObject, string> slotContents = new();

    private const int MAX_SLOTS = 10;

    private InputAction unequipItemAction;

    #region Unity Lifecycle

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        unequipItemAction = RevivalRuntimeEngine.Instance.GetAction("Player", "RightClick");
    }

    private void Start() {
        itemDataLookup = new Dictionary<string, ItemData>();

        foreach (ItemData item in itemDataList) {
            itemDataLookup.Add(item.itemIdentifier, item);
        }

        // Load inventory from save if applicable, but otherwise register this saveable
        LoadInventoryFromSave();
        SavegameEngine.Register(this);
    }

    private void OnEnable() {
        unequipItemAction.performed += OnRightClick;
        unequipItemAction.Enable();
    }

    private void OnDisable() {
        unequipItemAction.performed -= OnRightClick;
        unequipItemAction.Disable();
    }

    private void OnDestroy() {
        SavegameEngine.Unregister(this);
        if (Instance == this) Instance = null;
    }

    #endregion

    #region Private Methods

    private GameObject CheckForEmptySlots() {
        // For every slot in the inventorySlotObjects array...
        foreach (GameObject slot in inventorySlotObjects) {
            // If the child count is 2 (Only the outlines) then return the current slot as available
            if (slot.transform.childCount == 2) return slot;
        }

        // Otherwise return null because the slot has something
        return null;
    }

    // Creats and sets up the UI object for an item inside a specific slot and registers it
    private void CreateItemInSlot(GameObject slot, ItemData itemData)
    {
        if (inventoryItemTemplate == null)
        {
            Debug.LogError("inventoryItemTemplate is null.");
        }

        GameObject itemObj = Instantiate(inventoryItemTemplate, slot.transform);

        if (itemObj.TryGetComponent<Image>(out Image img))
        {
            img.sprite = itemData.itemInventoryIcon;
        }

        if (itemObj.TryGetComponent<TooltipTrigger>(out TooltipTrigger tooltip))
        {
            tooltip.Title = itemData.itemName;
            tooltip.Content = itemData.itemDescription;
        }

        if (itemObj.TryGetComponent<InventoryItem>(out InventoryItem invItem))
        {
            invItem.itemData = itemData;
        }

        RegisterItem(slot, itemData.itemIdentifier);
    }

    private void RegisterItem(GameObject slot, string itemIdentifier) {
        slotContents[slot] = itemIdentifier;
        inventoryContents[itemIdentifier] = inventoryContents.TryGetValue(itemIdentifier, out int count) ? count + 1 : 1;
    }

    private void UnregisterItem(GameObject slot) {
        if (!slotContents.Remove(slot, out string itemIdentifier)) return;

        if (!inventoryContents.TryGetValue(itemIdentifier, out int count)) return;
        if (count <= 1) inventoryContents.Remove(itemIdentifier);
        else inventoryContents[itemIdentifier] = count - 1;
    }

    private void OnRightClick(InputAction.CallbackContext context) {
        if (context.performed && currentlyHeldItem != null) UnequipCurrentItem();
    }

    private void LoadInventoryFromSave() {
        PlayerData data = SavegameEngine.Get<PlayerData>();
        string[] slotItems = data.slotItems;
        if (slotItems == null) return;

        int count = Mathf.Min(slotItems.Length, inventorySlotObjects.Length);
        for (int i = 0; i < count; i++) {
            string id = slotItems[i];

            if (string.IsNullOrEmpty(id)) continue;
            if (!itemDataLookup.TryGetValue(id, out ItemData item)) continue;

            CreateItemInSlot(inventorySlotObjects[i], item);
        }
    }

    #endregion

    #region Public Methods

    public int GetItemCount(string itemIdentifier) => inventoryContents.GetValueOrDefault(itemIdentifier, 0);

    public bool IsFull() {
        int fullSlotCount = 0;
        foreach (var slot in inventorySlotObjects) {
            if (slot.transform.childCount > 2) {
                fullSlotCount++;
                if (fullSlotCount >= MAX_SLOTS)
                    return true;
            }
        }
        return false;
    }

    public bool HasItem(string itemIdentifier) => GetItemCount(itemIdentifier) > 0;

    public void AddItemToInventory(string itemIdentifier, int amount = 1) {
        if (!itemDataLookup.TryGetValue(itemIdentifier, out ItemData itemData)) return;

        for (var i = 0; i < amount; i++) {
            GameObject slot = CheckForEmptySlots();
            if (!slot) {
                RevivalSessionEngine.Instance.NotifyPlayer("You cannot pick up any more items.");
                return;
            }

            CreateItemInSlot(slot, itemData);
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
        RevivalSessionEngine.ResumeGame();
        menuManager.ToggleMenu(1, false);
    }

    public void SetPlayerCamera(Camera camera) => playerCamera = camera;

    // Save inventory data
    public void PrepareSaveableValues() {
        PlayerData data = SavegameEngine.Get<PlayerData>();
        string[] slotItems = new string[inventorySlotObjects.Length];

        for (int i = 0; i < slotItems.Length; i++) {
            slotItems[i] = slotContents.TryGetValue(inventorySlotObjects[i], out string id) ? id : string.Empty;
        }

        data.slotItems = slotItems;
    }

    #endregion

    #region Debug Methods

    public void DebugListItems() => Debug.Log(string.Join(", ", itemDataLookup.Keys));

    public void DebugSpawnItem(string itemIdentifier, bool viaConsole = false) {
        if (!itemDataLookup.TryGetValue(itemIdentifier, out var item)) {
            Debug.LogWarning("An item with that identifier was not found.");
            return;
        }

        if (viaConsole) Debug.Log($"<color=#00ffff>Spawned item: {itemIdentifier}</color>");

        var spawnPos = playerCamera.transform.position + playerCamera.transform.forward * 0.7f;
        var randomRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        Instantiate(item.itemWorldPrefab, spawnPos, randomRot);
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
    }

    #endregion
}