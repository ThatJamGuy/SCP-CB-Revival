#if UNITY_EDITOR && SORTIFY
using System;
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    [CustomEditor(typeof(SortifyChangeLog))]
    public class SortifyChangeLogEditor : Editor
    {
        private SerializedProperty _changelogEntries;

        private void OnEnable()
        {
            _changelogEntries = serializedObject.FindProperty("_changelogEntries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Sortify Changelog", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            for (int i = 0; i < _changelogEntries.arraySize; i++)
            {
                SerializedProperty entry = _changelogEntries.GetArrayElementAtIndex(i);
                SerializedProperty version = entry.FindPropertyRelative("version");
                SerializedProperty year = entry.FindPropertyRelative("year");
                SerializedProperty month = entry.FindPropertyRelative("month");
                SerializedProperty day = entry.FindPropertyRelative("day");
                SerializedProperty details = entry.FindPropertyRelative("details");

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Entry {i + 1}", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(version, new GUIContent("Version"));
                EditorGUILayout.PropertyField(details, new GUIContent("Details"));

                EditorGUILayout.LabelField("Release Date:", $"{day.intValue:D2}/{month.intValue:D2}/{year.intValue}", EditorStyles.label);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Update Date to Today"))
                {
                    var today = DateTime.Today;
                    year.intValue = today.Year;
                    month.intValue = today.Month;
                    day.intValue = today.Day;
                }
                if (GUILayout.Button("Remove Entry"))
                {
                    _changelogEntries.DeleteArrayElementAtIndex(i);
                    continue;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            if (GUILayout.Button("Add New Entry"))
            {
                _changelogEntries.InsertArrayElementAtIndex(_changelogEntries.arraySize);
                SerializedProperty newEntry = _changelogEntries.GetArrayElementAtIndex(_changelogEntries.arraySize - 1);

                newEntry.FindPropertyRelative("version").stringValue = "New Version";
                newEntry.FindPropertyRelative("details").stringValue = "Enter changelog details here.";

                var today = DateTime.Today;
                newEntry.FindPropertyRelative("year").intValue = today.Year;
                newEntry.FindPropertyRelative("month").intValue = today.Month;
                newEntry.FindPropertyRelative("day").intValue = today.Day;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif