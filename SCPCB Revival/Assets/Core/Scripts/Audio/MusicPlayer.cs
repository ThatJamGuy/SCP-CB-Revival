using System.Collections;
using UnityEngine;

namespace scpcbr {
    public class MusicPlayer : MonoBehaviour {
        public static MusicPlayer Instance { get; private set; }

        public Soundtrack[] soundtracks;

        AudioSource musicSource;
        Soundtrack currentSoundtrack;
        string currentTrackName;

        void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = GetComponent<AudioSource>();
            musicSource.ignoreListenerPause = true;
        }

        public void StartMusic(AudioClip music) {
            if (music == null) return;
            musicSource.Stop();
            musicSource.clip = music;
            musicSource.Play();
        }

        public void StartMusicByName(string trackName) {
            if (string.IsNullOrEmpty(trackName)) return;
            if (currentSoundtrack == null) {
                int index = SettingsManager.Instance?.CurrentSettings?.soundtrack ?? 0;
                if (soundtracks.Length > index) currentSoundtrack = soundtracks[index];
            }
            if (currentSoundtrack == null) return;

            var track = System.Array.Find(currentSoundtrack.tracks, t => t.trackName == trackName);
            if (track?.clip == null) return;

            currentTrackName = trackName;
            musicSource.Stop();
            musicSource.clip = track.clip;
            musicSource.Play();
        }

        public void ChangeMusic(string trackName) {
            if (trackName == currentTrackName || string.IsNullOrEmpty(trackName)) return;
            currentTrackName = trackName;
            StopAllCoroutines();
            StartCoroutine(FadeToTrack(trackName, 0.5f, 0.5f));
        }

        IEnumerator FadeToTrack(string trackName, float fadeOutTime, float fadeInTime) {
            float timer = 0f;
            while (timer < fadeOutTime) {
                musicSource.volume = Mathf.Lerp(1f, 0f, timer / fadeOutTime);
                timer += Time.deltaTime;
                yield return null;
            }
            musicSource.Stop();

            if (currentSoundtrack == null) currentSoundtrack = soundtracks.Length > 0 ? soundtracks[0] : null;
            var track = System.Array.Find(currentSoundtrack?.tracks, t => t.trackName == trackName);
            if (track?.clip == null) yield break;

            musicSource.clip = track.clip;
            musicSource.Play();
            timer = 0f;
            while (timer < fadeInTime) {
                musicSource.volume = Mathf.Lerp(0f, 1f, timer / fadeInTime);
                timer += Time.deltaTime;
                yield return null;
            }
            musicSource.volume = 1f;
        }

        public void SetCurrentSoundtrack(int index) {
            if (soundtracks == null || index < 0 || index >= soundtracks.Length || string.IsNullOrEmpty(currentTrackName)) return;
            currentSoundtrack = soundtracks[index];
            StartMusicByName(currentTrackName);
        }
    }
}