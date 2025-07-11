using System;
using System.IO;
using UnityEngine;

public class SettingsManager : MonoBehaviour {
    private static SettingsManager instance;
    public static SettingsManager Instance {
        get {
            if (instance == null) {
                instance = FindFirstObjectByType<SettingsManager>();

                if (instance == null) {
                    GameObject obj = new GameObject("_SettingsManager");
                    instance = obj.AddComponent<SettingsManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    private const string SETTINGS_FILENAME = "settings.json";
    private string _filePath;
    private PlayerSettings _currentSettings;

    public PlayerSettings CurrentSettings => _currentSettings;
    public event Action OnSettingsChanged;

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        _filePath = Path.Combine(Application.persistentDataPath, SETTINGS_FILENAME);
        LoadSettings();
    }

    public void LoadSettings() {
        try {
            if (File.Exists(_filePath)) {
                string json = File.ReadAllText(_filePath);
                _currentSettings = JsonUtility.FromJson<PlayerSettings>(json);
                Debug.Log("[Settings] Loaded successfully");
            }
            else {
                ResetToDefaults();
            }
        }
        catch (Exception e) {
            Debug.LogError($"[Settings] Load failed: {e.Message}");
            ResetToDefaults();
        }

        ApplySettings();
    }

    public void SaveSettings() {
        if (_currentSettings == null) return;

        try {
            string json = JsonUtility.ToJson(_currentSettings, true);
            File.WriteAllText(_filePath, json);
            Debug.Log("[Settings] Saved successfully");
        }
        catch (Exception e) {
            Debug.LogError($"[Settings] Save failed: {e.Message}");
        }
    }

    public void ResetToDefaults() {
        _currentSettings = new PlayerSettings();
        Debug.Log("[Settings] Reset to defaults");
        SaveSettings();
    }

    public void ApplySettings() {
        if (_currentSettings == null) return;

        // Trigger event
        OnSettingsChanged?.Invoke();
    }
}

/* USAGE EXAMPLE
// Get settings
float vol = SettingsManager.Instance.CurrentSettings.musicVolume;

// Change settings
SettingsManager.Instance.CurrentSettings.musicVolume = 0.5f;
SettingsManager.Instance.SaveSettings();

// Listen for changes
void OnEnable() => SettingsManager.Instance.OnSettingsChanged += UpdateUI;
void OnDisable() => SettingsManager.Instance.OnSettingsChanged -= UpdateUI;
*/