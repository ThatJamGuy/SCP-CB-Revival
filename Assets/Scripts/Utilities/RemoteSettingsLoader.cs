using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Purpose: Attatches to standalone objects not automatically referencable in the settings system and loads given data.
/// IE. Allows the camera to adjust it's anti-aliasing mode on start by loading data from the save file if it exists.
/// </summary>
public class RemoteSettingsLoader : MonoBehaviour {
    private enum SettingPreset { AntiAliasing, ShadowQuality, Brightness, Bloom, SSR, SSAO };

    [SerializeField] private SettingPreset[] settingToRetreive;

    private HDAdditionalCameraData playerCameraData;
    private HDAdditionalLightData lightData;
    private ColorAdjustments colAdj;
    private Bloom bloom;
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
                case SettingPreset.Brightness:
                case SettingPreset.Bloom:
                case SettingPreset.SSR:
                case SettingPreset.SSAO:
                    CacheVolumeOverrides();
                    break;
            }
        }
    }

    private void OnEnable() {
        hasSettingsFile = DataSaver.DataFileExists("settings.json");

        // Applies avaiable settings to make sure it updates automatically when loading into a scene
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
                    case SettingPreset.Brightness:
                        UpdateBrightness(RevivalRuntimeEngine.SettingsData.brightness);
                        break;
                    case SettingPreset.Bloom:
                        UpdateBloom(RevivalRuntimeEngine.SettingsData.enableBloom);
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
        OptionsMenu.OnBrightnessChanged.AddListener(UpdateBrightness);
        OptionsMenu.OnBloomChanged.AddListener(UpdateBloom);
        OptionsMenu.OnSSRChanged.AddListener(UpdateSSR);
        OptionsMenu.OnSSAOChanged.AddListener(UpdateSSAO);
    }

    private void OnDisable() {
        OptionsMenu.OnMinorGraphicsChanged.RemoveListener(UpdateAntiAliasing);
        OptionsMenu.OnMajorGraphicsChanged.RemoveListener(UpdateShadowQuality);
        OptionsMenu.OnBrightnessChanged.RemoveListener(UpdateBrightness);
        OptionsMenu.OnBloomChanged.RemoveListener(UpdateBloom);
        OptionsMenu.OnSSRChanged.RemoveListener(UpdateSSR);
        OptionsMenu.OnSSAOChanged.RemoveListener(UpdateSSAO);
    }

    #endregion

    #region Private Methods

    private void CacheVolumeOverrides() {
        // If we already have everything cached no need to do it again
        //if (ssr != null && ssao != null && bloom != null && colAdj != null) return;

        Volume volume = GetComponent<Volume>();
        if (volume == null || volume.profile == null) return;

        volume.profile.TryGet(out colAdj);
        volume.profile.TryGet(out bloom);
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

    private void UpdateBrightness(float value) {
        if (colAdj != null) colAdj.postExposure.value = value;
    }

    // Turns out bloom wasn't doing much, so using the intensity value at 0
    // and turning bloom on will have the opposite effect, as without that setup
    // turning bloom off didn't do anything
    private void UpdateBloom(bool enabled) {
        if (bloom != null) bloom.active = !enabled;
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