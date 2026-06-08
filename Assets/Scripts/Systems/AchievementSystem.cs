using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using IngameDebugConsole;

/// <summary>
/// Global system to handle achievement things
/// </summary>
public class AchievementSystem : MonoBehaviour {
    public static int TotalAchievements { get; private set; }
    public static int ObtainedAchievementsCount => obtainedAchievementNames.Count;
    
    private const string ACHIEVMENTS_FILE_NAME = "achievements.json";
    
    [SerializeField] private AchievementData[] achievements;
    [SerializeField] private EventReference achievementUnlockStinger;
    
    private static readonly HashSet<string> obtainedAchievementNames = new HashSet<string>();
    private AchievementFile achievementFileData;

    #region Unity Callbacks
    
    private void Start() {
        achievementFileData = DataSaver.Load<AchievementFile>(ACHIEVMENTS_FILE_NAME);
        TotalAchievements = achievements.Length;

        // Fill the obtainedAchievementNames hashset with achievement names from the achievements file
        if (achievementFileData?.obtainedAchievementIDs != null) {
            foreach (string achievementID in achievementFileData.obtainedAchievementIDs) {
                if (!string.IsNullOrEmpty(achievementID)) obtainedAchievementNames.Add(achievementID);
            }
        }
        
        DebugLogConsole.AddCommand<string>("giveachievement", "Gives the player an achievement via it's identifier", GiveAchievement);
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
    /// Gives the player an achievement via that achievements identifier
    /// </summary>
    /// <param name="achievementIdentifier">Identifier for this achievement. (IE. "achv_914")</param>
    public void GiveAchievement(string achievementIdentifier) {
        foreach (var achievement in achievements) {
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