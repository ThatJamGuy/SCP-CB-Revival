using System.IO;
using UnityEngine;

/// <summary>
/// Global system to save data to a JSON file because PlayerPrefs is a mess
/// </summary>
public static class DataSaver {
    // Saves data to a file
    public static void Save<T>(T data, string fileName) {
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(fullPath, json);

        Debug.Log("Saved data to " + fullPath);
    }

    // Loads data from a file
    public static T Load<T>(string fileName) where T : new()
    {
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError($"Load<{typeof(T).Name}>: fileName is null or empty, returning default data.");
            return new T();
        }

        string fullPath = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            T loaded = string.IsNullOrWhiteSpace(json) ? default : JsonUtility.FromJson<T>(json);

            if (loaded != null)
                return loaded;

            Debug.LogWarning($"Load<{typeof(T).Name}>: {fileName} was empty or corrupt, creating a fresh one...");
        }
        else
        {
            Debug.LogWarning("No existing file could be found to load this data from, creating one now...");
        }

        T defaultData = new T();
        Save(defaultData, fileName);
        return defaultData;
    }

    // Helper to check if the file we want to load from exists
    public static bool DataFileExists(string fileName) {
        return File.Exists(Path.Combine(Application.persistentDataPath, fileName));
    }
}