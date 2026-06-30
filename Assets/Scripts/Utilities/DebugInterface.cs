using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugInterface : MonoBehaviour {
    [Header("Menu Manager")]
    [SerializeField] private MenuManager menuManager;

    private const float CONSOLE_HEIGHT = 300f;
    private const float INPUT_FIELD_HEIGHT = 30f;
    private const float LOG_HEIGHT = 18f;

    private static readonly Color ColorLog = Color.white;
    private static readonly Color ColorWarning = Color.yellow;
    private static readonly Color ColorError = Color.red;

    private readonly List<DebugCommand> suggestions = new();
    private readonly List<string> commandHistory = new();

    private InputAction consoleAction;
    private InputAction submitAction;
    private InputAction navigateAction;
    private InputAction autocompleteAction;
    private GUIStyle suggestionStyle;

    private Vector2 scroll;

    private bool debugInfoVisible;
    private bool consoleVisible;
    private bool moveCaretToEnd;
    private int lastLogCount;
    private int selectedSuggestion = -1;
    private int selectedHistory = -1;
    private string lastSuggestionInput = string.Empty;
    string input;

    #region Unity Callbacks

    private void Awake() {
        // Grab all these inputs
        consoleAction = InputManager.Instance.GetAction("Player", "Console");
        submitAction = InputManager.Instance.GetAction("UI", "Submit");
        navigateAction = InputManager.Instance.GetAction("UI", "Navigate");
        autocompleteAction = InputManager.Instance.GetAction("Player", "Inventory");

        // General purpose info for first time startup
        DebugConsole.LogDirect("SCP - Containment Breach Revival. Copyright (c) 2024-2026 through the Creative Commons 4 Liscence.");
        DebugConsole.LogDirect("Type 'help' to display all available console commands. Use [TAB] to auto-complete a highlighted entry.");

        // Some built in global commands
        DebugConsole.AddCommand("help", "Displays all available commands.", PrintHelp);
        DebugConsole.AddCommand("clear", "Clears the console log.", DebugConsole.ClearLog);
        DebugConsole.AddCommand("debug", "Toggles the debug info overlay.", () => debugInfoVisible = !debugInfoVisible);
    }

    private void OnDestroy() {
        DebugConsole.UnhookUnityLog();
    }

    private void OnEnable() {
        if (consoleAction == null) return;

        consoleAction.performed += ToggleConsole;
        submitAction.performed += OnSubmit;
        navigateAction.performed += OnNaviagate;
        autocompleteAction.performed += OnAutoComplete;

        consoleAction.Enable();
        submitAction.Enable();
        navigateAction.Enable();
        autocompleteAction.Enable();
    }

    private void OnDisable() {
        if (consoleAction == null) return;

        consoleAction.performed -= ToggleConsole;
        submitAction.performed -= OnSubmit;
        navigateAction.performed -= OnNaviagate;
        autocompleteAction.performed -= OnAutoComplete;

        consoleAction.Disable();
        submitAction.Disable();
        navigateAction.Disable();
        autocompleteAction.Disable();
    }

    #endregion

    #region Private Methods

    private void ToggleConsole(InputAction.CallbackContext context) {
        if (!SettingsManager.settingsData.consoleEnabled) return;
        consoleVisible = !consoleVisible;
        menuManager.ToggleUnregisteredMenu(consoleVisible);
    }

    private void OnSubmit(InputAction.CallbackContext context) {
        if (!consoleVisible) return;
        if (string.IsNullOrWhiteSpace(input)) return;

        DebugConsole.Execute(input);

        commandHistory.Insert(0, input);
        selectedHistory = -1;

        input = "";

        suggestions.Clear();
        selectedSuggestion = -1;
        lastSuggestionInput = "";
    }

    private void OnNaviagate(InputAction.CallbackContext context) {
        if (!consoleVisible) return;

        Vector2 navigate = context.ReadValue<Vector2>();

        if (Mathf.Abs(navigate.y) < 0.5f) return;

        // If the suggestions are showing handle the selection of suggestions
        if (suggestions.Count > 0) {
            if (navigate.y > 0) selectedSuggestion--;
            else selectedSuggestion++;

            if (selectedSuggestion < 0) selectedSuggestion = suggestions.Count - 1;

            if (selectedSuggestion >= suggestions.Count) selectedSuggestion = 0;

            return;
        }

        // Otherwise recall previous input history
        if (commandHistory.Count == 0) return;

        if (navigate.y > 0) selectedHistory++;
        else selectedHistory--;

        selectedHistory = Mathf.Clamp(selectedHistory, -1, commandHistory.Count - 1);
        input = selectedHistory == -1 ? "" : commandHistory[selectedHistory];
    }

    private void OnAutoComplete(InputAction.CallbackContext context) {
        if (!consoleVisible || selectedSuggestion < 0) return;

        input = suggestions[selectedSuggestion].ID + "";
        moveCaretToEnd = true;

        UpdateSuggestions();
    }

    private void UpdateSuggestions() {
        string command = input;
        int spaceIndex = input.IndexOf(' ');

        if (spaceIndex >= 0) command = input[..spaceIndex];
        if (command == lastSuggestionInput) return;

        suggestions.Clear();
        suggestions.AddRange(DebugConsole.GetMatchingCommands(command));
        selectedSuggestion = suggestions.Count > 0 ? 0 : -1;
        lastSuggestionInput = command;
    }

    private void PrintHelp() {
        var commands = new List<DebugCommand>(DebugConsole.GetAllCommands());
        commands.Sort((a, b) => string.Compare(a.ID, b.ID, StringComparison.Ordinal));

        DebugConsole.LogDirect(" ========== Available Commands ==========");

        foreach (DebugCommand cmd in commands) DebugConsole.LogDirect($"   {cmd.Format} - {cmd.Description}");
    }

    private string HighlightCommand(string command) {
        if (string.IsNullOrEmpty(input)) return command;

        string typed = input.Split(' ')[0];

        if (!command.StartsWith(typed, StringComparison.OrdinalIgnoreCase)) return command;

        return $"<color=yellow>{command[..typed.Length]}</color>{command[typed.Length..]}";
    }

    #endregion

    #region OnGUI

    private void OnGUI() {
        DrawDebugOverlay();
        DrawConsole();
    }

    private void DrawDebugOverlay() {
        if (!debugInfoVisible) return;

        GUILayout.BeginArea(new Rect((Screen.width - 300) - 10, 10, 300, 400), GUI.skin.box);
        GUILayout.Label("<b>Debug Information</b>");
        GUILayout.Space(10);
        GUILayout.Label("Application Version: " + Application.version);
        GUILayout.Label($"Player pos: {Player.Instance.transform.position.x}, {Player.Instance.transform.position.y}, {Player.Instance.transform.position.z}");
        GUILayout.EndArea();
    }

    private void DrawConsole() {
        if (!consoleVisible) return;

        float logPanelY = DrawLogPanel();
        DrawInputField(logPanelY);
    }

    private float DrawLogPanel() {
        IReadOnlyList<DebugConsole.LogEntry> log = DebugConsole.GetLog();

        float contentHeight = Mathf.Max(CONSOLE_HEIGHT, (LOG_HEIGHT * log.Count) + 10f);
        var viewport = new Rect(0, 0, Screen.width - 16f, contentHeight);

        GUI.Box(new Rect(0, 0, Screen.width, CONSOLE_HEIGHT), "");

        scroll = GUI.BeginScrollView(
            new Rect(0, 0, Screen.width, CONSOLE_HEIGHT),
            scroll,
            viewport
        );

        Color prevColor = GUI.contentColor;

        for (int i = 0; i < log.Count; i++) {
            DebugConsole.LogEntry entry = log[i];

            GUI.contentColor = entry.Type switch {
                LogType.Warning => ColorWarning,
                LogType.Error or LogType.Exception => ColorError,
                _ => ColorLog,
            };

            GUI.Label(new Rect(5, LOG_HEIGHT * i, viewport.width - 10, LOG_HEIGHT), entry.Message);
        }

        GUI.contentColor = prevColor;
        GUI.EndScrollView();

        if (log.Count != lastLogCount) {
            scroll.y = Mathf.Max(0, contentHeight - CONSOLE_HEIGHT);
            lastLogCount = log.Count;
        }

        return CONSOLE_HEIGHT;
    }

    private void DrawInputField(float y) {
        GUI.Box(new Rect(0, y, Screen.width, INPUT_FIELD_HEIGHT), "");
        GUI.backgroundColor = Color.clear;

        GUI.SetNextControlName("console");
        string newInput = GUI.TextField(new Rect(10, y + 5, Screen.width - 20, 20), input);
        GUI.FocusControl("console");

        if (newInput != input) {
            input = newInput;
            UpdateSuggestions();
        }

        if (moveCaretToEnd) {
            TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);

            editor.cursorIndex = input.Length;
            editor.selectIndex = input.Length;

            moveCaretToEnd = false;
        }

        DrawSuggestions(y + INPUT_FIELD_HEIGHT);
    }

    private void DrawSuggestions(float y) {
        if (suggestions.Count == 0) return;

        suggestionStyle = new GUIStyle(GUI.skin.label) { richText = true };

        const float rowHeight = 22;

        for (int i = 0; i < suggestions.Count; i++) {
            Rect row = new Rect(10, y + i * rowHeight, 500, rowHeight);

            if (i == selectedSuggestion) GUI.Box(row, "");

            GUI.Label(row, HighlightCommand(suggestions[i].ID), suggestionStyle);
        }
    }

    #endregion
}