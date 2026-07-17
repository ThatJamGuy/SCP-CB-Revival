[System.Serializable]
public class SettingsData {
    // Default values for the settings JSON file
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public int windowMode;
    public int qualityLevel;
    public int globalTextureMipmapLimit = 0;
    public bool vSync = true;
    public int frameLimit = 120;
    public int soundtrack = 0;
    public bool consoleEnabled = false;
    public bool fpsCounter = false;
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public float voiceVolume = 1f;
    public float mouseSensitivity = 25f;
    public float mouseSmoothing = 95f;
    public int hudDesign = 0;
    public int hudFunctionality = 1;
}