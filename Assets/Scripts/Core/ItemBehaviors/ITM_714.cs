using UnityEngine;

[CreateAssetMenu(fileName = "B_SCP-714", menuName = "SCP:CBR/Behaviors/SCP-714")]
public class ITM_714 : ItemBehavior {
    private bool equipped;

    public override void OnDoubleClick(ItemData itemData) {
        if (equipped) {
            Player.wearing714 = false;
            Player.Instance.staminaDepletionModifier = 0;

            RevivalSessionEngine.Instance.NotifyPlayer("You took SCP-714 off of your finger.");

            equipped = false;

            return;
        }

        Player.wearing714 = true;
        Player.Instance.staminaDepletionModifier = 5f;

        RevivalRuntimeEngine.Instance.GiveAchievement("achv_714");
        RevivalSessionEngine.Instance.NotifyPlayer("You slipped SCP-714 onto your finger.");

        equipped = true;
    }
}