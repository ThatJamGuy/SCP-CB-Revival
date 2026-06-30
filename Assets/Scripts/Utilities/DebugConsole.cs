using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Static command registry. Register commands from any script via AddCommand();
/// This thing is literally black magic I barely know how it works.
/// </summary>
public static class DebugConsole {
    public const int MaxLogEntries = 200;

    private static readonly Dictionary<string, DebugCommand> _commands = new();
    private static readonly List<LogEntry> _log = new();

    public static event Action OnLogChanged;

    // --- Log Entry ---

    public readonly struct LogEntry {
        public readonly string Message;
        public readonly LogType Type;
        public readonly float Time;

        public LogEntry(string message, LogType type) {
            Message = message;
            Type = type;
            Time = UnityEngine.Time.time;
        }
    }

    // --- Unity Log Hook ---

    [RuntimeInitializeOnLoadMethod]
    public static void HookUnityLog() =>
        Application.logMessageReceived += OnUnityLog;

    public static void UnhookUnityLog() =>
        Application.logMessageReceived -= OnUnityLog;

    private static void OnUnityLog(string message, string stackTrace, LogType type) =>
        AddEntry(message, type);

    // --- Public Log API ---

    public static IReadOnlyList<LogEntry> GetLog() => _log;

    public static void ClearLog() {
        _log.Clear();
        OnLogChanged?.Invoke();
    }

    public static List<DebugCommand> GetMatchingCommands(string input) {
        input = input.Trim().ToLower();

        if (string.IsNullOrEmpty(input)) return new List<DebugCommand>();

        List<DebugCommand> matches = new();

        foreach (DebugCommand command in _commands.Values) {
            if (command.ID.StartsWith(input)) matches.Add(command);
        }

        matches.Sort((a, b) => string.Compare(a.ID, b.ID, StringComparison.Ordinal));

        return matches;
    }

    // --- Registration API ---

    /// <summary>Register a no-parameter command.</summary>
    public static void AddCommand(string id, string description, Action method) =>
        Register(id, description, method.Method, method.Target);

    /// <summary>Register a command with typed parameters using a strongly-typed Action.</summary>
    public static void AddCommand<T>(string id, string description, Action<T> method) =>
        Register(id, description, method.Method, method.Target);

    public static void AddCommand<T1, T2>(string id, string description, Action<T1, T2> method) =>
        Register(id, description, method.Method, method.Target);

    public static void AddCommand<T1, T2, T3>(string id, string description, Action<T1, T2, T3> method) =>
        Register(id, description, method.Method, method.Target);

    /// <summary>Register a static method by name and owning type.</summary>
    public static void AddCommandStatic(string id, string description, string methodName, Type ownerType) {
        MethodInfo method = ownerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        if (method == null) {
            Debug.LogError($"Static method '{methodName}' not found on {ownerType.Name}.");
            return;
        }

        Register(id, description, method, target: null);
    }

    /// <summary>Register an instance method by name and instance.</summary>
    public static void AddCommandInstance(string id, string description, string methodName, object instance) {
        MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (method == null) {
            Debug.LogError($"Instance method '{methodName}' not found on {instance.GetType().Name}.");
            return;
        }

        Register(id, description, method, instance);
    }

    public static void RemoveCommand(string id) => _commands.Remove(id);

    // --- Execution ---

    public static void Execute(string input) {
        input = input.Trim();
        if (string.IsNullOrEmpty(input)) return;

        LogDirect($"> {input}");

        string[] tokens = input.Split(' ');
        string commandID = tokens[0].ToLower();

        if (!_commands.TryGetValue(commandID, out DebugCommand command)) {
            Debug.LogWarning($"Unknown command: '{commandID}'. Type 'help' for a list of commands.");
            return;
        }

        command.Invoke(tokens[1..]);
    }

    public static IEnumerable<DebugCommand> GetAllCommands() => _commands.Values;

    // --- Internal ---

    /// <summary>Routes through Unity's log so everything flows via the single OnUnityLog hook.</summary>
    private static void LogInternal(string message, LogType type = LogType.Log) {
        switch (type) {
            case LogType.Warning: Debug.LogWarning(message); break;
            case LogType.Error:
            case LogType.Exception: Debug.LogError(message); break;
            default: Debug.Log(message); break;
        }
    }

    /// <summary>Writes directly to the console log without going through Unity. Use for console-only output (e.g. help listings).</summary>
    public static void LogDirect(string message, LogType type = LogType.Log) => AddEntry(message, type);

    private static void Register(string id, string description, MethodInfo method, object target) {
        id = id.ToLower();

        if (_commands.ContainsKey(id)) Debug.LogWarning($"Command '{id}' is already registered. Overwriting.");

        _commands[id] = new DebugCommand(id, description, method, target);
    }

    private static void AddEntry(string message, LogType type) {
        if (_log.Count >= MaxLogEntries) _log.RemoveAt(0);

        _log.Add(new LogEntry(message, type));
        OnLogChanged?.Invoke();
    }
}