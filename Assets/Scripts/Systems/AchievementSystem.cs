using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global system to handle achievement things
/// </summary>
public class AchievementSystem : MonoBehaviour {
    public static AchievementSystem Instance { get; private set; }

    public static int TotalAchievements { get; private set; }
    public static int ObtainedAchievementsCount => obtainedAchievementNames.Count;

    private const string ACHIEVMENTS_FILE_NAME = "achievements.json";

    [SerializeField] private AchievementData[] achievements;
    [SerializeField] private EventReference achievementUnlockStinger;
    [SerializeField] private GameObject achievementItemPrefab;
    [SerializeField] private Transform achievementContainer;

    private static readonly HashSet<string> obtainedAchievementNames = new HashSet<string>();
    private AchievementFile achievementFileData;

    private GameObject currentAchievementItem = null;

    #region Unity Callbacks

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() {
        achievementFileData = DataSaver.Load<AchievementFile>(ACHIEVMENTS_FILE_NAME);
        TotalAchievements = achievements.Length;

        // Fill the obtainedAchievementNames hashset with achievement names from the achievements file
        if (achievementFileData?.obtainedAchievementIDs != null) {
            foreach (string achievementID in achievementFileData.obtainedAchievementIDs) {
                if (!string.IsNullOrEmpty(achievementID)) obtainedAchievementNames.Add(achievementID);
            }
        }

        // Create the visual representations of each achievement and assign it's data accordingly
        foreach (AchievementData achievementData in achievements) {
            currentAchievementItem = Instantiate(achievementItemPrefab, achievementContainer);
            currentAchievementItem.GetComponent<AchivementDisplay>().DisplayAchievementValues(achievementData);
        }
    }

    #endregion

    #region Private Methods

    private void SaveAchievementsToFile() {
        string[] names = new string[obtainedAchievementNames.Count];
        obtainedAchievementNames.CopyTo(names);

        achievementFileData = new AchievementFile { obtainedAchievementIDs = names };
        DataSaver.Save(achievementFileData, ACHIEVMENTS_FILE_NAME);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Returns a true or false value based on if the defined achievement is unlocked.
    /// </summary>
    /// <param name="achievementIdentifier">ID of the achievement to check</param>
    /// <returns></returns>
    public bool AchievementUnlocked(string achievementIdentifier) {
        if (obtainedAchievementNames.Contains(achievementIdentifier)) return true;
        else return false;
    }

    /// <summary>
    /// Gives the player an achievement via that achievements identifier
    /// </summary>
    /// <param name="achievementIdentifier">Identifier for this achievement. (IE. "achv_914")</param>
    public void GiveAchievement(string achievementIdentifier) {
        foreach (var achievement in achievements) {
            if (SettingsManager.settingsData.consoleEnabled) {
                Debug.Log("<color=#ff0000>Tried to give you an achievement, but it looks like you have the console enabled!");
                return;
            }

            if (achievement.achievementIdentifier != achievementIdentifier) continue;
            if (obtainedAchievementNames.Contains(achievement.achievementIdentifier)) return;

            obtainedAchievementNames.Add(achievement.achievementIdentifier);
            SaveAchievementsToFile();

            CanvasInstance.Instance.achievementName.text = achievement.achievementName;
            CanvasInstance.Instance.achievementDesc.text = achievement.achievementDescription;
            CanvasInstance.Instance.achievementIcon.sprite = achievement.achievementIcon;
            CanvasInstance.Instance.HUD_AchievementPopup.Play("HUD_AchievementPopup");
            AudioManager.PlayOneShot(achievementUnlockStinger, transform.position);

            Debug.Log($"Gave the player achievement: '{achievement.achievementName}' " +
                      $"({ObtainedAchievementsCount}/{TotalAchievements})");
            return;
        }

        Debug.LogWarning($"No achievement found with name: '{achievementIdentifier}'");
    }

    #endregion
}

[System.Serializable]
public class AchievementFile {
    public string[] obtainedAchievementIDs;
}