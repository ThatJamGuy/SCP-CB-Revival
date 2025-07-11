#if SORTIFY_ATTRIBUTES
using UnityEngine;

namespace Sortify
{
    public class UnitAttribute : PropertyAttribute
    {
        public string Unit;

        /// <summary>
        /// Initializes a new instance of the UnitAttribute, allowing a unit label to be displayed next to the property in the Inspector.
        /// </summary>
        /// <param name="unit">The unit label to display next to the property value (e.g., "kg", "m", "%").</param>
        public UnitAttribute(string unit)
        {
            Unit = unit;
        }
    }
}
#endif
