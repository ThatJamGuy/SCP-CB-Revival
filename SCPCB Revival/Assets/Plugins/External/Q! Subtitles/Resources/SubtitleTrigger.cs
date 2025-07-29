using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SubtitleEvent {
    public string text = "";
    public float startTime;
    public float duration = 2f;
    public SubtitleEffect effect = SubtitleEffect.Fade;
    public bool overrideGlobalSettings;

    public float EndTime => startTime + duration;
}

[Serializable]
public class AudioSubtitleTrack {
    public AudioClip audioClip;
    public List<SubtitleEvent> events = new List<SubtitleEvent>();
    public bool loop;

    [NonSerialized] public float[] waveformData;
    [NonSerialized] public bool waveformGenerated;
}

public enum TriggerMode {
    Proximity,
    OnStart,
    Manual,
    OnTriggerEnter
}

[RequireComponent(typeof(AudioSource))]
public class SubtitleTrigger : MonoBehaviour {
    [Header("Trigger Settings")]
    public TriggerMode mode = TriggerMode.Proximity;
    public float hearingRadius = 10f;
    public string playerTag = "Player";
    public LayerMask playerLayers = -1;

    [Header("Audio Settings")]
    public List<AudioSubtitleTrack> tracks = new List<AudioSubtitleTrack>();
    public bool playRandomTrack;
    public float cooldownTime = 1f;

    [Header("Subtitle Defaults")]
    public SubtitleEffect defaultEffect = SubtitleEffect.Fade;

    private AudioSource audioSource;
    private Coroutine subtitleCoroutine;
    private int currentTrackIndex;
    private float lastTriggerTime = -999f;
    private bool isPlaying;
    private Transform cachedPlayer;
    private float cacheUpdateInterval = 0.1f;
    private float lastCacheUpdate;

    void Start() {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (mode == TriggerMode.OnStart) {
            PlayRandomTrack();
        }
    }

    void Update() {
        if (mode != TriggerMode.Proximity) return;

        if (Time.time - lastCacheUpdate > cacheUpdateInterval) {
            cachedPlayer = FindNearestPlayer();
            lastCacheUpdate = Time.time;
        }

        if (cachedPlayer == null) {
            if (isPlaying) StopCurrentSubtitles();
            return;
        }

        float distance = Vector3.Distance(transform.position, cachedPlayer.position);
        bool inRange = distance <= hearingRadius;

        if (inRange && !isPlaying && CanTrigger()) {
            PlayRandomTrack();
        }
        else if (!inRange && isPlaying) {
            StopCurrentSubtitles();
        }
    }

    public void PlayTrack(int index) {
        if (index < 0 || index >= tracks.Count || !CanTrigger()) return;

        currentTrackIndex = index;
        var track = tracks[index];

        if (track.audioClip == null) return;

        StopCurrentSubtitles();

        audioSource.clip = track.audioClip;
        audioSource.loop = track.loop;
        audioSource.pitch = 1f;

        audioSource.Play();
        subtitleCoroutine = StartCoroutine(PlaySubtitles(track));
        isPlaying = true;
        lastTriggerTime = Time.time;
    }

    public void PlayRandomTrack() {
        if (tracks.Count == 0) return;
        int randomIndex = playRandomTrack ? UnityEngine.Random.Range(0, tracks.Count) : 0;
        PlayTrack(randomIndex);
    }

    public void StopCurrentSubtitles() {
        if (subtitleCoroutine != null) {
            StopCoroutine(subtitleCoroutine);
            subtitleCoroutine = null;
        }

        if (audioSource.isPlaying) {
            audioSource.Stop();
        }

        isPlaying = false;
    }

    private bool CanTrigger() {
        return Time.time - lastTriggerTime >= cooldownTime;
    }

    private AudioSubtitleTrack GetCurrentTrack() {
        return currentTrackIndex < tracks.Count ? tracks[currentTrackIndex] : null;
    }

    private IEnumerator PlaySubtitles(AudioSubtitleTrack track) {
        var events = track.events;
        if (events.Count == 0) yield break;

        events.Sort((a, b) => a.startTime.CompareTo(b.startTime));

        int eventIndex = 0;
        float startTime = Time.time;

        while (isPlaying && (audioSource.isPlaying || track.loop)) {
            float audioTime = track.loop ?
                ((Time.time - startTime) % track.audioClip.length) :
                (Time.time - startTime);

            if (eventIndex < events.Count && audioTime >= events[eventIndex].startTime) {
                var subtitleEvent = events[eventIndex];
                ShowSubtitle(subtitleEvent);
                eventIndex++;

                if (track.loop && eventIndex >= events.Count) {
                    eventIndex = 0;
                    startTime = Time.time - audioTime + track.audioClip.length;
                }
            }

            yield return null;
        }

        isPlaying = false;
    }

    private void ShowSubtitle(SubtitleEvent subtitleEvent) {
        var effect = subtitleEvent.overrideGlobalSettings ? subtitleEvent.effect : defaultEffect;

        Subtitles.Show(subtitleEvent.text, subtitleEvent.duration, effect);
    }

    private Transform FindNearestPlayer() {
        Collider[] colliders = Physics.OverlapSphere(transform.position, hearingRadius, playerLayers);
        Transform closest = null;
        float minDistance = float.MaxValue;

        foreach (var col in colliders) {
            if (col.CompareTag(playerTag)) {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < minDistance) {
                    closest = col.transform;
                    minDistance = distance;
                }
            }
        }

        return closest;
    }

    private void OnTriggerEnter(Collider other) {
        if (mode == TriggerMode.OnTriggerEnter && other.CompareTag(playerTag) && CanTrigger()) {
            PlayRandomTrack();
        }
    }

    private void OnDrawGizmos() {
        if (mode == TriggerMode.Proximity) {
            Gizmos.color = isPlaying ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, hearingRadius);

            if (cachedPlayer != null) {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, cachedPlayer.position);
            }
        }
    }

    public void GenerateWaveformData(int trackIndex) {
        if (trackIndex < 0 || trackIndex >= tracks.Count) return;

        var track = tracks[trackIndex];
        if (track.audioClip == null) return;

        int samples = track.audioClip.samples * track.audioClip.channels;
        float[] data = new float[samples];
        track.audioClip.GetData(data, 0);

        int waveformResolution = Mathf.Min(2048, samples / 10);
        track.waveformData = new float[waveformResolution];

        int samplesPerPoint = samples / waveformResolution;

        for (int i = 0; i < waveformResolution; i++) {
            float max = 0f;
            int start = i * samplesPerPoint;
            int end = Mathf.Min(start + samplesPerPoint, samples);

            for (int j = start; j < end; j++) {
                max = Mathf.Max(max, Mathf.Abs(data[j]));
            }

            track.waveformData[i] = max;
        }

        track.waveformGenerated = true;
    }
}