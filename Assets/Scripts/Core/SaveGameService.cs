using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coordinates saving the game by collecting state from every registered
/// <see cref="ISaveable"/> into a single <see cref="SaveData"/> and writing it to disk.
/// Should be an improvement over my previous solution.
/// </summary>
public sealed class SaveGameService {
    private readonly IReadOnlyList<ISaveable> saveables;

    private const string SAVE_FILE_NAME = "save.json";

    public SaveGameService(IReadOnlyList<ISaveable> saveables) {
        this.saveables = saveables;
    }

    /// <summary>
    /// Builds a save snapshot from all saveables and puts it all into save.json.
    /// Will later come back to this to support multiple save files.
    /// </summary>
    public void Save() {
        SaveData data = new SaveData {
            savedAtUTC = DateTimeOffset.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
            gameVersion = Application.version
        };

        foreach (ISaveable saveable in saveables) {
            if (saveable == null) continue;
            saveable.CaptureState(data);
        }

        DataSaver.Save(data, SAVE_FILE_NAME);
    }


    /// <summary>
    /// Requests data from save.json when called so it can restore the game state to that point
    /// </summary>
    /// <returns>Save Data</returns>
    public SaveData Load() {
        SaveData data = DataSaver.Load<SaveData>(SAVE_FILE_NAME);

        if (data.session == null)
            data.session = new SessionSaveData();

        if (data.player == null)
            data.player = new PlayerSaveData();

        if (data.world == null)
            data.world = new WorldSaveData();

        foreach (ISaveable saveable in saveables) {
            if (saveable == null)
                continue;

            saveable.LoadState(data);
        }

        return data;
    }
}