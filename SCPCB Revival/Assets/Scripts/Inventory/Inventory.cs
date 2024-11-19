using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [SerializeField] private List<GameObject> slots;
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private InventoryScreen inventoryScreen;
    [SerializeField] private Image currentHeldItemUI;
    [SerializeField] private AudioSource playerAudioSource;

    private List<ItemData> items = new List<ItemData>();
    public ItemData CurrentHeldItem { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (currentHeldItemUI != null)
            currentHeldItemUI.gameObject.SetActive(false); // Hide at start
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleDoubleClick();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }

    public bool AddItem(ItemData item)
    {
        if (items.Count >= slots.Count)
        {
            Debug.Log("Inventory is full!");
            return false;
        }

        items.Add(item);
        UpdateUI();
        Debug.Log($"{item.itemName} added to inventory. Total items: {items.Count}");
        return true;
    }

    public void EquipItem(ItemData item)
    {
        CurrentHeldItem = item;

        Debug.Log($"Attempting to equip: {item.itemName}, Icon: {item.itemIcon}");

        if (currentHeldItemUI != null)
        {
            currentHeldItemUI.sprite = item.itemIcon;
            currentHeldItemUI.gameObject.SetActive(true);
        }

        if (item.useSound != null && playerAudioSource != null)
            playerAudioSource.PlayOneShot(item.useSound);

        CloseInventory();
        Debug.Log($"Equipped: {item.itemName}");
    }

    public void UnequipItem()
    {
        if (CurrentHeldItem != null)
        {
            if (CurrentHeldItem.useSound != null && playerAudioSource != null)
                playerAudioSource.PlayOneShot(CurrentHeldItem.useSound);

            if (currentHeldItemUI != null)
                currentHeldItemUI.gameObject.SetActive(false); currentHeldItemUI.sprite = null;

            Debug.Log($"Unequipped: {CurrentHeldItem.itemName}");
            CurrentHeldItem = null;
        }
    }

    private void CloseInventory()
    {
        inventoryScreen.ToggleInventory();
        Debug.Log("Inventory closed.");
    }

    private void HandleDoubleClick()
    {
        if (Time.timeScale == 0)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (i < items.Count && slots[i].GetComponent<Slot>().IsDoubleClicked())
                {
                    EquipItem(items[i]);
                    break;
                }
            }
        }
    }

    private void HandleRightClick()
    {
        if (CurrentHeldItem != null)
        {
            UnequipItem();
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            Transform iconTransform = slots[i].transform.Find("Icon");
            if (iconTransform != null) Destroy(iconTransform.gameObject);

            if (i < items.Count)
            {
                GameObject icon = Instantiate(iconPrefab, slots[i].transform);
                icon.name = items[i].itemName;
                IconData iconData = icon.GetComponent<IconData>();
                if (iconData != null) iconData.SetData(items[i]);

                Image iconImage = icon.GetComponent<Image>();
                iconImage.sprite = items[i].itemIcon;
                iconImage.enabled = true;
            }
        }
    }
}