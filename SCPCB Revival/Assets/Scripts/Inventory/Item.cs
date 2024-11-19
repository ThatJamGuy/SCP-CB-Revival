using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemData itemData;

    public void Interact()
    {
        if (Inventory.Instance.AddItem(itemData))
        {
            if (itemData.pickUpSound != null)
                AudioSource.PlayClipAtPoint(itemData.pickUpSound, transform.position);

            Debug.Log($"Picked up: {itemData.itemName}");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Cannot pick up item; inventory is full.");
            InfoTextManager.Instance.NotifyPlayer("Cannot pick up item; inventory is full.");
        }
    }
}