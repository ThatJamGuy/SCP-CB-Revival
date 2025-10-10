using System;

namespace scpcbr {
    [Serializable]
    public class PlayerSettings {
        // Audio
        public float masterVolume = 0f;
        public float musicVolume = 0f;
        public float sfxVolume = 0f;
        public float voiceVolume = 0f;
        public bool subtitlesEnabled = true;
        public int soundtrack = 0;

        // Graphics
        public int qualityLevel = 2;
        public bool vsyncEnabled = true;
        public bool showFPSCounter = false;
        public int displayMode = 0;
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;

        // Gameplay
        public float mouseSensitivity = 1f;
    }
}