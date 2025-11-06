using UnityEditor;
using UnityEngine;

namespace Unity.PlasticSCM.Editor
{
    internal static class FindUnityObject
    {
        internal static Object ForInstanceID(int instanceID)
        {
#if UNITY_6000_3_OR_NEWER
            return EditorUtility.EntityIdToObject(instanceID);
#else
            return EditorUtility.InstanceIDToObject(instanceID);
#endif
        }
    }
}
