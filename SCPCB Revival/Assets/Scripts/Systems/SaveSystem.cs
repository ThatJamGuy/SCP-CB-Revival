using UnityEngine;
using System.IO;

public static class SaveSystem {
    // Saves data to the file
    public static void Save<T>(T data, string fileName) { 
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(fullPath, json);

        Debug.Log($"Save Data saved to {fullPath}");
    }

    // Loads data from the file
    public static T Load<T>(string fileName) where T : new() {
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(fullPath)) {
            string json = File.ReadAllText(fullPath);
            return JsonUtility.FromJson<T>(json);
        } else {
            Debug.LogWarning("Save file not found, creating a new instance now :/");
            return new T();
        }
    }

    // Helper to check if the save even exists
    public static bool SaveFileExists(string fileName) {
        return File.Exists(Path.Combine(Application.persistentDataPath, fileName));
    }
}