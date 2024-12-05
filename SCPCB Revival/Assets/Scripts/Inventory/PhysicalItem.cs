using UnityEngine;

public class PhysicalItem : MonoBehaviour
{
    public string itemName;

    public void AddItemToInventory()
    {
        if (!InventorySystem.instance.CheckIfFull())
        {
            InventorySystem.instance.AddToInventory(itemName);

            Debug.Log("Added " + itemName + " to inventory");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Inventory is full!");
        }
    }
}