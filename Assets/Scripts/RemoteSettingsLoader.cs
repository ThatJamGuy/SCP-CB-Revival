using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Purpose: Attatches to standalone objects not automatically referencable in the settings system and loads given data.
/// IE. Allows the camera to adjust it's anti-aliasing mode on start by loading data from the save file if it exists.
/// </summary>
public class RemoteSettingsLoader : MonoBehaviour {
    private enum SettingPreset { AntiAliasing, ShadowQuality };

    [SerializeField] private SettingPreset[] settingToRetreive;

    private HDAdditionalCameraData playerCameraData;
    private HDAdditionalLightData light;

    private void OnEnable() {
        if (!DataSaver.DataFileExists("settings.json")) return;

        foreach (SettingPreset setting in settingToRetreive) {
            int settingIndex = 0;

            switch (settingToRetreive[settingIndex]) {
                case SettingPreset.AntiAliasing:
                    playerCameraData = GetComponent<HDAdditionalCameraData>();
                    UpdateAntiAliasing(RevivalRuntimeEngine.SettingsData.antiAliasingMode);
                    break;
                case SettingPreset.ShadowQuality:
                    light = GetComponent<HDAdditionalLightData>();
                    UpdateShadowQuality(RevivalRuntimeEngine.SettingsData.shadowQualityMode);
                    break;
            }

            settingIndex++;
        }

        OptionsMenu.OnMinorGraphicsChanged.AddListener(UpdateAntiAliasing);
        OptionsMenu.OnMajorGraphicsChanged.AddListener(UpdateShadowQuality);
    }

    private void OnDisable() {
        OptionsMenu.OnMinorGraphicsChanged.RemoveListener(UpdateAntiAliasing);
        OptionsMenu.OnMajorGraphicsChanged.RemoveListener(UpdateShadowQuality);
    }

    private void UpdateAntiAliasing(int newData) {

        if (playerCameraData == null) return;

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

    private void UpdateShadowQuality(int newData) {
        if (light == null) return;

        switch (newData) {
            case 0: // High
                light.EnableShadows(true);
                light.SetShadowResolutionLevel((int)ShadowResolution.High);
                break;
            case 1: // Medium
                light.EnableShadows(true);
                light.SetShadowResolutionLevel((int)ShadowResolution.Medium);
                break;
            case 2: // Low
                light.EnableShadows(true);
                light.SetShadowResolutionLevel((int)ShadowResolution.Low);
                break;
            case 3: // None
                light.EnableShadows(false);
                break;
        }
    }
}