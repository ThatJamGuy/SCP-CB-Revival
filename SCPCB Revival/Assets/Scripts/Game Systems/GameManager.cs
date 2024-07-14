using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private AudioClip zone1Music;

    private void Start()
    {
        MusicPlayer.instance.ChangeMusic(zone1Music);
    }
}