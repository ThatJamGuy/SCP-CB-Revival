using System;
using System.Text;
using System.Reflection;
using System.Collections;
using SickDev.CommandSystem;
using UnityEngine;
using UnityEngine.UI;

namespace SickDev.CommandSystem {
    public class DevConsole {
        public static SickDev.DevConsole.DevConsole singleton { get { return SickDev.DevConsole.DevConsole.singleton; } }
    }
}

namespace SickDev.DevConsole{
    public class DevConsole : MonoBehaviour, ISerializationCallbackReceiver {
        public delegate void OnOpenStateChanged(bool isOpen);

        [SerializeField]
        Toolbar toolbar;
        [SerializeField]
        Logger logger;
        [SerializeField]
        ConsoleInput _input;
        [SerializeField]
        OpenButton _openButton;
        [SerializeField]
        History _history;
        [SerializeField]
        AutoComplete _autoComplete;
        [SerializeField]
        SettingsPanel settingsPanel;

        [NonSerialized]
        bool initialized;
        bool open;
        Coroutine openCoroutine;
        Vector2 position;
        int lastFrame;
        bool mouseDragEventProcessed;
        float lastScreenHeight = Screen.height;
        Canvas canvas;
        Image windowBlocker;
        Image buttonBlocker;
        Settings serializedSettings;

        public event OnOpenStateChanged onOpenStateChanged;

        public bool isOpen { get { return openCoroutine != null || open; } }
        public float height { get { return (Mathf.Clamp(settings.preferredHeight, 0, 1 - openButton.height * settings.scale / Screen.height) * Screen.height) / settings.scale; } }
        public ConsoleInput input { get { return _input; } }
        public History history { get { return _history; } }
        public AutoComplete autoComplete { get { return _autoComplete; } }
        public bool isSettingsOpen { get; private set; }
        public OpenButton openButton { get { return _openButton; } }

        static DevConsole _singleton;
        public static DevConsole singleton {
            get {
                if(_singleton == null)
                    Instantiate();
                return _singleton;
            }
        }

        static Settings settingsCopy;
        static Settings _settings;
        public static Settings settings {
            get {
                if(_settings == null) {
                    _settings = Resources.Load<Settings>("DevConsoleSettings");
                    settingsCopy = Instantiate(settings);
                }
                return _settings;
            }
        }

        CommandsManager _commandsManager;
        public CommandsManager commandsManager {
            get {
                if(_commandsManager == null)
                    _commandsManager = CreateCommandsManager();
                return _commandsManager;
            }
        }

