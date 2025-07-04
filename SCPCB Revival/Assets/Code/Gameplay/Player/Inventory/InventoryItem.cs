using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Item Data")]
    [Tooltip("The physical prefab for spawning in the world."), ShowAssetPreview]
    public GameObject itemPrefab;

    [Tooltip("The type of the item.")]
    public enum ItemType { Normal, Keycard, Consumable, Document, Equipment }
    public ItemType itemType;

    [Tooltip("Display name of the item.")]
    public string thisName;

    [ShowIf("itemType", ItemType.Keycard), Tooltip("Key level associated with the item.")]
    public int thisKeyLevel;

    [Tooltip("Icon or image representing the item.")]
    [SerializeField] private Sprite thisImage;

    [Header("Equipping")]
    [Tooltip("Indicates if the item can be equipped.")]
    public bool isEquippable;

    [Tooltip("Sound effect for equipping or unequipping.")]
    [SerializeField] private AudioClip equipUnequipSound;

    // --- Item Info UI --- //
    private GameObject itemInfoUI;
    private TextMeshProUGUI itemInfoUI_itemName;

    private void Start()
    {
        itemInfoUI = InventorySystem.instance.itemInfoUI;
        itemInfoUI_itemName = itemInfoUI.transform.Find("ItemName").GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        itemInfoUI.SetActive(true);
        itemInfoUI_itemName.text = thisName;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        itemInfoUI.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            switch (itemType)
            {
                case ItemType.Normal or ItemType.Keycard when isEquippable:
                    Debug.Log("Equipped " + thisName);
                    InventorySystem.instance.EquipItem(thisName, thisImage, equipUnequipSound, thisKeyLevel);
                    break;
                case ItemType.Consumable:
                    Debug.Log("Double clicked on a consumable item!");
                    break;
                case ItemType.Document:
                    Debug.Log("Reading " + thisName);
                    InventorySystem.instance.EquipDocument(thisName, thisImage, equipUnequipSound);
                    break;
                case ItemType.Equipment:
                    Debug.Log("Double clicked on an equipment item!");
                    break;
            }
        }
    }
}