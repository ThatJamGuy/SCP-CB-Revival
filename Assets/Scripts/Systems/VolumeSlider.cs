using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour {
    private enum VolumeType { MASTER, MUSIC, SFX, VOICE }

    [SerializeField] private OptionsMenu optionsMenu;

    [Header("Type")]
    [SerializeField] private VolumeType volumeType;

    private Slider volumeSlider;

    private void Awake() {
        volumeSlider = GetComponentInChildren<Slider>();
    }

    private void Update() {
        switch (volumeType) {
            case VolumeType.MASTER:
                volumeSlider.value = AudioManager.instance.masterVolume;
                break;
            case VolumeType.MUSIC:
                volumeSlider.value = AudioManager.instance.musicVolume;
                break;
            case VolumeType.SFX:
                volumeSlider.value = AudioManager.instance.SFXVolume;
                break;
            case VolumeType.VOICE:
                volumeSlider.value = AudioManager.instance.voiceVolume;
                break;
            default:
                Debug.LogWarning("Volume Type not supported: " + volumeType);
                break;
        }
    }

    public void OnSliderValueChanged() {
        switch (volumeType) {
            case VolumeType.MASTER:
                optionsMenu.SetMasterVolume(volumeSlider.value);
                break;
            case VolumeType.MUSIC:
                optionsMenu.SetMusicVolume(volumeSlider.value);
                break;
            case VolumeType.SFX:
                optionsMenu.SetSfxVolume(volumeSlider.value);
                break;
            case VolumeType.VOICE:
                optionsMenu.SetVoiceVolume(volumeSlider.value);
                break;
            default:
                Debug.LogWarning("Volume Type not supported: " + volumeType);
                break;
        }
    }
}