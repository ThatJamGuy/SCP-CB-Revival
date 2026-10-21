using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevelopmentConsole : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private int maxLogHistory = 100;

    [Header("High Priority")]
    [SerializeField] private GameObject logItemPrefab;

    [Header("References")]
    [SerializeField] private TMP_InputField consoleInput;
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private ScrollRect scrollRect;

    private delegate void CommandHandler(string[] args);

    // The mound
    private const int MAX_PENDING = 256;
    private static readonly Queue<string> PendingLogs = new Queue<string>(MAX_PENDING);
    private static readonly object PendingLock = new object();
    private static readonly char[] Separators = { ' ' };
    private struct CommandInfo { public CommandHandler Handler; public string Description; }
    private readonly Dictionary<string, CommandInfo> commands = new Dictionary<string, CommandInfo>(16, System.StringComparer.OrdinalIgnoreCase);

    private TextMeshProUGUI[] logPool;

    private int poolCount;
    private int poolHead;

    #region Unity Lifecycle

    private void Awake() {
        logPool = new TextMeshProUGUI[maxLogHistory];
        RegisterCommands();
    }

    private void OnEnable() {
        consoleInput.ActivateInputField();
        consoleInput.Select();

        consoleInput.onSubmit.AddListener(SubmitCommand);
    }

    private void OnDisable() {
        consoleInput.onSubmit.RemoveListener(SubmitCommand);
    }

    private void LateUpdate() {
        while (true) {
            string line;
            lock (PendingLock) {
                if (PendingLogs.Count == 0) break;
                line = PendingLogs.Dequeue();
            }
            AddLine(line);
        }
    }

    #endregion

    #region Private Methods

    private void RegisterCommands() {
        Register("help", "Lists all available commands.", CMD_Help);
        Register("clear", "Clear all console outputs.", CMD_Clear);
        Register("spawnitem", "Spawn an item in front of the player taking in an <itemid> argument.", CMD_SpawnItem);
    }

    private void Register(string name, string desc, CommandHandler handler) {
        commands[name] = new CommandInfo { Handler = handler, Description = desc };
    }

    private void SubmitCommand(string command) {
        if (string.IsNullOrEmpty(command)) return;

        AddLine("> " + command);

        string[] parts = command.Split(Separators, System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0) {
            if (commands.TryGetValue(parts[0], out CommandInfo cmd)) {
                // Pass in everything PAST the initial command name as arguments for the command
                string[] args = new string[parts.Length - 1];
                System.Array.Copy(parts, 1, args, 0, args.Length);
                cmd.Handler(args);
            } else {
                AddLine("<color=red>Unknown command: " + parts[0] + "</color>");
            }
        }

        consoleInput.text = string.Empty;
        consoleInput.ActivateInputField();
        consoleInput.Select();
    }

    private void AddLine(string text) {
        TextMeshProUGUI item;

        if (poolCount < logPool.Length) {
            item = Instantiate(logItemPrefab, scrollViewContent).GetComponent<TextMeshProUGUI>();
            logPool[poolCount++] = item;
        } else {
            // Recycle the oldest line instead of instantiating/destroying
            item = logPool[poolHead];
            poolHead = (poolHead + 1) % logPool.Length;
            item.transform.SetAsLastSibling();
        }

        item.SetText(text);

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void HookUnityLogs() {
        Application.logMessageReceivedThreaded -= EnqueueLog;
        Application.logMessageReceivedThreaded += EnqueueLog;

        lock (PendingLock) { PendingLogs.Clear(); }
    }

    private static void EnqueueLog(string condition, string stackTrace, LogType type) {
        string line;
        switch (type) {
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                line = "<color=red>" + condition + "</color>";
                break;
            case LogType.Warning:
                line = "<color=yellow>" + condition + "</color>";
                break;
            default:
                line = condition;
                break;
        }

        lock (PendingLock) {
            if (PendingLogs.Count >= MAX_PENDING) PendingLogs.Dequeue();
            PendingLogs.Enqueue(line);
        }
    }

    #endregion

    #region Console Commands

    private void CMD_Help(string[] args) {
        foreach (KeyValuePair<string, CommandInfo> pair in commands) {
            AddLine(pair.Key + " - " + pair.Value.Description);
        }
    }

    private void CMD_Clear(string[] args) {
        // Destroy every log item gameobject to clear it up
        for (int i = 0; i < poolCount; i++) {
            Destroy(logPool[i].gameObject);
        }

        // Set this to 0 so future clear calls won't try to get the old ones
        poolCount = 0;
    }

    private void CMD_SpawnItem(string[] args) {
        if (args.Length < 1) {
            AddLine("<color=yellow>Usage: spawnitem <itemid></color>");
            return;
        }

        InventorySystem.Instance.DebugSpawnItem(args[0]);
        Debug.Log($"<color=#00ffff>Spawned item: {args[0]}</color>");
    }

    #endregion
}