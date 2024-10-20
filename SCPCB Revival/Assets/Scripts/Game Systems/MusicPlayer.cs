using System.Collections;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }
    public AudioSource Music;
    private bool isChanging;
    private AudioClip trackTo;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        Music.ignoreListenerPause = true;
    }

    public void ChangeMusic(AudioClip newMusic)
    {
        if (!isChanging) StartCoroutine(FadeOutAndChangeMusic(newMusic));
    }

    public void StopMusic()
    {
        if (!isChanging) StartCoroutine(FadeOutAndStopMusic());
    }

    public void StartMusic(AudioClip newMusic)
    {
        Music.Stop();
        Music.volume = 1f;
        Music.clip = newMusic;
        Music.Play();
    }

    private IEnumerator FadeOutAndChangeMusic(AudioClip newMusic)
    {
        isChanging = true;

        // Fade out
        yield return StartCoroutine(FadeOut(1f));

        // Asynchronously load new music if not already loaded
        if (!newMusic.loadState.Equals(AudioDataLoadState.Loaded))
        {
            yield return StartCoroutine(LoadAudioAsync(newMusic));
        }

        // Change the clip and start playing
        Music.clip = newMusic;
        Music.Play();

        // Fade in
        yield return StartCoroutine(FadeIn(1f));

        isChanging = false;
    }

    private IEnumerator FadeOutAndStopMusic()
    {
        isChanging = true;

        // Fade out
        yield return StartCoroutine(FadeOut(1f));

        // Stop the music after fade out
        Music.Stop();

        isChanging = false;
    }

    private IEnumerator FadeOut(float duration)
    {
        float startVolume = Music.volume;

        while (Music.volume > 0)
        {
            Music.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        Music.volume = 0;
    }

    private IEnumerator FadeIn(float duration)
    {
        float targetVolume = 1f;

        while (Music.volume < targetVolume)
        {
            Music.volume += Time.deltaTime / duration;
            yield return null;
        }

        Music.volume = targetVolume;
    }

    private IEnumerator LoadAudioAsync(AudioClip clip)
    {
        // Wait until the audio clip is fully loaded
        while (clip.loadState != AudioDataLoadState.Loaded)
        {
            clip.LoadAudioData();
            yield return null;
        }
    }
}