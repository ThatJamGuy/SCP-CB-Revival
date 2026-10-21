using UnityEngine;

[CreateAssetMenu(fileName = "KeyBehavior", menuName = "SCP:CBR/Behaviors/Gas Mask")]
public class ITM_GasMask : ItemBehavior {
    public bool superMask;

    private bool equipped;

    public override void OnDoubleClick(ItemData itemData) {

        // Player already is wearing a gas mask, just not this one
        if (Player.wearingGasMask && !equipped) {

            // Take off the old one
            Player.wearingGasMask = false;
            Player.Instance.staminaDepletionModifier = 0f;
            CanvasInstance.Instance.gasMaskOverlay.SetActive(false);

            // Continue on to further stuff
        }

        // Already have this item on, take it off
        if (equipped) {
            // Revert effects, disable overlay, ignore rest of method
            Player.wearingGasMask = false;
            if (superMask) Player.Instance.staminaDepletionModifier = 0f;
            CanvasInstance.Instance.gasMaskOverlay.SetActive(false);

            equipped = false;

            return;
        }

        if (!superMask) {
            // Enable overlay, set isWearingGasMask to true so future systems know if applicable
            Player.wearingGasMask = true;
            CanvasInstance.Instance.gasMaskOverlay.SetActive(true);
        } else {
            // Enable overlay, set bool, slow stamina depletion, notify player
            Player.wearingGasMask = true;
            Player.Instance.staminaDepletionModifier = -0.7f;
            RevivalSessionEngine.Instance.NotifyPlayer("You put on the gas mask and feel you can breathe easier.");
            CanvasInstance.Instance.gasMaskOverlay.SetActive(true);
        }

        equipped = true;
    }
}