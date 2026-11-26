[System.Serializable]
public class SettingsData {
    // Default values for the settings JSON file
    public int renderingAPI = 0; // 0 - Dx12, 1 - Dx11, 2 - Vulkan
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public int windowMode;
    public int qualityLevel;
    public int globalTextureMipmapLimit = 0;
    public int antiAliasingMode; // 0 - None, 1 - FXAA, 2 - TAA, 3 - SMAA
    public int shadowQualityMode = 1; // 0 - High, 1 - Medium, 2 - Low, 3 - None
    public bool vSync = true;
    public int frameLimit = 60;
    public float brightness = 0; // 0 - Default factory brightness (What I see when making the game)
    public bool enableSSR = true;
    public bool enableSSAO = false;

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