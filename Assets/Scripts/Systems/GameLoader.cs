using FMODUnity;
using PrimeTween;

public static class GameLoader {
    // Main menu -> game. Use after SavegameEngine.NewGame() or SavegameEngine.LoadGame() succeeded
    public static void EnterFromMenu() {
        SceneController.instance
            .NewTransition()
            .Load(SceneDatabase.Slots.SESSION, SceneDatabase.Scenes.SESSION)
            .Load(SceneDatabase.Slots.GAME, SceneDatabase.Scenes.GAME, setActive: true)
            .Unload(SceneDatabase.Slots.MENU)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }

    // In game -> the same scenes again, reinitialized from the save on disk (pause menu, death screen, etc)
    // Returns false if there is no save to load, in which case nothing is touched
    public static bool ReloadGame() {
        if (SceneController.instance == null || !SaveGameEngine.HasSave())
            return false;

        // This is the actual load operation; clearing registrations is expected because
        // the current scenes are about to be unloaded and recreated
        if (!SaveGameEngine.LoadGame()) return false;

        RevivalSessionEngine.ResumeGame();
        RuntimeManager.GetBus("bus:/SFX").stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        Tween.StopAll();

        SceneController.instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.GAME)
            .Unload(SceneDatabase.Slots.SESSION)
            .Load(SceneDatabase.Slots.SESSION, SceneDatabase.Scenes.SESSION)
            .Load(SceneDatabase.Slots.GAME, SceneDatabase.Scenes.GAME, setActive: true)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();

        return true;
    }
}