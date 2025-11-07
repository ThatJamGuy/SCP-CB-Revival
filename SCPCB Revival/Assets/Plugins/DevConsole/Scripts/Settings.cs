using UnityEngine;
using SickDev.Utils;
using System.Reflection;

namespace SickDev.DevConsole {
    public class Settings : ScriptableObject {
        public enum AutoCompleteBehaviour { Disabled = 0, Manual, Auto}

        [SerializeField]
        int selectedTab;

        [EnumFlags(displayOptions = new string[] { "Log", "Warning", "Error", "Exception", "Assert" })]
        public LogLevel attachedLogLevel;
        public KeyCode openKey;
        public KeyCode autoCompleteKey;
        public KeyCode historyKey;

        public bool autoInstantiate;
        public bool dontDestroyOnLoad;
        public bool showTimeStamp;
        public bool optimizeForOnGUI;
        public bool groupIdenticalEntries;
        public bool useAndFiltering;
        public bool clearInputOnClose;
        public bool enableHistory;
        public bool autoCompleteWithEnter;
        public AutoCompleteBehaviour autoCompleteBehaviour;
        public string defaultTag;

        [Range(0, 1)]
        public float preferredHeight;
        public bool showOpenButton;
        public float scale;
        public Font font;
        public int fontSize;
        public float entriesSpacing;
        public float maxButtonSize;

        public Color mainColor;
        public Color secondaryColor;
        public Color inputTextColor;
        public Color entryDefaultColor;
        public Color warningColor;
        public Color errorColor;

        public float animationDuration;
        public AnimationCurve animationY;
        public AnimationCurve animationX;

        public Texture2D errorIcon;
        public Texture2D warningIcon;
        public Texture2D closeConsoleIcon;
        public Texture2D openConsoleIcon;
        public Texture2D deleteInputIcon;
        public Texture2D submitInputIcon;
        public Texture2D historyIcon;
        public Texture2D autoCompleteIcon;
        public Texture2D entryExpandedIcon;
        public Texture2D entryCollapsedIcon;
        public Texture2D stackTraceIcon;
        public Texture2D removeEntryIcon;
        public Texture2D closeWindowIcon;
        public Texture2D scrollUpIcon;
        public Texture2D scrollDownIcon;
        public Texture2D clearLogIcon;
        public Texture2D saveLogIcon;
        public Texture2D groupEntriesIcon;
        public Texture2D settingsIcon;
        public Texture2D toggleOnIcon;
        public Texture2D toggleOffIcon;
        public Texture2D sliderIcon;
        public Texture2D sliderThumbIcon;

        public BuiltInCommandsPreferences builtInCommands;

        public void CopyFrom(Settings settings) {
            FieldInfo[] fields = GetType().GetFields();
            for(int i = 0; i < fields.Length; i++)
                fields[i].SetValue(this, fields[i].GetValue(settings));
        }

        [System.Serializable]
        public class BuiltInCommandsPreferences {
            public bool analytics = true;
            public bool performanceReporting = true;
            public bool androidInput = true;
            public bool animator = true;
            public bool appleReplayKit = true;
            public bool appleTvRemote = true;
            public bool application = true;
            public bool audioListener = true;
            public bool audioSettings = true;
            public bool audioSource = true;
            public bool caching = true;
            public bool camera = true;
            public bool canvas = true;
            public bool color = true;
            public bool color32 = true;
            public bool colorUtility = true;
            public bool crashReport = true;
            public bool crashReportHandler = true;
            public bool cursor = true;
            public bool debug = true;
            public bool playerConnection = true;
            public bool display = true;
            public bool dynamicGI = true;
            public bool font = true;
            public bool gameObject = true;
            public bool hash128 = true;
            public bool handheld = true;
            public bool humanTrait = true;
            public bool input = true;
            public bool compass = true;
            public bool gyroscope = true;
            public bool locationService = true;
            public bool iOSDevice = true;
            public bool iOSNotificationServices = true;
            public bool iOSOnDemandResources = true;
            public bool layerMask = true;
            public bool lightmapSettings = true;
            public bool lightProbeProxyVolume = true;
            public bool lODGroup = true;
            public bool masterServer = true;
            public bool mathf = true;
            public bool microphone = true;
            public bool physics = true;
            public bool physics2D = true;
            public bool playerPrefs = true;
            public bool proceduralMaterial = true;
            public bool profiler = true;
            public bool qualitySettings = true;
            public bool quaternion = true;
            public bool random = true;
            public bool rect = true;
            public bool reflectionProbe = true;
            public bool remoteSettings = true;
            public bool graphicsSettings = true;
            public bool renderSettings = true;
            public bool samsungTV = true;
            public bool sceneManager = true;
            public bool sceneUtility = true;
            public bool screen = true;
            public bool shader = true;
            public bool sortingLayer = true;
            public bool systemInfo = true;
            public bool texture = true;
            public bool time = true;
            public bool touchScreenKeyboard = true;
            public bool vector2 = true;
            public bool vector3 = true;
            public bool vector4 = true;
            public bool vRInputTracking = true;
            public bool vRDevice = true;
            public bool vRSettings = true;
        }
    }

    public enum LogLevel {
        None = 0,
        Log = 1,
        Warning = 2,
        Error = 4,
        Exception = 8,
        Assert = 16
    }
}