using UnityEngine;

[System.Serializable]
public class SaveData {
    // Variables for the save system to keep track of
    // Some values are set in the main menu before the game starts like the save name and seed
    // Will later learn to encrypt these files so it's not so easy to cheat the save file

    // NOTE: Might later look into keeping generated room in a dictionary or something and saving it so the map doesn't
    // need to regenerate when loading the save. Would allow people to play their older saves without messing up the
    // map they were last playing on

    public bool newGame = true; // The value GameManager will read to determine if the player is starting a new game or loading one
    public string currentDateTime = "CurrentDateTime"; // The current date & time so you know when you last played
    public string currentGameVersion = "v0.0.5"; // The current version of the game, informative little info thing
    public string currentSaveName = "Untitled"; // Default save name, mostly for devs
    public string currentMapSeed = "DefaultSeed"; // Default seed value, also mostly for devs
    public int difficulty; // 0 = Easy, 1 = Euclid, 2 = Keter
    public int currentZone = 1; // 0 = Intro, 1 = LCZ (Default for now CHANGE LATER), 2 = HCZ, 3 = EZ
    public bool lczLockdownLifted = false;
    public Vector3 playerPos; // The last known X,Y,Z coordinates of the player so loading works
    public Quaternion playerRot; // The last known rotation of the player. Main player obj, not the camera
}