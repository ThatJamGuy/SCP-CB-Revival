[System.Serializable]
public class SettingsData {
    public int resolutionWidth;
    public int resolutionHeight;
    public int windowMode;
    public int qualityLevel;
    public bool vSync;
    public int frameLimit;
    public int soundtrack;
    public bool console;
    public bool fpsCounter;
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public float voiceVolume = 1f;

    public SettingsData() {
        resolutionWidth = 1920;
        resolutionHeight = 1080;
        windowMode = 0;
        qualityLevel = 0;
        vSync = false;
        frameLimit = -1;
        soundtrack = 0;
        console = false;
        fpsCounter = false;
        masterVolume = 1f;
        musicVolume = 1f;
        sfxVolume = 1f;
        voiceVolume = 1f;
    }
}