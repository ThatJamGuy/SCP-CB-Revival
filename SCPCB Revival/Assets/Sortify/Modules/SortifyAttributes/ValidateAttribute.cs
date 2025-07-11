#if SORTIFY_ATTRIBUTES
using UnityEngine;

namespace Sortify
{
    public class ValidateAttribute : PropertyAttribute
    {
        public string ValidationMethod;
        public string Message;


        /// <summary>
        /// Initializes a new instance of the ValidateAttribute, specifying a method for validation and a message to display if validation fails.
        /// </summary>
        /// <param name="validationMethod">The name of the method used to validate the property. This method must return a bool and take no parameters.</param>
        /// <param name="message">The message displayed in the Inspector if validation fails.</param>
        public ValidateAttribute(string validationMethod, string message)
        {
            ValidationMethod = validationMethod;
            Message = message;
        }
    }
}
#endif
