using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModernMenu : MonoBehaviour
{
    private void Awake() {
        MusicPlayer.Instance.StartMusicByName("Menu");
    }

    public void QuitGame() {
        Application.Quit();
    }
}