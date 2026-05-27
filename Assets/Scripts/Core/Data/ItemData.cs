using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "SCP:CBR/Item Data")]
public class ItemData : ScriptableObject {
    [Header("Basic Info")] 
    public string itemIdentifier; // Internal name for the item. Will mainly be used for the console I think
    public Sprite itemInventoryIcon; // UI Sprite used to represent this item in the inventory
    public GameObject itemWorldPrefab; // World prefab for the item, need to know what to spawn in when dropping

    [Header("Audio")] 
    public EventReference useItemSound; // Sound to play when using the item. Either equipping or regular using
}