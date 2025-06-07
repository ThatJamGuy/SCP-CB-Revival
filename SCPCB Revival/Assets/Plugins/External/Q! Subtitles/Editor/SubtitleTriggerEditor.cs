using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SubtitleTrigger))]
public class SubtitleTriggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SubtitleTrigger trigger = (SubtitleTrigger)target;

        trigger.player = (Transform)EditorGUILayout.ObjectField("Player", trigger.player, typeof(Transform), true);
        trigger.hearingRadius = EditorGUILayout.FloatField("Hearing Radius", trigger.hearingRadius);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Audio Subtitle Pairs", EditorStyles.boldLabel);

        if (trigger.audioSubtitlePairs == null)
            trigger.audioSubtitlePairs = new List<AudioSubtitlePair>();

        for (int i = 0; i < trigger.audioSubtitlePairs.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Audio Clip {i + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("Remove Pair", GUILayout.Width(100)))
            {
                trigger.audioSubtitlePairs.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            var pair = trigger.audioSubtitlePairs[i];
            pair.audioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", pair.audioClip, typeof(AudioClip), false);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Subtitles", EditorStyles.boldLabel);

            for (int j = 0; j < pair.subtitles.Count; j++)
            {
                EditorGUILayout.BeginVertical("box");
                pair.subtitles[j].text = EditorGUILayout.TextField("Text", pair.subtitles[j].text);
                pair.subtitles[j].startTime = EditorGUILayout.FloatField("Start Time", pair.subtitles[j].startTime);
                pair.subtitles[j].duration = EditorGUILayout.FloatField("Duration", pair.subtitles[j].duration);

                if (GUILayout.Button("Remove Subtitle"))
                {
                    pair.subtitles.RemoveAt(j);
                    break;
                }
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Subtitle"))
            {
                if (pair.subtitles == null)
                    pair.subtitles = new List<SubtitleTiming>();
                pair.subtitles.Add(new SubtitleTiming());
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add Audio Subtitle Pair"))
        {
            trigger.audioSubtitlePairs.Add(new AudioSubtitlePair());
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(trigger);
        }
    }
}