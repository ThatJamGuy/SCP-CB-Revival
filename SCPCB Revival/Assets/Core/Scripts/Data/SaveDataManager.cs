using UnityEngine;
using System.IO;
using System;

[Serializable]
public class SaveData {
    public string saveName = "SaveGame";
    public string mapSeed = "";
    public bool enableIntro = true;
    public DateTime saveTime;
    public float playtime;
    public int gameVersion = 1;
}

public class SaveDataManager : MonoBehaviour {
    private static SaveDataManager instance;
    public static SaveDataManager Instance {
        get {
            if (instance == null) {
                instance = FindFirstObjectByType<SaveDataManager>();
                if (instance == null) {
                    GameObject obj = new GameObject("_SaveDataManager");
                    instance = obj.AddComponent<SaveDataManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    private const string SAVE_FILENAME = "savegame.json";
    private string FilePath => Path.Combine(Application.persistentDataPath, SAVE_FILENAME);

    public SaveData CurrentSave { get; private set; }
    public bool HasSaveData => File.Exists(FilePath);

    void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void CreateSaveData(string saveName = "New Game", string seed = "", bool intro = true) {
        CurrentSave = new SaveData {
            saveName = saveName,
            mapSeed = seed,
            enableIntro = intro,
            saveTime = DateTime.Now,
            playtime = 0f
        };
    }

    public bool SaveGame() {
        if (CurrentSave == null) {
            Debug.LogError("No save data to write");
            return false;
        }

        try {
            CurrentSave.saveTime = DateTime.Now;
            string json = JsonUtility.ToJson(CurrentSave, true);
            File.WriteAllText(FilePath, json);
            return true;
        }
        catch (Exception e) {
            Debug.LogError($"Save failed: {e.Message}");
            return false;
        }
    }

    public bool LoadSaveData() {
        if (!HasSaveData) return false;

        try {
            string json = File.ReadAllText(FilePath);
            CurrentSave = JsonUtility.FromJson<SaveData>(json);
            return CurrentSave != null;
        }
        catch (Exception e) {
            Debug.LogError($"Load failed: {e.Message}");
            return false;
        }
    }

    public bool DeleteSaveData() {
        try {
            if (HasSaveData) {
                File.Delete(FilePath);
                CurrentSave = null;
                return true;
            }
            return false;
        }
        catch (Exception e) {
            Debug.LogError($"Delete failed: {e.Message}");
            return false;
        }
    }

    public T GetSaveValue<T>(Func<SaveData, T> selector, T defaultValue = default) {
        return CurrentSave != null ? selector(CurrentSave) : defaultValue;
    }

    public string GetSaveName() => GetSaveValue(s => s.saveName, "No Save");
    public string GetMapSeed() => GetSaveValue(s => s.mapSeed, "");
    public bool GetIntroEnabled() => GetSaveValue(s => s.enableIntro, true);
}