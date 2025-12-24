using UnityEngine;
using System.Collections.Generic;

public static class ItemDatabase {
    static readonly Dictionary<string, ItemData> items = new();

    public static void Register(ItemData item) {
        var key = item.itemName.ToLower();
        if (items.ContainsKey(key)) {
            Debug.LogError($"Duplicate ItemData id: {item.itemName}");
            return;
        }
        items[key] = item;
    }

    public static bool TryGet(string id, out ItemData item) {
        return items.TryGetValue(id.ToLower(), out item);
    }
}
