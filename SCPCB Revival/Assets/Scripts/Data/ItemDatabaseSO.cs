using UnityEngine;

[CreateAssetMenu(menuName = "SCPCBR/Item Database")]
public class ItemDatabaseSO : ScriptableObject {
    public ItemData[] items;

    public void Init() {
        foreach (var item in items)
            ItemDatabase.Register(item);
    }
}