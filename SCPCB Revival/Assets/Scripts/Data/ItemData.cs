using UnityEngine;
using EditorAttributes;

[CreateAssetMenu(fileName = "NewItem", menuName = "SCPCBR/Item Data")]
public class ItemData : ScriptableObject {
    public enum Type { Normal, Keycard, Consumable, Document, Equipment }

    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    [AssetPreview] public GameObject worldPrefab;

    [Header("Type & Properties")]
    public Type itemType;
    public bool isEquippable;
    public int keyLevel;
}