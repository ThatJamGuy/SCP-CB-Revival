using UnityEngine;

public class SettingsManager : MonoBehaviour {
    public static SettingsData settingsData { get; set; }

    private const string SETTINGS_FILE_NAME = "settings.json";

    private void Start() {
        settingsData = DataSaver.Load<SettingsData>(SETTINGS_FILE_NAME);
    }

    public static void SaveSettingsData() => DataSaver.Save<SettingsData>(settingsData, SETTINGS_FILE_NAME);
}