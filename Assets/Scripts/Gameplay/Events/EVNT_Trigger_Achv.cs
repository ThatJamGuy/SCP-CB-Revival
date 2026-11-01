using EditorAttributes;
using UnityEngine;

// Extension for other EVNT_Trigger based systems that gives an achievement when entering the zone
public class EVNT_Trigger_Achv : MonoBehaviour {
    [SerializeField] private string achievementToGive;
    [SerializeField] private bool gameModeExclusive;
    [SerializeField, ShowField(nameof(gameModeExclusive))] private int requiredDifficulty;

    public void GiveAchivement() {
        if (achievementToGive == null) return;
        if (gameModeExclusive && RevivalSessionEngine.currentDifficulty != requiredDifficulty) {
            return;
        }

        RevivalRuntimeEngine.Instance.GiveAchievement(achievementToGive);
    }
}