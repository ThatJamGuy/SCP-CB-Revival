using UnityEngine;
using System.Collections.Generic;

namespace scpcbr {
    [RequireComponent(typeof(AudioSource))]
    public class SubtitleEmitter : MonoBehaviour {
        public float emissionRadius = 10f;
        public SubGroup[] subGroups;

        AudioSource source;
        Transform targetListener;
        int activeGroup = -1;
        int subIndex;
        float nextTriggerTime;
        float lastClipPos;
        double lastDSPTime;
        bool inRange, wasInRange;

        static readonly List<Transform> listeners = new List<Transform>();

        #region Player Listener
        public static void RegisterListener(Transform listener) {
            if (!listeners.Contains(listener)) listeners.Add(listener);
        }

        public static void UnregisterListener(Transform listener) {
            listeners.Remove(listener);
        }
        #endregion

        #region Default Methods
        void Awake() {
            source = GetComponent<AudioSource>();
        }

        void Update() {
            UpdateTargetListener();
            if (!source.isPlaying) { activeGroup = -1; wasInRange = false; return; }

            float clipPos = source.time;
            if (activeGroup == -1 || LoopDetected(clipPos)) {
                for (int i = 0; i < subGroups.Length; i++) {
                    if (subGroups[i].associatedVoiceLine == source.clip) {
                        activeGroup = i;
                        subIndex = 0;
                        nextTriggerTime = subGroups[i].subtitles.Length > 0 ? subGroups[i].subtitles[0].startTime : float.MaxValue;
                        break;
                    }
                }
            }

            inRange = targetListener && Vector3.SqrMagnitude(targetListener.position - transform.position) <= emissionRadius * emissionRadius;

            if (!wasInRange && inRange && activeGroup != -1) {
                for (int i = 0; i < subGroups[activeGroup].subtitles.Length; i++) {
                    var sub = subGroups[activeGroup].subtitles[i];
                    if (clipPos >= sub.startTime && clipPos <= sub.startTime + sub.duration) {
                        Subtitles.Show(sub.text, sub.startTime + sub.duration - clipPos);
                        subIndex = i + 1;
                        nextTriggerTime = subIndex < subGroups[activeGroup].subtitles.Length ? subGroups[activeGroup].subtitles[subIndex].startTime : float.MaxValue;
                        break;
                    }
                }
            }

            if (inRange && subIndex < subGroups[activeGroup].subtitles.Length && clipPos >= nextTriggerTime) {
                var sub = subGroups[activeGroup].subtitles[subIndex];
                Subtitles.Show(sub.text, sub.duration);
                subIndex++;
                nextTriggerTime = subIndex < subGroups[activeGroup].subtitles.Length ? subGroups[activeGroup].subtitles[subIndex].startTime : float.MaxValue;
            }

            wasInRange = inRange;
            lastClipPos = clipPos;
            lastDSPTime = AudioSettings.dspTime;
        }
        #endregion

        #region Private Methods
        private void UpdateTargetListener() {
            if (listeners.Count == 0) { targetListener = null; return; }
            float closest = float.MaxValue;
            foreach (var l in listeners) {
                if (!l) continue;
                float dist = Vector3.SqrMagnitude(l.position - transform.position);
                if (dist < closest) { closest = dist; targetListener = l; }
            }
        }

        private bool LoopDetected(float currentPos) {
            double dspDelta = AudioSettings.dspTime - lastDSPTime;
            return currentPos < lastClipPos && dspDelta < source.clip.length * 1.5;
        }
        #endregion
    }

    #region Additional Classes
    [System.Serializable]
    public class SubGroup {
        public AudioClip associatedVoiceLine;
        public Sub[] subtitles;
    }

    [System.Serializable]
    public class Sub {
        public float startTime;
        public float duration = 5f;
        [TextArea] public string text;
    }
    #endregion
}