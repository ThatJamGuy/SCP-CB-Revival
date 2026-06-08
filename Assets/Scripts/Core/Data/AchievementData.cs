using UnityEngine;

[CreateAssetMenu(fileName = "AchievementData", menuName = "SCP:CBR/Achievement Data")]
public class AchievementData : ScriptableObject {
    public string achievementIdentifier;
    public string achievementName;
    public string achievementDescription;
    public Sprite achievementIcon;
}