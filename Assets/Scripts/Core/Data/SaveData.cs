using UnityEngine;

[System.Serializable]
public class SaveData {
    // Variables for the save system to keep track of
    // Some values are set in the main menu before the game starts like the save name and seed
    // Will later learn to encrypt these files so it's not so easy to cheat the save file
    
    public string currentSaveName = "Untitled"; // Default save name, mostly for devs
    public string currentMapSeed = "DefaultSeed"; // Default seed value, also mostly for devs
    public int difficulty; // 0 = Easy, 1 = Euclid, 2 = Keter
    public int currentZone = 1; // 0 = Intro, 1 = LCZ (Default for now CHANGE LATER), 2 = HCZ, 3 = EZ
    public Vector3 playerPos; // The last known X,Y,Z coordinates of the player so loading works
}