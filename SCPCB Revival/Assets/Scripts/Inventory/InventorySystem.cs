using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem instance { get; set; }

    public List<GameObject> slotList = new List<GameObject>();
    public List<string> itemList = new List<string>();

    [SerializeField] private InventoryScreen inventoryScreen;

    public GameObject itemInfoUI;

    private GameObject itemToAdd;
    private GameObject whatSlotToEquip;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;
    }

    public void AddToInventory(string itemName)
    { 
        whatSlotToEquip = FindNextEmptySlot();

        itemToAdd = Instantiate(Resources.Load<GameObject>(itemName), whatSlotToEquip.transform.position, whatSlotToEquip.transform.rotation);
        itemToAdd.transform.SetParent(whatSlotToEquip.transform);

        itemList.Add(itemName);
    }

    public bool CheckIfFull()
    {
        int counter = 0;

        foreach (GameObject slot in slotList)
        {
            if(slot.transform.childCount > 0)
            {
                counter += 1;
            }
        }

        if (counter == 10)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private GameObject FindNextEmptySlot()
    {
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount == 0)
            {
                return slot;
            }
        }

        return new GameObject();
    }

    public void CloseInventory()
    {
        inventoryScreen.ToggleInventory();
    }
}