using UnityEngine;

[CreateAssetMenu(fileName = "KeyBehavior", menuName = "SCP:CBR/Behaviors/Key")]
public class KeycardBehavior : ItemBehavior {
    public int clearanceLevel = 1;

    public override void OnDoubleClick(ItemData itemData) {
        throw new System.NotImplementedException();
    }
}