        [RuntimeInitializeOnLoadMethod(loadType: RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializeOnLoad() {
            if(settings.autoInstantiate)
                Instantiate();
        }

        static void Instantiate() {
            _singleton = FindObjectOfType<DevConsole>();
            if(_singleton == null)
                _singleton = Instantiate(Resources.Load<DevConsole>("DevConsole"));
        }

        public void OnBeforeSerialize() {
            if(Application.isPlaying)
                serializedSettings = settingsCopy;
        }

        public void OnAfterDeserialize() { }

        void Awake() {
            if(singleton != this) {
                Debug.LogWarning("There can only be one Console per project");
                DestroyImmediate(gameObject);
                return;
            }
            if(settings.dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
            useGUILayout = false;
            openCoroutine = null;
            EvaluatePosition(0);
            CreateCanvas();
        }

        void OnEnable() {
            Application.logMessageReceived += OnLogMessageReceived;
        }

        void OnDisable() {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        void OnDestroy() {
            settings.CopyFrom(settingsCopy);
        }

        void OnLogMessageReceived(string condition, string stackTrace, LogType type) {
            LogLevel level = (LogLevel)Enum.Parse(typeof(LogLevel), type.ToString());
            if((settings.attachedLogLevel & level) == level) {
                EntryData entry = new EntryData(condition, level.ToString());
                entry.stackTrace = stackTrace;

                if(level == LogLevel.Exception || level == LogLevel.Error)
                    LogError(entry);
                else if(level == LogLevel.Warning)
                    LogWarning(entry);
                else
                    Log(entry);
            }
        }

        CommandsManager CreateCommandsManager() {
            CommandsManager.onExceptionThrown += OnCommandSystemExceptionThrown;
            CommandsManager.onMessage += OnCommandSystemMessage;
            Configuration configuration = new Configuration(
                Application.platform != RuntimePlatform.WebGLPlayer,
                "Assembly-CSharp-firstpass",
                "Assembly-CSharp"
            );
            CommandsManager commandsManager = new CommandsManager(configuration);
            commandsManager.LoadCommands();
            new BuiltInCommandsBuilder(commandsManager).Build();
            return commandsManager;
        }

        void OnCommandSystemExceptionThrown(Exception exception) {
            EntryData entry = new EntryData();
            if (exception is TargetInvocationException)
                exception = exception.InnerException;
            entry.text = string.Format("{0}: {1}", exception.GetType().Name, exception.Message);
            entry.stackTrace = exception.StackTrace;
            LogError(entry);
        }

        void OnCommandSystemMessage(string message) {
            EntryData entry = new EntryData();
            entry.text = message;
            entry.stackTrace = StackTraceUtility.ExtractStackTrace();
            Log(entry);
        }

        void CreateCanvas() {
            canvas = gameObject.AddComponent<Canvas>();
            gameObject.AddComponent<GraphicRaycaster>();
            StartCoroutine(CreateBlockers());
        }

        IEnumerator CreateBlockers() {
            yield return null;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            windowBlocker = new GameObject("Window Blocker").AddComponent<Image>();
            windowBlocker.transform.SetParent(canvas.transform);
            windowBlocker.color = Color.clear;
            windowBlocker.rectTransform.pivot = new Vector2(0, 1);
            windowBlocker.rectTransform.anchorMin = new Vector2(0, 1);
            windowBlocker.rectTransform.anchorMax = new Vector2(1, 1);

            buttonBlocker = new GameObject("Button Blocker").AddComponent<Image>();
            buttonBlocker.transform.SetParent(canvas.transform);
            buttonBlocker.color = Color.clear;
            buttonBlocker.rectTransform.pivot = new Vector2(0.5f, 1);
            buttonBlocker.rectTransform.anchorMin = new Vector2(0.5f, 1);
            buttonBlocker.rectTransform.anchorMax = new Vector2(0.5f, 1);
            yield return null;
            windowBlocker.rectTransform.anchoredPosition = Vector2.zero;
            RepositionBlockers(0);
        }

        public void Open() {
            if(!CanSetOpenState(true))
                return;
            SetOpenState(true);
            input.Focus();
        }

        public void Close() {
            if(!CanSetOpenState(false))
                return;
            SetOpenState(false);
            history.Close();
            autoComplete.Close();
        }

        public void ToggleOpen() {
            if(open)
                Close();
            else
                Open();
        }

        bool CanSetOpenState(bool open) {
            return this.open != open && openCoroutine == null;
        }

        void SetOpenState(bool open) {
            this.open = open;
            openCoroutine = StartCoroutine(OpenCoroutine());
            if (onOpenStateChanged != null)
                onOpenStateChanged(open);
        }

        IEnumerator OpenCoroutine() {
            if(openCoroutine != null)
                yield break;

            float time = 0;
            do {
                time = Mathf.Clamp(time + Time.unscaledDeltaTime, 0, settings.animationDuration);
                float evaluateTime = time / settings.animationDuration;
                if(!open)
                    evaluateTime = 1 - evaluateTime;
                EvaluatePosition(evaluateTime);
                RepositionBlockers(evaluateTime);
                yield return null;
            } while(time < settings.animationDuration);

            openCoroutine = null;
            if (settings.clearInputOnClose)
                input.Clear();
        }

        void EvaluatePosition(float percentage) {
            position = new Vector2(Screen.width * (settings.animationX.Evaluate(percentage) - 1),
                (height * settings.scale) * (settings.animationY.Evaluate(percentage) - 1));
        }

        void RepositionBlockers(float percentage) {
            if(windowBlocker == null)
                return;
            float windowBlockerHeight = height * settings.scale * percentage;
            windowBlocker.rectTransform.sizeDelta = new Vector2(0, windowBlockerHeight);
            buttonBlocker.rectTransform.anchoredPosition = new Vector2(0, -windowBlockerHeight);
        }

        void OnGUI() {
            if(ShouldSkipEvent())
                return;
            if(Event.current.type == EventType.Repaint)
                Reposition();

            if(!initialized)
                Initialize();

            DetectInput();
            ApplyScaleAndDraw();
            useGUILayout = history.isOpen || autoComplete.isOpen;
        }

        bool ShouldSkipEvent() {
            if(Event.current.type != EventType.MouseDrag)
                return false;

            if(lastFrame != Time.frameCount) {
                lastFrame = Time.frameCount;
                mouseDragEventProcessed = false;
            }
            if(mouseDragEventProcessed)
                return true;
            mouseDragEventProcessed = true;
            return false;
        }

        void Reposition() {
            if(Screen.height != lastScreenHeight) {
                if(openCoroutine == null && !isOpen)
                    EvaluatePosition(0);
                lastScreenHeight = Screen.height;
            }
        }

        void Initialize() {
            Slider.Initialize();
            Toggle.Initialize();
            settingsPanel.Initialize();
            Tab.Initialize();
            toolbar.Initialize(logger);
            logger.Initialize();
            input.Initialize();
            ScrollBar.Initialize();
            history.Initialize();
            autoComplete.Initialize();
            initialized = true;
            if(serializedSettings != null)
                settingsCopy.CopyFrom(serializedSettings);
        }

        void DetectInput() {
            Event e = Event.current;

            //Prevent tabbing
            if(e.type == EventType.Layout || e.type == EventType.Repaint)
                return;
            else if(e.character == '\t')
                e.Use();
            else if(e.type == EventType.KeyDown) {
                if(e.keyCode == settings.openKey)
                    ToggleOpen();
                else {
                    if(!isOpen)
                        return;
                    if (e.keyCode == KeyCode.Return) {
                        if (autoComplete.isOpen) {
                            if (autoComplete.hasMatches && settings.autoCompleteWithEnter) {
                                autoComplete.SelectCurrent();
                                e.Use();
                            }
                            else
                                SubmitInputText();
                        }
                        else
                            SubmitInputText();
                    }
                    else if (e.keyCode == KeyCode.Tab) {
                        if (autoComplete.isOpen) {
                            if (autoComplete.hasMatches) {
                                autoComplete.SelectCurrent();
                                e.Use();
                            }
                        }
                    }
                    else if (e.keyCode == KeyCode.DownArrow)
                        Navigate(1);
                    else if (e.keyCode == KeyCode.UpArrow)
                        Navigate(-1);
                    else if (e.keyCode == KeyCode.Escape) {
                        if (history.isOpen)
                            history.Close();
                        else if (autoComplete.isOpen)
                            autoComplete.Close();
                    }
                    else if (e.keyCode == settings.historyKey && settings.historyKey != KeyCode.None)
                        ToggleHistory();
                    else if (e.keyCode == settings.autoCompleteKey && settings.autoCompleteKey != KeyCode.None)
                        ToggleAutoComplete();
                }
            }
        }

        public void ToggleHistory() {
            history.ToggleOpen();
            if (history.isOpen && autoComplete.isOpen)
                autoComplete.Close();
        }

        public void ToggleAutoComplete() {
            autoComplete.ToggleOpen();
            if (autoComplete.isOpen && history.isOpen)
                history.Close();
        }

        void Navigate(int direction) {
            if(autoComplete.isOpen)
                autoComplete.Navigate(direction);
            else
                history.Navigate(direction);
            Event.current.Use();
        }

        void ApplyScaleAndDraw() {
            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one * settings.scale);
            Draw();
            GUI.matrix = oldMatrix;
        }

        void Draw() {
            if(settings.showOpenButton)
                openButton.Draw(height);
            if(buttonBlocker != null)
                buttonBlocker.rectTransform.sizeDelta = settings.showOpenButton ? new Vector2(openButton.width * settings.scale, openButton.height * settings.scale) : Vector2.zero;
            if(!isOpen)
                return;

            Vector2 windowPosition = new Vector2(input.offsetX, height - input.height);
            //When layouting, only AutoComplete & History need to be drawn
            if(Event.current.type == EventType.Layout) {
                history.Draw(windowPosition);
                autoComplete.Draw(windowPosition);
                return;
            }
            //Input has to be drawn first thing to keep focus across frames
            DrawInput();
            toolbar.Draw();
            float mainWindowHeight = height - input.height - toolbar.height;
            if(isSettingsOpen)
                settingsPanel.Draw(toolbar.height, mainWindowHeight);
            else
                logger.Draw(toolbar.height, mainWindowHeight);
            history.Draw(windowPosition);
            autoComplete.Draw(windowPosition);
        }

        void DrawInput() {
            //Disable input when animation is playing
            GUI.enabled = openCoroutine == null;
            input.Draw(height - input.height);
            GUI.enabled = true;
        }

        public void SubmitInputText() {
            string text = input.text.Trim();
            if(text == string.Empty)
                return;
            Log(new EntryData() { text = "> " + text, stackTrace = "User input: no stack trace info available." });
            if(history.CanEntryBeAdded(text)) {
                history.UpdateLastEntry(text);
                history.Add(string.Empty);
            }
            history.NavigateToLast();

            if(history.isOpen)
                history.Close();
            else if(autoComplete.isOpen)
                autoComplete.Close();

            input.Clear();
            ExecuteIfCommand(text);
        }

        public void Log(string text) {
            Log(new EntryData() { text = text });
        }

        public void Log(string text, EntryOptions options) {
            Log(new EntryData() { text = text, options = options });
        }

        public void Log(EntryData data) {
            logger.AddEntry(data);
        }

        public void LogWarning(string text) {
            LogWarning(new EntryData() { text = text });
        }

        public void LogWarning(string text, EntryOptions options) {
            LogWarning(new EntryData() { text = text, options = options });
        }

        public void LogWarning(EntryData data) {
            data.options.color = settings.warningColor;
            data.icon = settings.warningIcon;
            logger.AddEntry(data);
        }

        public void LogError(string text) {
            LogError(new EntryData() { text = text });
        }

        public void LogError(string text, EntryOptions options) {
            LogError(new EntryData() { text = text, options = options });
        }

        public void LogError(EntryData data) {
            data.options.color = settings.errorColor;
            data.icon = settings.errorIcon;
            logger.AddEntry(data);
        }

        void ExecuteIfCommand(string text) {
            CommandExecuter executer = commandsManager.GetCommandExecuter(text);
            if(executer.isValidCommand) {
                try {
                    object result = executer.Execute();
                    if(executer.hasReturnValue) {
                        string resultString = ConvertCommandResultToString(result);
                        Log(resultString);
                    }
                }
                catch(CommandSystemException exception) {
                    Debug.LogException(exception);
                }
            }
        }

        string ConvertCommandResultToString(object result) {
            if(result == null)
                return "null";
            else if(result is Array) {
                Array resultArray = (Array)result;
                StringBuilder builder = new StringBuilder();
                for(int i = 0; i < resultArray.Length; i++) {
                    builder.Append(resultArray.GetValue(i).ToString());
                    if(i < resultArray.Length - 1)
                        builder.Append(", ");
                }
                return builder.ToString();
            }
            else
                return result.ToString();
        }

        public void ClearLog() {
            logger.Clear();
        }

        public Entry[] GetEntries() {
            return logger.GetEntries();
        }

        public void OpenSettings() {
            isSettingsOpen = true;
        }

        public void CloseSettings() {
            isSettingsOpen = false;
        }

        public void ToggleSettings() {
            isSettingsOpen = !isSettingsOpen;
        }

        public void AddDegelateAsCommand(Action commandDelegate) {
            AddCommand(new ActionCommand(commandDelegate));
        }

        public void AddDegelateAsCommand<T1>(Action<T1> commandDelegate) {
            AddCommand(new ActionCommand<T1>(commandDelegate));
        }

        public void AddDegelateAsCommand<T1, T2>(Action<T1, T2> commandDelegate) {
            AddCommand(new ActionCommand<T1, T2>(commandDelegate));
        }

        public void AddDegelateAsCommand<T1, T2, T3>(Action<T1, T2, T3> commandDelegate) {
            AddCommand(new ActionCommand<T1, T2, T3>(commandDelegate));
        }

        public void AddDegelateAsCommand<T1, T2, T3, T4>(Action<T1, T2, T3, T4> commandDelegate) {
            AddCommand(new ActionCommand<T1, T2, T3, T4>(commandDelegate));
        }

        public void AddDegelateAsCommand<T1>(Func<T1> commandDelegate) {
            AddCommand(new FuncCommand<T1>(commandDelegate));
        }

        public void AddDegelateAsCommand<T1, TResult>(Func<T1, TResult> commandDelegate) {
            AddCommand(new FuncCommand<T1, TResult>(commandDelegate));
        }

        public void AddDegelateAsCommand<T1, T2, TResult>(Func<T1, T2, TResult> commandDelegate) {
            AddCommand(new FuncCommand<T1, T2, TResult>(commandDelegate));
        }

        public void AddDegelateAsCommand<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> commandDelegate) {
            AddCommand(new FuncCommand<T1, T2, T3, TResult>(commandDelegate));
        }

        public void AddDegelateAsCommand<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> commandDelegate) {
            AddCommand(new FuncCommand<T1, T2, T3, T4, TResult>(commandDelegate));
        }

        public void AddCommand(Command command) {
            commandsManager.Add(command);
        }

        public void AddCommands(Command[] commands) {
            commandsManager.Add(commands);
        }

        public void RemoveCommand(Command command) {
            commandsManager.Remove(command);
        }

        public void RemoveCommands(Command[] commands) {
            commandsManager.Remove(commands);
        }

        public void RemoveCommandOverloads(Command command) {
            commandsManager.RemoveOverloads(command);
        }

        public void RemoveCommandOverloads(Command[] commands) {
            commandsManager.RemoveOverloads(commands);
        }

        public bool IsCommandAdded(Command command) {
            return commandsManager.IsCommandAdded(command);
        }

        public bool IsCommandOverloadAdded(Command command) {
            return commandsManager.IsCommandOverloadAdded(command);
        }

        public Command[] GetCommands() {
            return commandsManager.GetCommands();
        }
    }
}