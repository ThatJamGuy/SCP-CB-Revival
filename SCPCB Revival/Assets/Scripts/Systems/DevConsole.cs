using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevConsole : MonoBehaviour {
    public static DevConsole Instance { get; private set; }

    [SerializeField] TMP_Text consoleOutput;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] int visibleLines = 120;
    [SerializeField] TMP_Text suggestionText;

    struct LogRecord {
        public string msg;
        public LogType type;
        public string time;
    }

    readonly Dictionary<string, Action<string[]>> commands = new Dictionary<string, Action<string[]>>();
    static readonly ConcurrentQueue<LogRecord> incoming = new ConcurrentQueue<LogRecord>();
    readonly List<LogRecord> scrollback = new List<LogRecord>();

    bool shouldAutoScroll = true;
    bool repaint;

    void Awake() {
        Instance = this;
        inputField.onSubmit.AddListener(OnSubmit);
        inputField.onValueChanged.AddListener(OnInputChanged);
        Register("help", Help);
    }

    void OnEnable() {
        Application.logMessageReceivedThreaded += HandleLog;
        inputField.Select();
        inputField.ActivateInputField();
        UpdateSuggestion("");
    }

    void OnDisable() {
        Application.logMessageReceivedThreaded -= HandleLog;
    }

    void HandleLog(string msg, string stack, LogType type) {
        incoming.Enqueue(new LogRecord {
            msg = msg,
            type = type,
            time = DateTime.Now.ToString("HH:mm:ss")
        });
    }

    string F(LogRecord r) {
        switch (r.type) {
            case LogType.Warning: return "<color=#8888FF>[" + r.time + "]</color> <color=#FFA800>" + r.msg + "</color>";
            case LogType.Error: return "<color=#8888FF>[" + r.time + "]</color> <color=#FF3A3A>" + r.msg + "</color>";
            case LogType.Exception: return "<color=#8888FF>[" + r.time + "]</color> <color=#FF3A3A>" + r.msg + "</color>";
            default: return "<color=#8888FF>[" + r.time + "]</color> <color=#FFFFFF>" + r.msg + "</color>";
        }
    }

    void Update() {
        bool newLogs = false;
        while (incoming.TryDequeue(out var r)) {
            scrollback.Add(r);
            newLogs = true;
            repaint = true;
        }

        if (newLogs) {
            CheckScrollPosition();
        }

        if (!repaint) return;
        repaint = false;

        RenderWindow();

        if (shouldAutoScroll) {
            StartCoroutine(ScrollToBottomNextFrame());
        }
    }

    void CheckScrollPosition() {
        if (scrollRect.verticalNormalizedPosition <= 0.01f) {
            shouldAutoScroll = true;
        }
    }

    IEnumerator ScrollToBottomNextFrame() {
        yield return null;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    void RenderWindow() {
        var total = scrollback.Count;
        if (total == 0) {
            consoleOutput.text = "";
            return;
        }

        var pos = 1f - scrollRect.verticalNormalizedPosition;
        var maxStart = Mathf.Max(0, total - visibleLines);
        var start = Mathf.Clamp((int)(pos * total) - visibleLines / 2, 0, maxStart);
        var end = Mathf.Min(start + visibleLines, total);

        var sb = new System.Text.StringBuilder(4096);
        for (int i = start; i < end; i++)
            sb.Append(F(scrollback[i])).Append('\n');

        consoleOutput.text = sb.ToString();
    }

    public void OnScrollChanged(Vector2 pos) {
        if (pos.y > 0.01f) {
            shouldAutoScroll = false;
        }
    }

    public void Register(string name, Action<string[]> action) {
        commands[name.ToLower()] = action;
    }

    public void Add(string name, Action action) {
        Register(name, _ => action());
    }

    public void Add<T>(string name, Action<T> action) {
        Register(name, args => {
            if (args.Length < 1) {
                Log($"{name} requires 1 argument", LogType.Error);
                return;
            }
            if (TryParse(args[0], out T val)) action(val);
            else Log($"Failed to parse '{args[0]}' as {typeof(T).Name}", LogType.Error);
        });
    }

    public void Add<T1, T2>(string name, Action<T1, T2> action) {
        Register(name, args => {
            if (args.Length < 2) {
                Log($"{name} requires 2 arguments", LogType.Error);
                return;
            }
            if (TryParse(args[0], out T1 v1) && TryParse(args[1], out T2 v2)) action(v1, v2);
            else Log($"Failed to parse arguments for {name}", LogType.Error);
        });
    }

    public void Add<T1, T2, T3>(string name, Action<T1, T2, T3> action) {
        Register(name, args => {
            if (args.Length < 3) {
                Log($"{name} requires 3 arguments", LogType.Error);
                return;
            }
            if (TryParse(args[0], out T1 v1) && TryParse(args[1], out T2 v2) && TryParse(args[2], out T3 v3)) action(v1, v2, v3);
            else Log($"Failed to parse arguments for {name}", LogType.Error);
        });
    }

    public void Bind<T>(string name, Func<T> getter, Action<T> setter) where T : IConvertible {
        Register(name, args => {
            if (args.Length == 0) {
                Log($"{name} = {getter()}", LogType.Log);
                return;
            }
            if (TryParse(args[0], out T val)) {
                setter(val);
                Log($"{name} = {val}", LogType.Log);
            }
            else Log($"Failed to parse '{args[0]}' as {typeof(T).Name}", LogType.Error);
        });

        Register(name + "?", _ => Log($"{name} = {getter()}", LogType.Log));

        Register(name + "+=", args => {
            if (args.Length < 1) {
                Log($"{name}+= requires 1 argument", LogType.Error);
                return;
            }
            if (TryParse(args[0], out T delta)) {
                var current = getter();
                var result = Add(current, delta);
                setter(result);
                Log($"{name} = {result} (was {current})", LogType.Log);
            }
            else Log($"Failed to parse '{args[0]}' as {typeof(T).Name}", LogType.Error);
        });

        Register(name + "-=", args => {
            if (args.Length < 1) {
                Log($"{name}-= requires 1 argument", LogType.Error);
                return;
            }
            if (TryParse(args[0], out T delta)) {
                var current = getter();
                var result = Subtract(current, delta);
                setter(result);
                Log($"{name} = {result} (was {current})", LogType.Log);
            }
            else Log($"Failed to parse '{args[0]}' as {typeof(T).Name}", LogType.Error);
        });
    }

    T Add<T>(T a, T b) where T : IConvertible {
        return (T)Convert.ChangeType(a.ToDouble(null) + b.ToDouble(null), typeof(T));
    }

    T Subtract<T>(T a, T b) where T : IConvertible {
        return (T)Convert.ChangeType(a.ToDouble(null) - b.ToDouble(null), typeof(T));
    }

    bool TryParse<T>(string str, out T result) {
        try {
            var t = typeof(T);
            if (t == typeof(string)) {
                result = (T)(object)str;
                return true;
            }
            if (t == typeof(int)) {
                result = (T)(object)int.Parse(str);
                return true;
            }
            if (t == typeof(float)) {
                result = (T)(object)float.Parse(str, CultureInfo.InvariantCulture);
                return true;
            }
            if (t == typeof(bool)) {
                result = (T)(object)(str == "1" || str.ToLower() == "true");
                return true;
            }
            if (t == typeof(double)) {
                result = (T)(object)double.Parse(str, CultureInfo.InvariantCulture);
                return true;
            }
            if (t.IsEnum) {
                result = (T)Enum.Parse(t, str, true);
                return true;
            }
            result = default;
            return false;
        }
        catch {
            result = default;
            return false;
        }
    }

    void OnSubmit(string txt) {
        if (string.IsNullOrWhiteSpace(txt)) return;
        Log(txt, LogType.Log);
        inputField.text = "";
        UpdateSuggestion("");
        Execute(txt);
        shouldAutoScroll = true;
    }

    void Execute(string input) {
        var parts = input.Split(' ');
        var cmd = parts[0].ToLower();
        var args = new string[Mathf.Max(0, parts.Length - 1)];
        if (args.Length > 0) Array.Copy(parts, 1, args, 0, args.Length);

        if (commands.TryGetValue(cmd, out var act)) {
            try { act(args); }
            catch (Exception e) { Log(e.ToString(), LogType.Error); }
        }
        else Log("Unknown command: " + cmd, LogType.Error);
    }

    void Help(string[] a) {
        Log("=== Available Commands ===", LogType.Log);
        var sorted = new List<string>(commands.Keys);
        sorted.Sort();
        foreach (var cmd in sorted)
            Log($"  • {cmd}", LogType.Log);
        Log($"=== Total: {commands.Count} ===", LogType.Log);
    }

    void OnInputChanged(string input) {
        UpdateSuggestion(input);
    }

    void UpdateSuggestion(string input) {
        if (suggestionText == null) return;

        if (string.IsNullOrEmpty(input)) {
            suggestionText.text = "";
            return;
        }

        var lower = input.ToLower();
        foreach (var cmd in commands.Keys) {
            if (cmd.StartsWith(lower) && cmd != lower) {
                suggestionText.text = input + "<color=#88888888>" + cmd.Substring(lower.Length) + "</color>";
                return;
            }
        }

        suggestionText.text = "";
    }

    public void Log(string msg, LogType type) {
        incoming.Enqueue(new LogRecord {
            msg = msg,
            type = type,
            time = DateTime.Now.ToString("HH:mm:ss")
        });
    }

    public void SelectInputField() {
        inputField.Select();
        inputField.ActivateInputField();
    }
}