using UnityEngine;

public class PauseMenu : MonoBehaviour {
    public void ResumeGame() {
        IngameMenuManager.instance.ToggleMenuByID(2);
    }
}