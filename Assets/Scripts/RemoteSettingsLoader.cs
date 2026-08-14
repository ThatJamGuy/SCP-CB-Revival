using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Purpose: Attatches to standalone objects not automatically referencable in the settings system and loads given data.
/// IE. Allows the camera to adjust it's anti-aliasing mode on start by loading data from the save file if it exists.
/// </summary>
public class RemoteSettingsLoader : MonoBehaviour {
    private enum SettingPreset { AntiAliasing };

    [SerializeField] private SettingPreset settingToRetreive;

    //private Camera playerCamera;
    private HDAdditionalCameraData playerCameraData;

    private void OnEnable() {
        OptionsMenu.OnMinorGraphicsChanged.AddListener(UpdateAntiAliasing);
    }

    private void OnDisable() {
        OptionsMenu.OnMinorGraphicsChanged.RemoveListener(UpdateAntiAliasing);
    }

    private void Start() {
        if (!DataSaver.DataFileExists("settings.json")) return;

        switch (settingToRetreive) {
            case SettingPreset.AntiAliasing:
                playerCameraData = GetComponent<HDAdditionalCameraData>();
                UpdateAntiAliasing(RevivalRuntimeEngine.SettingsData.antiAliasingMode);
                break;
        }
    }

    private void UpdateAntiAliasing(int newData) {
        switch (newData) {
            case 0:
                playerCameraData.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
                break;
            case 1:
                playerCameraData.antialiasing = HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;
                break;
            case 2:
                playerCameraData.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
                break;
            case 3:
                playerCameraData.antialiasing = HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                break;
        }
    }
}