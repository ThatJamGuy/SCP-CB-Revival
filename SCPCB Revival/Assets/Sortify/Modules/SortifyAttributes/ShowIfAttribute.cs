#if SORTIFY_ATTRIBUTES
using System;
using UnityEngine;

namespace Sortify
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionName;
        public object CompareValue;
        public bool Inverted;

        /// <summary>
        /// Initializes a new instance of the ShowIfAttribute, specifying a condition for displaying the field in the Inspector.
        /// </summary>
        /// <param name="conditionName">The name of the condition field or property that controls visibility.</param>
        /// <param name="compareValue">Optional value to compare against the condition field's value.</param>
        /// <param name="inverted">If true, inverts the condition, showing the field when the condition is not met. Default is false.</param>
        public ShowIfAttribute(string conditionName, object compareValue = null, bool inverted = false)
        {
            ConditionName = conditionName;
            CompareValue = compareValue;
            Inverted = inverted;
        }
    }
}
#endif
