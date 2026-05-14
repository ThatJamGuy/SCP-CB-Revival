using UnityEngine;
using System.IO;

/// <summary>
/// Global system to save data to a JSON file because PlayerPrefs is a mess
/// </summary>
public static class DataSaver {
    // Saves data to a file
    public static void Save<T>(T data, string fileName) {
        var fullPath = Path.Combine(Application.persistentDataPath, fileName);
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(fullPath, json);
        
        Debug.Log("Saved data to " + fullPath);
    }

    // Loads data from a file
    public static T Load<T>( string fileName) where T : new() {
        var fullPath = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(fullPath))
            return JsonUtility.FromJson<T>(File.ReadAllText(fullPath));
        
        Debug.LogWarning("No existing file could be found to load this data from, creating one now...");
        var defaultData = new T();
        Save(defaultData, fileName);
        return defaultData;
    }
    
    // Helper to check if the file we want to load from exists
    public static bool DataFileExists(string fileName) {
        return File.Exists(Path.Combine(Application.persistentDataPath, fileName));
    }
}