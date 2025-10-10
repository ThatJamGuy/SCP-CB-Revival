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
        float nextTriggerTime = float.MaxValue;
        float lastClipPos;
        double lastDSPTime;
        AudioClip lastClip;
        bool wasInRange;

        static readonly List<Transform> listeners = new List<Transform>();
        Dictionary<AudioClip, int> clipToGroupMap;

        public static void RegisterListener(Transform listener) {
            if (listener == null) return;
            if (!listeners.Contains(listener)) listeners.Add(listener);
        }

        public static void UnregisterListener(Transform listener) {
            if (listener == null) return;
            listeners.Remove(listener);
        }

        void Awake() {
            source = GetComponent<AudioSource>();
            lastDSPTime = AudioSettings.dspTime;
            lastClip = source.clip;
            BuildClipMap();
        }

        void BuildClipMap() {
            clipToGroupMap = new Dictionary<AudioClip, int>();
            if (subGroups == null) return;

            for (int i = subGroups.Length - 1; i >= 0; i--) {
                var g = subGroups[i];
                if (g?.associatedVoiceLine != null) {
                    clipToGroupMap[g.associatedVoiceLine] = i;
                }
            }
        }

        void Update() {
            UpdateTargetListener();

            if (source.clip != lastClip) {
                lastClip = source.clip;
                activeGroup = clipToGroupMap.TryGetValue(source.clip, out int groupIndex) ? groupIndex : -1;
                subIndex = 0;
                nextTriggerTime = float.MaxValue;
                lastClipPos = source.time;
                lastDSPTime = AudioSettings.dspTime;

                if (activeGroup != -1) {
                    var subs = subGroups[activeGroup].subtitles;
                    nextTriggerTime = (subs?.Length > 0) ? subs[0].startTime : float.MaxValue;
                }
            }

            if (!source.isPlaying) { activeGroup = -1; return; }
            if (source.clip == null) return;

            float clipPos = source.time;

            if (activeGroup == -1 || LoopDetected(clipPos)) {
                activeGroup = clipToGroupMap.TryGetValue(source.clip, out int groupIndex) ? groupIndex : -1;
                subIndex = 0;
                if (activeGroup != -1) {
                    var subs = subGroups[activeGroup].subtitles;
                    nextTriggerTime = (subs?.Length > 0) ? subs[0].startTime : float.MaxValue;
                }
                else {
                    nextTriggerTime = float.MaxValue;
                }
            }

            bool inRange = targetListener != null && Vector3.SqrMagnitude(targetListener.position - transform.position) <= emissionRadius * emissionRadius;

            if (activeGroup != -1 && !wasInRange && inRange) {
                var subs = subGroups[activeGroup].subtitles;
                if (subs != null) {
                    for (int i = 0; i < subs.Length; i++) {
                        var s = subs[i];
                        if (clipPos >= s.startTime && clipPos <= s.startTime + s.duration) {
                            Subtitles.Show(s.text, s.startTime + s.duration - clipPos);
                            subIndex = i + 1;
                            nextTriggerTime = subIndex < subs.Length ? subs[subIndex].startTime : float.MaxValue;
                            break;
                        }
                    }
                }
            }

            if (activeGroup != -1 && inRange) {
                var subs = subGroups[activeGroup].subtitles;
                if (subs != null && subIndex < subs.Length && clipPos >= nextTriggerTime) {
                    var s = subs[subIndex];
                    Subtitles.Show(s.text, s.duration);
                    subIndex++;
                    nextTriggerTime = subIndex < subs.Length ? subs[subIndex].startTime : float.MaxValue;
                }
            }

            wasInRange = inRange;
            lastClipPos = clipPos;
            lastDSPTime = AudioSettings.dspTime;
        }

        void UpdateTargetListener() {
            for (int i = listeners.Count - 1; i >= 0; i--) if (listeners[i] == null) listeners.RemoveAt(i);
            if (listeners.Count == 0) { targetListener = null; return; }
            float closest = float.MaxValue;
            Transform best = null;
            for (int i = 0; i < listeners.Count; i++) {
                var l = listeners[i];
                float d = Vector3.SqrMagnitude(l.position - transform.position);
                if (d < closest) { closest = d; best = l; }
            }
            targetListener = best;
        }

        bool LoopDetected(float currentPos) {
            if (source.clip == null) return false;
            double dspDelta = AudioSettings.dspTime - lastDSPTime;
            return currentPos < lastClipPos && dspDelta < source.clip.length * 1.5;
        }

        void OnValidate() {
            if (Application.isPlaying) BuildClipMap();
        }
    }

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
}