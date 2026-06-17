using FMODUnity;
using UnityEngine;
using EditorAttributes;

public enum PresetBehavior {
    None, // Generic item that does nothing on use, such as batteries
    Key, // Key based items like keycards or severed hands
    Document, // Readable document sprites
    Custom // Custom behaviors for more advanced and less common items, like the gas mask and its upgrades
}

[CreateAssetMenu(fileName = "ItemData", menuName = "SCP:CBR/Item Data")]
public class ItemData : ScriptableObject {
    [Header("Basic Info")] 
    public string itemName = "NullItemName";
    public string itemDescription = "NullItemDescription";
    public string itemIdentifier = "NullItemID";

    public Sprite itemInventoryIcon;
    [AssetPreview] public GameObject itemWorldPrefab;

    [Header("Chosen Behavior")]
    public PresetBehavior chosenBehavior;

    // Reusable key settings
    [ShowField(nameof(chosenBehavior), PresetBehavior.Key)] public bool openAllCardDoors;
    [ShowField(nameof(chosenBehavior), PresetBehavior.Key)] public int clearance;

    [ShowField(nameof(chosenBehavior), PresetBehavior.Document)] public Sprite documentDisplaySprite;
    [ShowField(nameof(chosenBehavior), PresetBehavior.Custom)] public ItemBehavior itemBehavior;

    [Header("Audio")] 
    public EventReference useItemSound;
}