using UnityEngine;

public abstract class ItemAction : ScriptableObject {
    public abstract void EquipItem(ItemData itemData);
}