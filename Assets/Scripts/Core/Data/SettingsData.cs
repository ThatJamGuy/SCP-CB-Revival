[System.Serializable]
public class SettingsData {
    // Default values for the settings JSON file
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public int windowMode;
    public int qualityLevel;
    public bool vSync;
    public int frameLimit = -1;
    public int soundtrack = 2;
    public bool console;
    public bool fpsCounter;
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public float voiceVolume = 1f;
    public float mouseSensitivity = 25f;
    public float mouseSmoothing = 95f;
}