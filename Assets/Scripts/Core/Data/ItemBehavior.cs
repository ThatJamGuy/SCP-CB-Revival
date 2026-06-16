using UnityEngine;

public abstract class ItemBehavior : ScriptableObject {
    public abstract void OnDoubleClick(ItemData itemData);
}