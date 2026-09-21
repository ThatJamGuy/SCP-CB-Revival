using System;
using UnityEngine;

[Serializable]
public sealed class SaveData {
    public int saveVersion = 1;
    public string savedAtUTC;
    public string gameVersion;

    public SessionSaveData session = new();
    public PlayerSaveData player = new();
    public WorldSaveData world = new();
}

[Serializable]
public sealed class SessionSaveData {
    public int difficulty;
    public int currentZone;
}

[Serializable]
public sealed class PlayerSaveData {
    public Vector3 position;
    public Quaternion rotation;
    public float stamina;
    public float blink;
}

[Serializable]
public sealed class WorldSaveData {
    public string mapSeed;
}