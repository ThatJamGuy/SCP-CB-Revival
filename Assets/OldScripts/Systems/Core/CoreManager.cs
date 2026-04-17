using UnityEngine;

public class CoreManager : MonoBehaviour {
    private void Start() {
        // Core setup for the game

        SceneController.instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
            .WithOverlay()
            .Perform();
    }
}