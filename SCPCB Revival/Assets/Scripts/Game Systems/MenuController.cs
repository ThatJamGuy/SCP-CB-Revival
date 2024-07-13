using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private AudioClip menuMusic;

    private void Start()
    {
        MusicPlayer.instance.StartMusic(menuMusic);
    }
}