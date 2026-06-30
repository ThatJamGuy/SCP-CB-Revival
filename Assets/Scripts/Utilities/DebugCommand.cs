using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Egyptian hieroglyph ass script to help with the creation of and handle parsing of commands
/// </summary>
public class DebugCommand {
    public string ID { get; }
    public string Description { get; }
    public string Format { get; }

    private readonly MethodInfo _method;
    private readonly object _target;

    public DebugCommand(string id, string description, MethodInfo method, object target = null) {
        ID = id;
        Description = description;
        _method = method;
        _target = target;

        Format = BuildFormat(id, method);
    }

    public void Invoke(string[] args) {
        ParameterInfo[] parameters = _method.GetParameters();

        if (args.Length < parameters.Length) {
            Debug.LogWarning($"Usage: {Format}");
            return;
        }

        object[] parsedArgs = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++) {
            if (!TryParse(args[i], parameters[i].ParameterType, out parsedArgs[i])) {
                Debug.LogWarning($"Invalid argument '{args[i]}' for parameter <{parameters[i].Name}> ({parameters[i].ParameterType.Name})");
                return;
            }
        }

        object result = _method.Invoke(_target, parsedArgs);

        if (result != null)
            Debug.Log($"[{ID}] => {result}");
    }

    // Supported type parsers below

    private static bool TryParse(string input, Type type, out object result) {
        try {
            if (type == typeof(string)) { result = input; return true; }
            if (type == typeof(int)) { result = int.Parse(input); return true; }
            if (type == typeof(float)) { result = float.Parse(input); return true; }
            if (type == typeof(bool)) { result = ParseBool(input); return true; }
            if (type == typeof(Vector2)) { result = ParseVector2(input); return true; }
            if (type == typeof(Vector3)) { result = ParseVector3(input); return true; }

            if (type.IsEnum) { result = Enum.Parse(type, input, ignoreCase: true); return true; }

            Debug.LogError($"Unsupported parameter type: {type.Name}");
            result = null;
            return false;
        } catch {
            result = null;
            return false;
        }
    }

    private static bool ParseBool(string input) =>
        input is "1" or "true" ? true
        : input is "0" or "false" ? false
        : bool.Parse(input);

    private static Vector2 ParseVector2(string input) {
        string[] parts = input.Trim('[', ']', '(', ')').Split(',');
        return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
    }

    private static Vector3 ParseVector3(string input) {
        string[] parts = input.Trim('[', ']', '(', ')').Split(',');
        return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
    }

    private static string BuildFormat(string id, MethodInfo method) {
        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length == 0) return id;

        string paramList = string.Join(" ", Array.ConvertAll(parameters, p => $"<{p.Name} ({p.ParameterType.Name})>"));

        return $"{id} {paramList}";
    }
}