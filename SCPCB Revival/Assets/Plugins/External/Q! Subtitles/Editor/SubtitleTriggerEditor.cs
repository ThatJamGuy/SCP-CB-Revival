using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SubtitleTrigger))]
public class SubtitleTriggerEditor : Editor {
    private bool showTriggerSettings = true;
    private bool showAudioSettings = true;
    private bool showSubtitleDefaults = true;
    private bool[] showTracks;
    private int selectedTrack = -1;
    private Vector2 timelineScrollPos;
    private float timelineZoom = 1f;
    private bool isDraggingEvent = false;
    private SubtitleEvent draggedEvent;
    private float dragOffset;

    private const float TIMELINE_HEIGHT = 150f;
    private const float WAVEFORM_HEIGHT = 60f;
    private const float EVENT_HEIGHT = 40f;

    public override void OnInspectorGUI() {
        SubtitleTrigger trigger = (SubtitleTrigger)target;

        DrawTriggerSettings(trigger);
        DrawAudioSettings(trigger);
        DrawSubtitleDefaults(trigger);
        DrawTracksSection(trigger);

        if (selectedTrack >= 0 && selectedTrack < trigger.tracks.Count) {
            DrawTimelineEditor(trigger, selectedTrack);
        }

        if (GUI.changed) {
            EditorUtility.SetDirty(trigger);
        }
    }

