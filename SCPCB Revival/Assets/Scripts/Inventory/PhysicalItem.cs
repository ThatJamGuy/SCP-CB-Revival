using UnityEngine;

public class PhysicalItem : MonoBehaviour
{
    public string itemName;

    public void AddItemToInventory()
    {
        Debug.Log("Added " + itemName + " to inventory");
        Destroy(gameObject);
    }
}