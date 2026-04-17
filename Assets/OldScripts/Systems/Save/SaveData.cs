[System.Serializable]
public class SaveData {

    // Variables for the save system to keep track of
    // As of write now just holds the new game data so that the map generator knows what seed to use
    // Will later actually hold more stuff. A lot more stuff. Oh dear.

    public string currentSaveName;
    public string currentMapSeed;

    // Set some default values for the save I suppose
    public SaveData() {
        currentSaveName = "Untitled";
        currentMapSeed = "DefaultSeed";
    }
}