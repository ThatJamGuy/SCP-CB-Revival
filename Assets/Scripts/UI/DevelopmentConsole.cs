using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DevelopmentConsole : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private int maxLogHistory = 100;

    [Header("High Priority")]
    [SerializeField] private InputAction historyPrevious;
    [SerializeField] private InputAction historyNext;
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
    private readonly List<string> commandHistory = new List<string>();

    private TextMeshProUGUI[] logPool;

    private int poolCount;
    private int poolHead;
    private int historyBrowseIndex = -1;
    private string historyDraft = string.Empty;

    #region Unity Lifecycle

    private void Awake() {
        logPool = new TextMeshProUGUI[maxLogHistory];
        RegisterCommands();
    }

    private void OnEnable() {
        consoleInput.ActivateInputField();
        consoleInput.Select();
        consoleInput.onSubmit.AddListener(SubmitCommand);

        historyPrevious?.Enable();
        historyNext?.Enable();
    }

    private void OnDisable() {
        consoleInput.onSubmit.RemoveListener(SubmitCommand);

        historyPrevious?.Disable();
        historyNext?.Disable();
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

        if (historyPrevious != null && historyPrevious.WasPressedThisFrame()) {
            NavigateCommandHistory(-1);
        }

        if (historyNext != null && historyNext.WasPressedThisFrame()) {
            NavigateCommandHistory(1);
        }
    }

    #endregion

    #region Private Methods

    private void RegisterCommands() {
        Register("help", "Lists all available commands.", CMD_Help);
        Register("clear", "Clear all console outputs.", CMD_Clear);
        Register("teleport", "Teleport the player to a room, taking in a <roomid> argument.", CMD_TeleportPlayer);
        Register("spawnitem", "Spawn an item in front of the player, taking in an <itemid> argument.", CMD_SpawnItem);
        Register("spawnentity", "Spawn an entity at the closest waypoint to the player, taking in <entityid>.", CMD_SpawnEntity);
        Register("settrack", "Set the music track, taking in <trackindex> and optinally <intensity>.", CMD_SetTrack);
        Register("printlockdown", "Prints the current state of the LCZ lockdown.", CMD_PrintLockdown);
        Register("disablelockdown", "Disabled the lockdown on the Light Containment Zone.", CMD_DisableLockdown);
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

        // Add the command just entered into the history for easy reuse
        commandHistory.Add(command);
        historyBrowseIndex = -1;
        historyDraft = string.Empty;

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

        UnityEngine.Canvas.ForceUpdateCanvases();
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

    private void NavigateCommandHistory(int direction) {
        if (commandHistory.Count == 0) return;
        if (historyBrowseIndex == -1) {
            if (direction > 0) return;

            historyDraft = consoleInput.text;
            historyBrowseIndex = commandHistory.Count;
        }

        int nextIndex = historyBrowseIndex + direction;

        if (nextIndex < 0) nextIndex = 0;
        if (nextIndex >= commandHistory.Count) {
            historyBrowseIndex = -1;
            consoleInput.text = historyDraft;
        } else {
            historyBrowseIndex = nextIndex;
            consoleInput.text = commandHistory[historyBrowseIndex];
        }

        consoleInput.caretPosition = consoleInput.text.Length;
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

        InventorySystem.Instance.DebugSpawnItem(args[0], true);
    }

    private void CMD_SpawnEntity(string[] args) {
        if (args.Length < 1) {
            AddLine("<color=yellow>Usage: spawnentity <entityid>");
            return;
        }

        EntitySystem.Instance.DebugSpawnEntity(args[0]);
    }

    private void CMD_SetTrack(string[] args) {
        if (args.Length < 1 || !int.TryParse(args[0], out int trackIndex) || !int.TryParse(args[1], out int intensity)) {
            AddLine("<color=yellow>Usage: settrack <trackindex> <intensity>");
            return;
        }

        MusicManager.Instance.SetTrack(trackIndex, intensity);
        Debug.Log($"<color=#00ffff>Attempting to change the currently playing track to {trackIndex} with an intensity of {intensity}.</color>");
    }

    private void CMD_PrintLockdown(string[] args) {
        Debug.Log($"<color=#00ffff>Current LCZ lockdown state: {RevivalSessionEngine.lczInLockdown}</color>");
    }

    private void CMD_DisableLockdown(string[] args) {
        EVNT_SL.DisableLCZLockdownViaCMD();
    }

    private void CMD_TeleportPlayer(string[] args) {
        if (args.Length < 1) {
            AddLine("<color=yellow>Usage: teleport <roomname>");
            return;
        }

        // Not usually a good idea to do this upon executing the command but it will pass for now since the only people that will complain about suttering from this
        // are those that are using it to cheat lol (at least until the project get's bigger and there are more legit uses for it in which case I'll fix this later)
        RoomHandler[] rooms = FindObjectsByType<RoomHandler>();

        foreach (RoomHandler room in rooms) {
            if (room.roomName == args[0] && room.teleportLocation != null) {
                room.TeleportPlayerHere();
                Debug.Log($"<color=#00ffff>Teleporting player to room: '{room.roomName}'</color>");
                return;
            }
        }

        Debug.LogWarning("Could not find a room with that ID, or the room is not configured for teleporting.");
    }

    #endregion
}