using UnityEngine;

public class MainMenuController : MonoBehaviour {
    [SerializeField] private int menuMusicID = 0;

    private void Start() {
        MusicManager.instance.SetMusicState(menuMusicID);
    }

    public void QuitGame() {
        Application.Quit();
    }
}