using TMPro;
using UnityEngine;

public class DebugDisplay : MonoBehaviour {
    public static DebugDisplay instance;

    [SerializeField] private GameObject debugDisplayCanvas;
    [SerializeField] private TextMeshProUGUI playerPosText;

    private bool debugDisplayIsEnabled = true;

    private void Awake() {
        // Assume that this is the only one because I doubt I'll have two in any scene ever, yes I'm that confident
        instance = this;
    }

    private void Update() {
        if (!debugDisplayIsEnabled) return;

        // Show the player position via the playerPosText (formatted to two decimal places)
        var pos = PlayerAccessor.instance.GetPlayerPos();
        playerPosText.text = $"PosX: {pos.x:F2} PosY: {pos.y:F2} PosZ: {pos.z:F2}";
    }

    public void ToggleDebugDispaly(bool enable) {
        debugDisplayCanvas.SetActive(enable);
        debugDisplayIsEnabled = enable;
    }
}