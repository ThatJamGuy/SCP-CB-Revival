using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Linq;

namespace scpcbr {
    public class OptionsMenu : MonoBehaviour {
        [Header("Graphics Settings")]
        public Toggle vsyncToggle;
        public Toggle showFPSToggle;
        public TMP_Dropdown displayModeDropdown;
        public TMP_Dropdown resolutionDropdown;
        public TMP_Dropdown qualityDropdown;

        [Header("Audio Settings")]
        public AudioMixer audioMixer;
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public Slider voiceVolumeSlider;
        public Toggle subtitleToggle;

        [Header("Soundtrack")]
        public TMP_Dropdown soundtrackDropdown;

        private void Start() {
            PopulateResolutions();

            var s = SettingsManager.Instance.CurrentSettings;

            vsyncToggle.isOn = s.vsyncEnabled;
            showFPSToggle.isOn = s.showFPSCounter;
            displayModeDropdown.value = s.displayMode;
            displayModeDropdown.RefreshShownValue();
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.RefreshShownValue();

            string targetRes = $"{s.resolutionWidth}x{s.resolutionHeight}";
            int resIndex = resolutionDropdown.options.FindIndex(o => o.text == targetRes);
            resolutionDropdown.value = resIndex >= 0 ? resIndex : 0;
            resolutionDropdown.RefreshShownValue();

            audioMixer.SetFloat("MasterVolume", s.masterVolume);
            audioMixer.SetFloat("MusicVolume", s.musicVolume);
            audioMixer.SetFloat("SFXVolume", s.sfxVolume);
            audioMixer.SetFloat("VoiceVolume", s.voiceVolume);
            masterVolumeSlider.value = s.masterVolume;
            musicVolumeSlider.value = s.musicVolume;
            sfxVolumeSlider.value = s.sfxVolume;
            voiceVolumeSlider.value = s.voiceVolume;

            subtitleToggle.isOn = s.subtitlesEnabled;
            soundtrackDropdown.value = s.soundtrack;
            soundtrackDropdown.RefreshShownValue();
        }

        private void PopulateResolutions() {
            resolutionDropdown.ClearOptions();
            var options = Screen.resolutions
                .Select(r => $"{r.width}x{r.height}")
                .Distinct()
                .OrderByDescending(res => {
                    var parts = res.Split('x');
                    return int.Parse(parts[0]) * int.Parse(parts[1]);
                })
                .ToList();
            resolutionDropdown.AddOptions(options);
        }

        public void SetVSyncEnabled() {
            bool on = vsyncToggle.isOn;
            SettingsManager.Instance.CurrentSettings.vsyncEnabled = on;
            QualitySettings.vSyncCount = on ? 1 : 0;
        }

        public void SetShowFPSCounter() {
            SettingsManager.Instance.CurrentSettings.showFPSCounter = showFPSToggle.isOn;
        }

        public void SetResolution() {
            string[] parts = resolutionDropdown.options[resolutionDropdown.value].text.Split('x');
            int w = int.Parse(parts[0]);
            int h = int.Parse(parts[1]);
            SettingsManager.Instance.CurrentSettings.resolutionWidth = w;
            SettingsManager.Instance.CurrentSettings.resolutionHeight = h;
            Screen.SetResolution(w, h, Screen.fullScreenMode);
        }

        public void SetGraphicsQuality() {
            int qualityIndex = qualityDropdown.value;
            SettingsManager.Instance.CurrentSettings.qualityLevel = qualityIndex;
            QualitySettings.SetQualityLevel(qualityIndex, true);
        }

        public void SetDisplayMode() {
            int index = displayModeDropdown.value;
            SettingsManager.Instance.CurrentSettings.displayMode = index;
            Screen.fullScreenMode = index switch {
                0 => FullScreenMode.ExclusiveFullScreen,
                1 => FullScreenMode.FullScreenWindow,
                2 => FullScreenMode.MaximizedWindow,
                _ => FullScreenMode.Windowed
            };
        }

        public void SetMasterVolume() {
            float v = masterVolumeSlider.value;
            audioMixer.SetFloat("MasterVolume", v);
            SettingsManager.Instance.CurrentSettings.masterVolume = v;
        }

        public void SetMusicVolume() {
            float v = musicVolumeSlider.value;
            audioMixer.SetFloat("MusicVolume", v);
            SettingsManager.Instance.CurrentSettings.musicVolume = v;
        }

        public void SetSFXVolume() {
            float v = sfxVolumeSlider.value;
            audioMixer.SetFloat("SFXVolume", v);
            SettingsManager.Instance.CurrentSettings.sfxVolume = v;
        }

        public void SetVoiceVolume() {
            float v = voiceVolumeSlider.value;
            audioMixer.SetFloat("VoiceVolume", v);
            SettingsManager.Instance.CurrentSettings.voiceVolume = v;
        }

        public void SetSubtitlesEnabled() {
            SettingsManager.Instance.CurrentSettings.subtitlesEnabled = subtitleToggle.isOn;
        }

        public void SetSoundtrack() {
            int i = soundtrackDropdown.value;
            SettingsManager.Instance.CurrentSettings.soundtrack = i;
            MusicPlayer.Instance.SetCurrentSoundtrack(i);
        }

        public void ApplyAllSettings() {
            SettingsManager.Instance.SaveSettings();
            SettingsManager.Instance.ApplySettings();
        }
    }
}