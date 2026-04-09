using FMODUnity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour {
    public static InventorySystem instance { get; private set; }

    [SerializeField] private IngameMenuManager ingameMenuManager;
    public ItemData currentHeldItemData;

    [Header("Input")]
    public InputActionAsset playerControls;

    [Header("Slots")]
    public List<GameObject> slotList = new List<GameObject>();
    private const int MAX_SLOTS = 10;

    private readonly HashSet<string> itemSet = new HashSet<string>();
    private InputAction rightClickAction;

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void OnEnable() {
        playerControls.Enable();
        rightClickAction = playerControls.FindAction("RightClick");
        rightClickAction.performed += OnRightClick;
    }

    private void OnDisable() {
        if (rightClickAction != null) {
            rightClickAction.performed -= OnRightClick;
        }
        playerControls.Disable();
    }

    public bool IsFull() {
        int count = 0;
        foreach (GameObject slot in slotList) {
            if (slot.transform.childCount > 1) count++;
        }
        return count >= MAX_SLOTS;
    }

    public void AddToInventory(string itemName) {
        GameObject slot = FindNextEmptySlot();
        if (slot == null) return;

        GameObject itemObj = Instantiate(Resources.Load<GameObject>(itemName), slot.transform);
        itemSet.Add(itemName);
    }

    public void RemoveItem(string itemName) {
        itemSet.Remove(itemName);
    }

    private GameObject FindNextEmptySlot() {
        foreach (GameObject slot in slotList) {
            if (slot.transform.childCount == 1) return slot;
        }
        return null;
    }

    public void EquipItem(ItemData data) {
        if (CanvasInstance.instance.heldDocumentDisplay.activeSelf) return;

        currentHeldItemData = data;
        SetDisplayActive(CanvasInstance.instance.heldItemDisplay, data.icon, data.equipSound);
        ingameMenuManager.ToggleMenuByID(0);
    }

    public void EquipDocument(ItemData data) {
        if (CanvasInstance.instance.heldDocumentDisplay.activeSelf) return;

        currentHeldItemData = data;
        SetDisplayActive(CanvasInstance.instance.heldDocumentDisplay, data.icon, data.equipSound);
        ingameMenuManager.ToggleMenuByID(0);
    }

    private void OnRightClick(InputAction.CallbackContext ctx) {
        if (currentHeldItemData == null) return;

        if (CanvasInstance.instance.heldDocumentDisplay.activeSelf) {
            UnequipDocument();
        }
        else if (CanvasInstance.instance.heldItemDisplay.activeSelf) {
            UnequipItem();
        }
    }

    public void UnequipItem() {
        CanvasInstance.instance.heldItemDisplay.SetActive(false);
        AudioManager.instance.PlaySound(currentHeldItemData.equipSound, transform.position);
        currentHeldItemData = null;
    }

    private void UnequipDocument() {
        CanvasInstance.instance.heldDocumentDisplay.SetActive(false);
        AudioManager.instance.PlaySound(currentHeldItemData.equipSound, transform.position);
        currentHeldItemData = null;
    }

    private void SetDisplayActive(GameObject display, Sprite sprite, EventReference eventReference) {
        display.GetComponent<Image>().sprite = sprite;
        display.SetActive(true);
        AudioManager.instance.PlaySound(eventReference, transform.position);
    }
}