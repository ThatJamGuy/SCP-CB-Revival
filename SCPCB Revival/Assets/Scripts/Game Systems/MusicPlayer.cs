using System.Collections;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }
    private AudioSource musicSource;

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

    public void ChangeMusic(AudioClip newMusic)
    {
        StartCoroutine(ChangeMusicCoroutine(newMusic));
    }
    
    private IEnumerator ChangeMusicCoroutine(AudioClip newMusic)
    {
        float fadeOutTime = 1f;
        float fadeInTime = 1f;
    
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