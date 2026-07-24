using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script attached to achievement display prefabs so that they can display the players achievements.
/// Upon initial value display the achievement data used for it is cached so the display can be refreshed later.
/// </summary>
public class AchivementDisplay : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private Color achievementLockedColor;
    [SerializeField] private Color achievementUnlockedColor;

    [Header("Referneces")]
    [SerializeField] private Image achievementBox;
    [SerializeField] private Sprite achievementLockedIcon;
    [SerializeField] private Image achievementIcon;
    [SerializeField] private TextMeshProUGUI achievementTitle;
    [SerializeField] private TextMeshProUGUI achievementDescription;

    private AchievementData cachedAchievementData;

    private bool unlocked;

    private void OnEnable() {
        // Do this stuff here so that checking achievements mid-game is possible
        if (cachedAchievementData == null) return;
        DisplayAchievementValues(cachedAchievementData);
    }

    public void DisplayAchievementValues(AchievementData achievementData) {
        if (cachedAchievementData == null) cachedAchievementData = achievementData;
        unlocked = AchievementSystem.Instance.AchievementUnlocked(achievementData.achievementIdentifier);

        if (unlocked) {
            // Set references for if the achievement is achieved
            achievementIcon.sprite = achievementData.achievementIcon;
            achievementTitle.text = achievementData.achievementName;
            achievementDescription.text = achievementData.achievementDescription;

            achievementBox.color = achievementUnlockedColor;
        } else {
            // Set references for if the achievement is not achieved
            achievementIcon.sprite = achievementLockedIcon;
            achievementTitle.text = "Achievement Locked";
            achievementDescription.text = achievementData.achievementDescription;

            achievementBox.color = achievementLockedColor;
        }
    }
}