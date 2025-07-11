#if SORTIFY_ATTRIBUTES
using System;
using UnityEngine;

namespace Sortify
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false)]
    public class ButtonAttribute : PropertyAttribute
    {
        public string ButtonText;

        /// <summary>
        /// Initializes a new instance of the ButtonAttribute with specified button text.
        /// </summary>
        /// <param name="buttonText">The text displayed on the button in the Inspector.</param>

        public ButtonAttribute(string buttonText = "")
        {
            ButtonText = buttonText;
        }
    }
}
#endif
