using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem instance { get; set; }

    public List<GameObject> slotList = new List<GameObject>();
    public List<string> itemList = new List<string>();

    [SerializeField] private InventoryScreen inventoryScreen;

    public GameObject itemInfoUI;
    [SerializeField] private GameObject currentHeldItemDisplay;
    [SerializeField] private AudioSource itemEquipUnequipSource;

    public string currentHeldItem;
    public int currentKeyLevel;

    private GameObject itemToAdd;
    private GameObject whatSlotToEquip;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(1) && currentHeldItem != null)
        {
            UnequipItem();
        }
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

    public void EquipItem(string itemName, Sprite itemImage, AudioClip audioClip, int keyLevel)
    {
        currentHeldItem = itemName;
        currentKeyLevel = keyLevel;

        currentHeldItemDisplay.GetComponent<Image>().sprite = itemImage;
        currentHeldItemDisplay.SetActive(true);

        itemEquipUnequipSource.clip = audioClip;
        itemEquipUnequipSource.Stop();
        itemEquipUnequipSource.Play();

        CloseInventory();
    }

    public void UnequipItem()
    {
        currentHeldItemDisplay.SetActive(false);
        itemEquipUnequipSource.Stop();
        itemEquipUnequipSource.Play();

        currentHeldItem = null;
        currentKeyLevel = 0;
    }

    public void CloseInventory()
    {
        inventoryScreen.ToggleInventory();
    }
}