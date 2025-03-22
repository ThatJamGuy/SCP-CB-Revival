using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SubtitleTiming
{
    public string text;
    public float startTime;
    public float duration;
}

[System.Serializable]
public class AudioSubtitlePair
{
    public AudioClip audioClip;
    public List<SubtitleTiming> subtitles = new List<SubtitleTiming>();
}

[RequireComponent(typeof(AudioSource))]
public class SubtitleTrigger : MonoBehaviour
{
    public List<AudioSubtitlePair> audioSubtitlePairs = new List<AudioSubtitlePair>();
    public float hearingRadius = 10f;
    public Transform player;
    private AudioSource audioSource;
    private bool isPlaying = false;
    private Coroutine currentSubtitleCoroutine;
    private AudioClip currentClip;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        currentClip = audioSource.clip;
    }

    private void Update()
    {
        if (player == null) return;

        // Check if audio clip has changed
        if (currentClip != audioSource.clip)
        {
            currentClip = audioSource.clip;
            OnAudioChanged(currentClip);
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= hearingRadius && !isPlaying && audioSource.clip != null)
        {
            StartCoroutine(PlaySubtitles());
        }
        else if (distance > hearingRadius && isPlaying)
        {
            StopAllCoroutines();
            isPlaying = false;
        }
    }

    private void OnAudioChanged(AudioClip newClip)
    {
        StopAllCoroutines();
        isPlaying = false;
        currentSubtitleCoroutine = null;

        if (newClip == null) return;

        var subtitlePair = audioSubtitlePairs.Find(pair => pair.audioClip == newClip);
        if (subtitlePair != null)
        {
            currentSubtitleCoroutine = StartCoroutine(PlaySubtitlesForClip(subtitlePair));
            isPlaying = true;
        }
    }

    private IEnumerator PlaySubtitles()
    {
        isPlaying = true;
        var subtitlePair = audioSubtitlePairs.Find(pair => pair.audioClip == audioSource.clip);
        if (subtitlePair != null)
        {
            yield return PlaySubtitlesForClip(subtitlePair);
        }
        isPlaying = false;
    }

    private IEnumerator PlaySubtitlesForClip(AudioSubtitlePair pair)
    {
        float clipLength = pair.audioClip.length;
        float elapsedTime = audioSource.time; // Start from current audio time
        int nextSubtitleIndex = 0;

        // Find the correct starting subtitle based on current audio time
        while (nextSubtitleIndex < pair.subtitles.Count &&
               pair.subtitles[nextSubtitleIndex].startTime < elapsedTime)
        {
            nextSubtitleIndex++;
        }

        while (elapsedTime < clipLength && nextSubtitleIndex < pair.subtitles.Count && audioSource.clip == pair.audioClip)
        {
            if (nextSubtitleIndex < pair.subtitles.Count &&
                elapsedTime >= pair.subtitles[nextSubtitleIndex].startTime)
            {
                Subtitles.Show(pair.subtitles[nextSubtitleIndex].text,
                              pair.subtitles[nextSubtitleIndex].duration);
                nextSubtitleIndex++;
            }

            yield return null;
            elapsedTime = audioSource.time;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
    }
}
