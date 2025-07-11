#if SORTIFY_ATTRIBUTES
using UnityEngine;

namespace Sortify
{
    public class MessageAttribute : PropertyAttribute
    {
        public enum MessageType
        {
            Info,
            Warning,
            Error,
        }

        public MessageType Type;
        public string Message;

        /// <summary>
        /// Initializes a new instance of the MessageAttribute with a specified message type and text.
        /// </summary>
        /// <param name="type">The type of the message to display (Info, Warning, Error).</param>
        /// <param name="message">The text of the message to display in the Inspector.</param>
        public MessageAttribute(MessageType type, string message)
        {
            Type = type;
            Message = message;
        }
    }
}
#endif
