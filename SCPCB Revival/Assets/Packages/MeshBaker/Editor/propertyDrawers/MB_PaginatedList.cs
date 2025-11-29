using UnityEditor;
using UnityEngine;

namespace DigitalOpus.MB.MBEditor
{
    public class MB_PaginatedList
    {
        public static void GenerateObjectsToBeCombinedList(SerializedProperty objsToMesh, ref int currentPage, ref int objectsPerPage)
        {
            SerializedProperty arraySizeProp = objsToMesh.FindPropertyRelative("Array.size");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Size", GUILayout.Width(EditorGUIUtility.labelWidth));
            int newSize = EditorGUILayout.DelayedIntField(arraySizeProp.intValue);
            arraySizeProp.intValue = Mathf.Max(1, newSize);
            if (newSize <= 0 &&
                arraySizeProp.intValue == 1)
            {
                // This is necessary because the first element in the list gets preserved.
                // If the user sets the number of elements to 0 or less then we should 
                // make the displayed first element == null
                SerializedProperty element = objsToMesh.GetArrayElementAtIndex(0);
                element.objectReferenceValue = null;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Objects per Page", GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUI.BeginChangeCheck();
            int newObjectsPerPage = EditorGUILayout.DelayedIntField(objectsPerPage);
            if (EditorGUI.EndChangeCheck())
            {
                objectsPerPage = Mathf.Clamp(newObjectsPerPage, 10, 500);
                currentPage = 1;
            }
            EditorGUILayout.EndHorizontal();

            int totalObjects = objsToMesh.arraySize;
            int totalPages = totalObjects / objectsPerPage;
            if (totalObjects % objectsPerPage != 0)
            {
                totalPages++;
            }

            if (totalPages == 0)
            {
                currentPage = 1;
            }
            else if (currentPage > totalPages)
            {
                currentPage = totalPages;
            }
            else if (currentPage < 1)
            {
                currentPage = 1;
            }

            int startIndex = (currentPage - 1) * objectsPerPage;
            int endIndex = Mathf.Min(startIndex + objectsPerPage, totalObjects);
            for (int i = startIndex; i < endIndex; i++)
            {
                SerializedProperty objProp = objsToMesh.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(objProp, new GUIContent("Object " + (i + 1)));
            }

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = currentPage > 1;
            if (GUILayout.Button("Previous"))
            {
                currentPage--;
            }
            bool prevGUIEnabled = GUI.enabled;
            GUI.enabled = true;
            GUIStyle centeredLabelStyle = new GUIStyle(EditorStyles.label);
            centeredLabelStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("Page " + currentPage + " of " + totalPages, centeredLabelStyle);
            GUI.enabled = prevGUIEnabled;
            GUI.enabled = currentPage < totalPages;
            if (GUILayout.Button("Next"))
            {
                currentPage++;
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
    }
}
