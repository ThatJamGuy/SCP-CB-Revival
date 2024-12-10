using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Inventory Item Data")]
    public GameObject itemPrefab;

    public enum ItemType {Normal, Consumable, Document, Equipment};
    public ItemType itemType;

    // --- Item Info UI --- //
    private GameObject itemInfoUI;
    private TextMeshProUGUI itemInfoUI_itemName;

    [SerializeField] private Sprite thisImage;
    public string thisName;
    public int thisKeyLevel;

    // --- Equipping --- //
    private GameObject itemPendingEqip;
    public bool isEquippable;

    [SerializeField] AudioClip equipUnequipSound;

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
        if (eventData.clickCount == 2 && itemType == ItemType.Normal && isEquippable)
        {
            Debug.Log("Equipped " + thisName);
            InventorySystem.instance.EquipItem(thisName, thisImage, equipUnequipSound, thisKeyLevel);
        }
        else if(eventData.clickCount == 2 && itemType == ItemType.Consumable)
        {
            Debug.Log("Double clicked on a consumable item!");
        }
        else if(eventData.clickCount == 2 && itemType == ItemType.Document)
        {
            Debug.Log("Double clicked on a document!");
        }
        else if(eventData.clickCount == 2 && itemType == ItemType.Equipment)
        {
            Debug.Log("Double clicked on an equipment item!");
        }
    }
}