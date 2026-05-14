using System;
using UnityEngine;

public class SettingsManager : MonoBehaviour {
    private const string SETTINGS_FILE_NAME = "settings.json";
    
    private SettingsData settingsData;

    private void Start() {
        settingsData = DataSaver.Load<SettingsData>(SETTINGS_FILE_NAME);
    }
    
    private void SaveSettingsData() => DataSaver.Save<SettingsData>(settingsData, SETTINGS_FILE_NAME);
}