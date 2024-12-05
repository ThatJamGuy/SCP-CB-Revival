using UnityEngine;

public class PhysicalItem : MonoBehaviour
{
    public string itemName;

    [SerializeField] private AudioClip itemPickup;

    public void AddItemToInventory()
    {
        if (!InventorySystem.instance.CheckIfFull())
        {
            InventorySystem.instance.AddToInventory(itemName);
            AudioSource.PlayClipAtPoint(itemPickup, transform.position, 1);

            Debug.Log("Added " + itemName + " to inventory");
            Destroy(gameObject);
        }
        else
        {
            InfoTextManager.Instance.NotifyPlayer("Cannot pick up item; inventory is full.");
            Debug.Log("Inventory is full!");
        }
    }
}