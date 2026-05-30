using FMODUnity;
using UnityEngine;
using EditorAttributes;

/// <summary>
/// Item data scriptable object to handle, well, item data. Keeps track of important stuff other scripts need to know
/// </summary>
[CreateAssetMenu(fileName = "ItemData", menuName = "SCP:CBR/Item Data")]
public class ItemData : ScriptableObject {
    [Header("Basic Info")] 
    public string itemName = "NullItemName"; // Name for the item, probably used exclusively for the tooltip
    public string itemDescription = "NullItemDescription"; // Description of the item, leave blank for just the name
    public string itemIdentifier = "NullItemID"; // Internal name for the item. Will mainly be used for the console I think
    public Sprite itemInventoryIcon; // UI Sprite used to represent this item in the inventory
    [AssetPreview] public GameObject itemWorldPrefab; // World prefab for the item, need to know what to spawn in when dropping

    [Header("Audio")] 
    public EventReference useItemSound; // Sound to play when using the item. Either equipping or regular using
}