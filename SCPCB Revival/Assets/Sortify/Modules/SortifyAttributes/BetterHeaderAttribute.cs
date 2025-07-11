#if SORTIFY_ATTRIBUTES
using UnityEngine;

namespace Sortify
{
    public class BetterHeaderAttribute : PropertyAttribute
    {
        public string HeaderText;
        public int FontSize;
        public TextAnchor Alignment;

        /// <summary>
        /// Initializes a new instance of the BetterHeaderAttribute with specified header text, font size, and alignment.
        /// </summary>
        /// <param name="headerText">The header text displayed above the property in the Inspector.</param>
        /// <param name="fontSize">The font size of the header text. Default is 14.</param>
        /// <param name="alignment">The alignment of the header text. Default is TextAnchor.MiddleLeft.</param>
        public BetterHeaderAttribute(string headerText, int fontSize = 14, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            HeaderText = headerText;
            FontSize = fontSize;
            Alignment = alignment;
        }
    }
}
#endif
