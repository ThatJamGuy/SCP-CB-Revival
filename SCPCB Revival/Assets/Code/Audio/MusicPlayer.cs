using System.Collections;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }
    public Soundtrack[] soundtracks;

    private AudioSource musicSource;
    private Soundtrack currentSoundtrack;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();
        musicSource.ignoreListenerPause = true;
    }

    public void StartMusic(AudioClip music)
    {
        musicSource.Stop();
        musicSource.clip = music;
        musicSource.Play();
    }

    public void StartMusicByName(string trackName) {
        if (currentSoundtrack == null && soundtracks.Length > 0) currentSoundtrack = soundtracks[0];
        if (currentSoundtrack == null) return;

        var track = System.Array.Find(currentSoundtrack.tracks, t => t.trackName == trackName);
        if (track == null || track.clip == null) return;

        musicSource.Stop();
        musicSource.clip = track.clip;
        musicSource.Play();
    }

    public void ChangeMusic(AudioClip newMusic)
    {
        StartCoroutine(ChangeMusicCoroutine(newMusic));
    }

    private IEnumerator ChangeMusicCoroutine(AudioClip newMusic)
    {
        float fadeOutTime = 0.5f;
        float fadeInTime = 0.5f;

        float timer = 0f;
        while (timer < fadeOutTime)
        {
            musicSource.volume = Mathf.Lerp(1f, 0f, timer / fadeOutTime);
            timer += Time.deltaTime;
            yield return null;
        }
        musicSource.Stop();

        musicSource.clip = newMusic;
        musicSource.Play();
        timer = 0f;
        while (timer < fadeInTime)
        {
            musicSource.volume = Mathf.Lerp(0f, 1f, timer / fadeInTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}