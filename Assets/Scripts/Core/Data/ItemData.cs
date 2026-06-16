using FMODUnity;
using UnityEngine;
using EditorAttributes;

[CreateAssetMenu(fileName = "ItemData", menuName = "SCP:CBR/Item Data")]
public class ItemData : ScriptableObject {
    [Header("Basic Info")] 
    public string itemName = "NullItemName";
    public string itemDescription = "NullItemDescription";
    public string itemIdentifier = "NullItemID";

    public Sprite itemInventoryIcon;
    [AssetPreview] public GameObject itemWorldPrefab;

    [Header("Audio")] 
    public EventReference useItemSound;
}