#if UNITY_EDITOR
using System.Collections.Generic;

namespace Verpha.HierarchyDesigner
{
    [System.Serializable]
    internal class HierarchyDesigner_Shared_Serializable<T>
    {
        public List<T> items;

        public HierarchyDesigner_Shared_Serializable(List<T> items)
        {
            this.items = items;
        }
    }
}
#endif