using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer instance { get; private set; }
    public AudioSource Music;
    private bool changeTrack, changed;
    private AudioClip trackTo;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        Music.ignoreListenerPause = true;
    }

    private void Update()
    {
        if (changeTrack) MusicChanging();
    }

    public void ChangeMusic(AudioClip newMusic) => (changeTrack, trackTo, changed) = (true, newMusic, false);

    public void StartMusic(AudioClip newMusic)
    {
        Music.Stop();
        Music.volume = 1f;
        Music.clip = newMusic;
        Music.Play();
    }

    public void StopMusic() => (changeTrack, trackTo, changed) = (true, null, false);

    private void MusicChanging()
    {
        if (!changed) Music.volume -= Time.deltaTime / 2;
        if (Music.volume <= 0 && !changed && trackTo != null)
        {
            changed = true;
            Music.clip = trackTo;
            Music.Play();
        }
        if (changed) Music.volume += Time.deltaTime;
        if (Music.volume >= 1f && changed) changeTrack = false;
    }
}