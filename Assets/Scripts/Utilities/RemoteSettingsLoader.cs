using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Purpose: Attatches to standalone objects not automatically referencable in the settings system and loads given data.
/// IE. Allows the camera to adjust it's anti-aliasing mode on start by loading data from the save file if it exists.
/// </summary>
public class RemoteSettingsLoader : MonoBehaviour {
    private enum SettingPreset { AntiAliasing, ShadowQuality, SSR, SSAO };

    [SerializeField] private SettingPreset[] settingToRetreive;

    private HDAdditionalCameraData playerCameraData;
    private HDAdditionalLightData lightData;
    private ScreenSpaceReflection ssr;
    private ScreenSpaceAmbientOcclusion ssao;

    private bool hasSettingsFile;

    #region Unity Lifecycle

    private void Awake() {
        for (int i = 0; i < settingToRetreive.Length; i++) {
            switch (settingToRetreive[i]) {
                case SettingPreset.AntiAliasing:
                    playerCameraData = GetComponent<HDAdditionalCameraData>();
                    break;
                case SettingPreset.ShadowQuality:
                    lightData = GetComponent<HDAdditionalLightData>();
                    break;
                case SettingPreset.SSR:
                case SettingPreset.SSAO:
                    CacheVolumeOverrides();
                    break;
            }
        }
    }

    private void OnEnable() {
        hasSettingsFile = DataSaver.DataFileExists("settings.json");

        if (hasSettingsFile) {
            for (int i = 0; i < settingToRetreive.Length; i++) {
                switch (settingToRetreive[i]) {
                    case SettingPreset.AntiAliasing:
                        UpdateAntiAliasing(RevivalRuntimeEngine.SettingsData.antiAliasingMode);
                        break;
                    case SettingPreset.ShadowQuality:
                        if (RevivalRuntimeEngine.Instance != null)
                            UpdateShadowQuality(RevivalRuntimeEngine.SettingsData.shadowQualityMode);
                        break;
                    case SettingPreset.SSR:
                        UpdateSSR(RevivalRuntimeEngine.SettingsData.enableSSR);
                        break;
                    case SettingPreset.SSAO:
                        UpdateSSAO(RevivalRuntimeEngine.SettingsData.enableSSAO);
                        break;
                }
            }
        }

        OptionsMenu.OnMinorGraphicsChanged.AddListener(UpdateAntiAliasing);
        OptionsMenu.OnMajorGraphicsChanged.AddListener(UpdateShadowQuality);
        OptionsMenu.OnSSRChanged.AddListener(UpdateSSR);
        OptionsMenu.OnSSAOChanged.AddListener(UpdateSSAO);
    }

    private void OnDisable() {
        OptionsMenu.OnMinorGraphicsChanged.RemoveListener(UpdateAntiAliasing);
        OptionsMenu.OnMajorGraphicsChanged.RemoveListener(UpdateShadowQuality);
        OptionsMenu.OnSSRChanged.RemoveListener(UpdateSSR);
        OptionsMenu.OnSSAOChanged.RemoveListener(UpdateSSAO);
    }

    #endregion

    #region Private Methods

    private void CacheVolumeOverrides() {
        if (ssr != null || ssao != null) return;

        Volume volume = GetComponent<Volume>();
        if (volume == null || volume.profile == null) return;

        volume.profile.TryGet(out ssr);
        volume.profile.TryGet(out ssao);
    }

    private void UpdateAntiAliasing(int newData) {
        if (playerCameraData == null || (uint)newData >= (uint)AAModes.Length) return;
        playerCameraData.antialiasing = AAModes[newData];
    }

    private void UpdateShadowQuality(int newData) {
        if (lightData == null) return;

        switch (newData) {
            case 0:
                lightData.EnableShadows(true);
                lightData.SetShadowResolutionLevel((int)ShadowResolution.High);
                break;
            case 1:
                lightData.EnableShadows(true);
                lightData.SetShadowResolutionLevel((int)ShadowResolution.Medium);
                break;
            case 2:
                lightData.EnableShadows(true);
                lightData.SetShadowResolutionLevel((int)ShadowResolution.Low);
                break;
            case 3:
                lightData.EnableShadows(false);
                break;
        }
    }

    private void UpdateSSR(bool enabled) {
        if (ssr != null) ssr.active = enabled;
    }

    private void UpdateSSAO(bool enabled) {
        if (ssao != null) ssao.active = enabled;
    }

    #endregion

    private static readonly HDAdditionalCameraData.AntialiasingMode[] AAModes = {
        HDAdditionalCameraData.AntialiasingMode.None,
        HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing,
        HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing,
        HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing
    };
}