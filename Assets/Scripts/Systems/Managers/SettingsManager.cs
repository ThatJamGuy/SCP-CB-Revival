using UnityEngine;

public class SettingsManager : MonoBehaviour {
    private const string SETTINGS_FILE_NAME = "settings.json";

    public static SettingsData settingsData { get; private set; }

    private void Awake() {
        settingsData = DataSaver.Load<SettingsData>(SETTINGS_FILE_NAME);
    }

    private void SaveSettingsData() => DataSaver.Save<SettingsData>(settingsData, SETTINGS_FILE_NAME);
}