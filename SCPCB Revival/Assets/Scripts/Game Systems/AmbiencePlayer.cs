using System.Collections;
using UnityEngine;

public class AmbiencePlayer : MonoBehaviour
{
    public static AmbiencePlayer Instance { get; private set; }
    private AudioSource ambienceSource;

    private float maxAmbienceVolume = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        //DontDestroyOnLoad(gameObject);

        ambienceSource = GetComponent<AudioSource>();
        ambienceSource.ignoreListenerPause = true;
    }

    public void StartMusic(AudioClip ambience)
    {   
        ambienceSource.Stop();
        ambienceSource.clip = ambience;
        ambienceSource.Play();
    }

    public void SetAmbienceVolume(float volume)
    {
        maxAmbienceVolume = volume;
        ambienceSource.volume = volume;
    }

    public void ChangeAmbience(AudioClip newAmbience)
    {
        StartCoroutine(ChangeAmbienceCoroutine(newAmbience));
    }
    
    private IEnumerator ChangeAmbienceCoroutine(AudioClip newMusic)
    {
        float fadeOutTime = 0.3f;
        float fadeInTime = 0.3f;
    
        float timer = 0f;
        while (timer < fadeOutTime)
        {
            ambienceSource.volume = Mathf.Lerp(maxAmbienceVolume, 0f, timer / fadeOutTime);
            timer += Time.deltaTime;
            yield return null;
        }
        ambienceSource.Stop();
    
        ambienceSource.clip = newMusic;
        ambienceSource.Play();
        timer = 0f;
        while (timer < fadeInTime)
        {
            ambienceSource.volume = Mathf.Lerp(0f, maxAmbienceVolume, timer / fadeInTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}