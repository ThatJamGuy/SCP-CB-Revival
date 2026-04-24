[System.Serializable]
public class SaveData {
    // Variables for the save system to keep track of
    // Some values are set in the main menu before the game starts like the save name and seed
    
    public string currentSaveName = "Untitled"; // Default save name, mostly for devs
    public string currentMapSeed = "DefaultSeed"; // Default seed value, also mostly for devs
    public int difficulty; // 0 = Easy, 1 = Euclid, 2 = Keter
}