using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Inventory Item Data")]
    public GameObject itemPrefab;

    public enum ItemType {Normal, Consumable, Document, Equipment};
    public ItemType itemType;

    // --- Item Info UI --- //
    private GameObject itemInfoUI;
    private TextMeshProUGUI itemInfoUI_itemName;

    public string thisName;

    // --- Equipping --- //
    private GameObject itemPendingEqip;
    public bool isEquippable;

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
}
