using System;

[Serializable]
public class PlayerSettings
{
    // Audio
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public float voiceVolume = 1f;

    // Graphics
    //public int qualityLevel = 2;
    public bool fullscreen = true;
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;

    // Gameplay
    public float mouseSensitivity = 1f;
}