using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Put this John on the Runtime Engine to handle everything relating to creating/saving/loading a game state
// Each save is it's own folder for self contained niceness, each featuring JSON modules and eventually a preview image
// NOTE: CURRENTLY HAS A LOT OF COMMENTED OUT CODE THAT WILL COME BACK LATER. IT'S JUST MOVED TO USE THE SINGLE SLOT SYSTEM FOR NOW
public static class SavegameEngine {
    private const string SAVE_ROOT_FOLDER = "Saves";
    private const string SAVE_EXTENSION = ".cbr";
    private const string TEMP_SINGLE_SLOT = "SaveFile";

    private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars();
    private static readonly Dictionary<Type, object> cache = new Dictionary<Type, object>(8);
    private static readonly Dictionary<Type, string> paths = new Dictionary<Type, string>(8);
    private static readonly List<ISaveable> saveables = new List<ISaveable>(16);

    private static string slotFolder; // IE. "Saves/D-9341"

    public static bool HasActiveSave => slotFolder != null;
    public static SessionData Session => Get<SessionData>();

    // Returns true or false if the save attempt is a new game
    public static bool NewGame(string saveName, string mapSeed, int difficulty, bool skipIntro) {
        //string saveFolder = Path.Combine(SAVE_ROOT_FOLDER, ToFolderName(saveName));
        string saveFolder = Path.Combine(SAVE_ROOT_FOLDER, TEMP_SINGLE_SLOT);
        string absolutePath = Path.Combine(Application.persistentDataPath, saveFolder);

        // Wipe the old save entirely so stale modules (player, world, etc.) can't leak into the new game
        try {
            if (Directory.Exists(absolutePath)) Directory.Delete(absolutePath, true);
            Directory.CreateDirectory(absolutePath);
        } catch (IOException e) {
            Debug.LogError("Could not reset the save slot: " + e.Message);
            return false;
        }

        //if (Directory.Exists(absolutePath)) return false;
        //Directory.CreateDirectory(absolutePath);

        SetSlot(saveFolder);

        // Setup the default save file values with stuff set in the main menu config
        var session = new SessionData {
            saveName = saveName,
            mapSeed = mapSeed,
            difficulty = difficulty,
            skipIntro = skipIntro,
            createdUTC = DateTime.UtcNow.ToString("o"),
            version = Application.version
        };

        cache[typeof(SessionData)] = session;
        DataSaver.Save(session, GetPath(typeof(SessionData)));
        return true;
    }

    // Makes an existing save active (If it can find it, and it will return false if it can't)
    public static bool LoadGame(string saveFolder) {
        string folder = Path.Combine(SAVE_ROOT_FOLDER, saveFolder);
        SetSlot(folder);

        if (!DataSaver.DataFileExists(GetPath(typeof(SessionData)))) {
            slotFolder = null;
            return false;
        }

        Get<SessionData>();
        return true;
    }

    // Extra bool for loading the temporary single slot
    public static bool LoadGame() => LoadGame(TEMP_SINGLE_SLOT);

    // List the folder names for all the existing saves, and allows each saves SessionData to be read for UI reason
    public static List<string> ListSaves() {
        List<string> result = new List<string>();
        string saveRootFolder = Path.Combine(Application.persistentDataPath, SAVE_ROOT_FOLDER);

        if (!Directory.Exists(saveRootFolder)) return result;

        string[] dirs = Directory.GetDirectories(saveRootFolder);
        for (int i = 0; i < dirs.Length; i++) result.Add(Path.GetFileName(dirs[i]));

        return result;
    }

    public static void DeleteSave(string saveFolder) {
        string absolutePath = Path.Combine(Application.persistentDataPath, SAVE_ROOT_FOLDER, saveFolder);
        if (Directory.Exists(absolutePath)) Directory.Delete(absolutePath, true);
        if (slotFolder != null && slotFolder == Path.Combine(SAVE_ROOT_FOLDER, saveFolder)) SetSlot(null);
    }

    // Is the temporary save slot currently filled by a save or nah
    public static bool HasSave() {
        return DataSaver.DataFileExists(Path.Combine(SAVE_ROOT_FOLDER, TEMP_SINGLE_SLOT, nameof(SessionData) + SAVE_EXTENSION));
    }

    // Returns the cached module data, loading it (or creating a default) on first request
    public static T Get<T>() where T : class, new() {
        Type type = typeof(T);

        if (cache.TryGetValue(type, out object obj)) return (T)obj;

        T data = DataSaver.Load<T>(GetPath(type));
        cache[type] = data;
        return data;
    }

    // Saves a single module to disk where applicable
    public static void Save<T>() where T : class, new() {
        Type type = typeof(T);
        if (cache.TryGetValue(type, out object obj)) DataSaver.Save(obj, GetPath(type));
    }

    // Asks all registered ISaveables to update their data, then writes every loaded module
    public static void SaveAll() {
        for (int i = 0; i < saveables.Count; i++) saveables[i].PrepareSaveableValues();
        foreach (KeyValuePair<Type, object> pair in cache) DataSaver.Save(pair.Value, GetPath(pair.Key));
    }

    // Register a class using ISaveable to the SavegameEngine
    public static void Register(ISaveable saveable) {
        if (!saveables.Contains(saveable)) saveables.Add(saveable);
    }

    // Inverse of Register as one could probably guess
    public static void Unregister(ISaveable saveable) {
        saveables.Remove(saveable);
    }

    static void SetSlot(string folder) {
        slotFolder = folder;
        cache.Clear();
        paths.Clear();
        saveables.Clear();
    }

    // Builds "Saves/<slot>/<TypeName>.json" once per type per slot
    static string GetPath(Type type) {
        if (!paths.TryGetValue(type, out string path)) {
            path = Path.Combine(slotFolder, type.Name + SAVE_EXTENSION);
            paths[type] = path;
        }
        return path;
    }

    static string ToFolderName(string name) {
        char[] chars = name.ToCharArray();
        for (int i = 0; i < chars.Length; i++) {
            if (Array.IndexOf(InvalidChars, chars[i]) >= 0)
                chars[i] = '_';
        }
        return new string(chars);
    }
}

// Default save file setup. Necessary for pre-game configuration
[Serializable]
public class SessionData {
    public string saveName;
    public string mapSeed;
    public int difficulty;
    public bool skipIntro;
    public string createdUTC;
    public string version;
}