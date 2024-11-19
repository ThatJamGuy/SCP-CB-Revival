using UnityEngine;

public class IconData : MonoBehaviour
{
    public ItemData itemData;

    public void SetData(ItemData data)
    {
        itemData = data;
        Debug.Log($"Icon set for item: {itemData.itemName}");
    }
}