    private void DrawTriggerSettings(SubtitleTrigger trigger) {
        showTriggerSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showTriggerSettings, "Trigger Settings");
        if (showTriggerSettings) {
            trigger.mode = (TriggerMode)EditorGUILayout.EnumPopup("Trigger Mode", trigger.mode);

            if (trigger.mode == TriggerMode.Proximity || trigger.mode == TriggerMode.OnTriggerEnter) {
                trigger.hearingRadius = EditorGUILayout.FloatField("Hearing Radius", trigger.hearingRadius);
                trigger.playerTag = EditorGUILayout.TextField("Player Tag", trigger.playerTag);
                trigger.playerLayers = EditorGUILayout.MaskField("Player Layers", trigger.playerLayers, UnityEditorInternal.InternalEditorUtility.layers);
            }

            trigger.cooldownTime = EditorGUILayout.FloatField("Cooldown Time", trigger.cooldownTime);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawAudioSettings(SubtitleTrigger trigger) {
        showAudioSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showAudioSettings, "Audio Settings");
        if (showAudioSettings) {
            trigger.playRandomTrack = EditorGUILayout.Toggle("Play Random Track", trigger.playRandomTrack);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawSubtitleDefaults(SubtitleTrigger trigger) {
        showSubtitleDefaults = EditorGUILayout.BeginFoldoutHeaderGroup(showSubtitleDefaults, "Subtitle Defaults");
        if (showSubtitleDefaults) {
            trigger.defaultEffect = (SubtitleEffect)EditorGUILayout.EnumPopup("Default Effect", trigger.defaultEffect);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawTracksSection(SubtitleTrigger trigger) {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Audio Subtitle Tracks", EditorStyles.boldLabel);

        if (trigger.tracks == null) trigger.tracks = new List<AudioSubtitleTrack>();
        if (showTracks == null || showTracks.Length != trigger.tracks.Count) {
            showTracks = new bool[trigger.tracks.Count];
        }

        for (int i = 0; i < trigger.tracks.Count; i++) {
            DrawTrackItem(trigger, i);
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Track")) {
            trigger.tracks.Add(new AudioSubtitleTrack());
            System.Array.Resize(ref showTracks, trigger.tracks.Count);
        }
        if (GUILayout.Button("Clear All Tracks") && EditorUtility.DisplayDialog("Clear All", "Remove all tracks?", "Yes", "No")) {
            trigger.tracks.Clear();
            showTracks = new bool[0];
            selectedTrack = -1;
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawTrackItem(SubtitleTrigger trigger, int index) {
        var track = trigger.tracks[index];

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        showTracks[index] = EditorGUILayout.Foldout(showTracks[index], $"Track {index + 1}: {(track.audioClip ? track.audioClip.name : "No Audio")}", true);

        GUI.backgroundColor = selectedTrack == index ? Color.green : Color.white;
        if (GUILayout.Button("Edit Timeline", GUILayout.Width(100))) {
            selectedTrack = selectedTrack == index ? -1 : index;
        }
        GUI.backgroundColor = Color.white;

        if (GUILayout.Button("×", GUILayout.Width(20))) {
            trigger.tracks.RemoveAt(index);
            System.Array.Resize(ref showTracks, trigger.tracks.Count);
            if (selectedTrack == index) selectedTrack = -1;
            else if (selectedTrack > index) selectedTrack--;
            return;
        }
        EditorGUILayout.EndHorizontal();

        if (showTracks[index]) {
            EditorGUI.indentLevel++;

            var newClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", track.audioClip, typeof(AudioClip), false);
            if (newClip != track.audioClip) {
                track.audioClip = newClip;
                track.waveformGenerated = false;
            }

            track.loop = EditorGUILayout.Toggle("Loop", track.loop);

            EditorGUILayout.LabelField($"Subtitle Events: {track.events.Count}");

            if (track.audioClip && !track.waveformGenerated) {
                if (GUILayout.Button("Generate Waveform")) {
                    trigger.GenerateWaveformData(index);
                }
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }

    private void PlayPreviewAudio(AudioClip clip) {
        if (clip == null) return;

        StopPreviewAudio();

        var audioSource = EditorUtility.CreateGameObjectWithHideFlags("Audio Preview",
            HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();

        EditorApplication.update += UpdatePreviewAudio;
    }

    private void StopPreviewAudio() {
        var previewObject = GameObject.Find("Audio Preview");
        if (previewObject != null) {
            DestroyImmediate(previewObject);
        }
        EditorApplication.update -= UpdatePreviewAudio;
    }

    private void UpdatePreviewAudio() {
        var previewObject = GameObject.Find("Audio Preview");
        if (previewObject == null) {
            EditorApplication.update -= UpdatePreviewAudio;
            return;
        }

        var audioSource = previewObject.GetComponent<AudioSource>();
        if (!audioSource.isPlaying) {
            StopPreviewAudio();
        }
    }

    private void DrawTimelineEditor(SubtitleTrigger trigger, int trackIndex) {
        var track = trigger.tracks[trackIndex];
        if (track.audioClip == null) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Timeline Editor - {track.audioClip.name}", EditorStyles.boldLabel);

        DrawTimelineControls(trigger, track);

        Rect timelineRect = GUILayoutUtility.GetRect(0, TIMELINE_HEIGHT, GUILayout.ExpandWidth(true));
        DrawTimeline(timelineRect, track);

        HandleTimelineInput(timelineRect, track);

        DrawEventList(trigger, track);
    }

    private void DrawTimelineControls(SubtitleTrigger trigger, AudioSubtitleTrack track) {
        EditorGUILayout.BeginHorizontal();

        timelineZoom = EditorGUILayout.Slider("Zoom", timelineZoom, 0.1f, 5f);

        if (GUILayout.Button("Add Event", GUILayout.Width(80))) {
            track.events.Add(new SubtitleEvent {
                startTime = track.audioClip.length * 0.5f,
                text = "New Subtitle"
            });
        }

        if (GUILayout.Button("Play", GUILayout.Width(50))) {
            if (Application.isPlaying) {
                trigger.PlayTrack(selectedTrack);
            }
            else {
                PlayPreviewAudio(track.audioClip);
            }
        }

        if (GUILayout.Button("Stop", GUILayout.Width(50))) {
            if (Application.isPlaying) {
                trigger.StopCurrentSubtitles();
            }
            else {
                StopPreviewAudio();
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawTimeline(Rect rect, AudioSubtitleTrack track) {
        EditorGUI.DrawRect(rect, Color.black * 0.3f);

        float clipLength = track.audioClip.length;
        float pixelsPerSecond = (rect.width * timelineZoom) / clipLength;

        DrawWaveform(rect, track, pixelsPerSecond);
        DrawTimeMarkers(rect, clipLength, pixelsPerSecond);
        DrawSubtitleEvents(rect, track, pixelsPerSecond);
    }

    private void DrawWaveform(Rect rect, AudioSubtitleTrack track, float pixelsPerSecond) {
        if (!track.waveformGenerated || track.waveformData == null) return;

        Rect waveformRect = new Rect(rect.x, rect.y + rect.height - WAVEFORM_HEIGHT, rect.width, WAVEFORM_HEIGHT);

        Handles.color = Color.cyan * 0.7f;

        for (int i = 0; i < track.waveformData.Length - 1; i++) {
            float x1 = waveformRect.x + (i / (float)track.waveformData.Length) * waveformRect.width;
            float x2 = waveformRect.x + ((i + 1) / (float)track.waveformData.Length) * waveformRect.width;

            float y1 = waveformRect.y + waveformRect.height * 0.5f - track.waveformData[i] * waveformRect.height * 0.4f;
            float y2 = waveformRect.y + waveformRect.height * 0.5f - track.waveformData[i + 1] * waveformRect.height * 0.4f;

            Handles.DrawLine(new Vector3(x1, y1), new Vector3(x2, y2));
        }
    }

    private void DrawTimeMarkers(Rect rect, float clipLength, float pixelsPerSecond) {
        Handles.color = Color.white * 0.5f;

        float interval = clipLength > 30f ? 5f : (clipLength > 10f ? 1f : 0.5f);

        for (float time = 0; time <= clipLength; time += interval) {
            float x = rect.x + time * pixelsPerSecond;
            if (x > rect.xMax) break;

            Handles.DrawLine(new Vector3(x, rect.y), new Vector3(x, rect.yMax));

            Rect labelRect = new Rect(x - 20, rect.y, 40, 20);
            GUI.Label(labelRect, time.ToString("F1") + "s", EditorStyles.miniLabel);
        }
    }

    private void DrawSubtitleEvents(Rect rect, AudioSubtitleTrack track, float pixelsPerSecond) {
        foreach (var subtitleEvent in track.events) {
            float startX = rect.x + subtitleEvent.startTime * pixelsPerSecond;
            float endX = rect.x + (subtitleEvent.startTime + subtitleEvent.duration) * pixelsPerSecond;

            if (startX > rect.xMax || endX < rect.x) continue;

            Rect eventRect = new Rect(
                Mathf.Max(startX, rect.x),
                rect.y + TIMELINE_HEIGHT - WAVEFORM_HEIGHT - EVENT_HEIGHT,
                Mathf.Min(endX - startX, rect.xMax - startX),
                EVENT_HEIGHT
            );

            Color eventColor = Color.yellow;
            eventColor.a = 0.7f;
            EditorGUI.DrawRect(eventRect, eventColor);

            eventColor.a = 1f;
            Handles.color = eventColor;
            Handles.DrawLine(new Vector3(eventRect.x, eventRect.y), new Vector3(eventRect.x, eventRect.yMax));
            Handles.DrawLine(new Vector3(eventRect.xMax, eventRect.y), new Vector3(eventRect.xMax, eventRect.yMax));

            if (eventRect.width > 50) {
                GUI.Label(eventRect, subtitleEvent.text, EditorStyles.miniLabel);
            }
        }
    }

    private void HandleTimelineInput(Rect rect, AudioSubtitleTrack track) {
        Event e = Event.current;

        if (rect.Contains(e.mousePosition)) {
            if (e.type == EventType.MouseDown && e.button == 0) {
                float clickTime = ((e.mousePosition.x - rect.x) / rect.width) * track.audioClip.length / timelineZoom;

                foreach (var subtitleEvent in track.events) {
                    if (clickTime >= subtitleEvent.startTime && clickTime <= subtitleEvent.EndTime) {
                        isDraggingEvent = true;
                        draggedEvent = subtitleEvent;
                        dragOffset = clickTime - subtitleEvent.startTime;
                        e.Use();
                        break;
                    }
                }
            }
            else if (isDraggingEvent && e.type == EventType.MouseDrag) {
                float newTime = ((e.mousePosition.x - rect.x) / rect.width) * track.audioClip.length / timelineZoom - dragOffset;
                draggedEvent.startTime = Mathf.Clamp(newTime, 0, track.audioClip.length - draggedEvent.duration);
                e.Use();
                GUI.changed = true;
            }
            else if (e.type == EventType.MouseUp) {
                isDraggingEvent = false;
                draggedEvent = null;
            }
        }
    }

    private void DrawEventList(SubtitleTrigger trigger, AudioSubtitleTrack track) {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Subtitle Events", EditorStyles.boldLabel);

        for (int i = 0; i < track.events.Count; i++) {
            DrawEventItem(track.events[i], i, track);
        }
    }

    private void DrawEventItem(SubtitleEvent subtitleEvent, int index, AudioSubtitleTrack track) {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Event {index + 1}", EditorStyles.boldLabel, GUILayout.Width(70));
        if (GUILayout.Button("Remove", GUILayout.Width(60))) {
            track.events.RemoveAt(index);
            return;
        }
        EditorGUILayout.EndHorizontal();

        subtitleEvent.text = EditorGUILayout.TextField("Text", subtitleEvent.text);
        subtitleEvent.startTime = EditorGUILayout.FloatField("Start Time", subtitleEvent.startTime);
        subtitleEvent.duration = EditorGUILayout.FloatField("Duration", subtitleEvent.duration);

        subtitleEvent.overrideGlobalSettings = EditorGUILayout.Toggle("Override Global Settings", subtitleEvent.overrideGlobalSettings);

        if (subtitleEvent.overrideGlobalSettings) {
            EditorGUI.indentLevel++;
            subtitleEvent.effect = (SubtitleEffect)EditorGUILayout.EnumPopup("Effect", subtitleEvent.effect);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }
}