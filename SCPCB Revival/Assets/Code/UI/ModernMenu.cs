using UnityEngine;

public class ModernMenu : MonoBehaviour
{
    private void Start() {
        MusicPlayer.Instance.StartMusicByName("Menu");
    }

    public void StartGame() {
        SceneLoader.Instance.LoadScene("LoadingScene", "TestRoom", LoadType.LoadHeavy);
    }

    public void OpenLink(string link) {
        Application.OpenURL(link);
    }

    public void QuitGame() {
        Application.Quit();
    }
}