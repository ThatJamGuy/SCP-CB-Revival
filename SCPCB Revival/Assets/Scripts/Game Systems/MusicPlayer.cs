using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }
    public AudioSource Music;
    private bool isChanging;
    private Coroutine currentCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        Music.ignoreListenerPause = true;
    }

    public void ChangeMusic(AudioClip newMusic)
    {
        if (!isChanging)
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            currentCoroutine = StartCoroutine(FadeOutAndChangeMusic(newMusic));
        }
    }

    public void StopMusic()
    {
        if (!isChanging)
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            currentCoroutine = StartCoroutine(FadeOutAndStopMusic());
        }
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
        yield return StartCoroutine(FadeOut(1f, Music.volume));

        // Load the new music asynchronously if not already loaded
        if (newMusic.loadState != AudioDataLoadState.Loaded)
        {
            yield return StartCoroutine(LoadAudioAsync(newMusic));
        }

        // Assign new clip and play
        Music.clip = newMusic;
        Music.Play();

        // Fade in
        yield return StartCoroutine(FadeIn(1f, 0f));

        isChanging = false;
        currentCoroutine = null;
    }

    private IEnumerator FadeOutAndStopMusic()
    {
        isChanging = true;

        // Fade out
        yield return StartCoroutine(FadeOut(1f, Music.volume));

        // Stop the music after fade out
        Music.Stop();

        isChanging = false;
        currentCoroutine = null;
    }

    private IEnumerator FadeOut(float duration, float startVolume)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Music.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Music.volume = 0f;
    }

    private IEnumerator FadeIn(float duration, float startVolume)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            Music.volume = Mathf.Lerp(startVolume, 1f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Music.volume = 1f;
    }

    private IEnumerator LoadAudioAsync(AudioClip clip)
    {
        // If the clip is external, you can load it via UnityWebRequest for true async loading
        if (clip.loadState != AudioDataLoadState.Loaded)
        {
            clip.LoadAudioData();
            while (clip.loadState != AudioDataLoadState.Loaded)
            {
                yield return null;
            }
        }
    }
}