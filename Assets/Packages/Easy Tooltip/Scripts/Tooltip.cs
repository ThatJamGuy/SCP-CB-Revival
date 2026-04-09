namespace PixeLadder.SimpleTooltip
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    /// <summary>
    /// A simple "view" component that displays tooltip data. It is controlled by the TooltipManager.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class Tooltip : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The parent object for the title and icon. Can be hidden if both are absent.")]
        [SerializeField] private GameObject header;
        [Tooltip("The TextMeshPro component for the title text.")]
        [SerializeField] public TextMeshProUGUI titleField;
        [Tooltip("The TextMeshPro component for the main content text.")]
        [SerializeField] public TextMeshProUGUI contentField;
        [Tooltip("The Image component for the icon.")]
        [SerializeField] private Image iconField;

        /// <summary>
        /// Populates the UI elements with the provided content and styles. This method is null-safe.
        /// </summary>
        public void SetText(string content, string title = "", Sprite icon = null, Color? titleColor = null, Color? iconColor = null)
        {
            // Set Title (null-safe)
            bool hasTitle = !string.IsNullOrEmpty(title);
            if (titleField != null)
            {
                titleField.gameObject.SetActive(hasTitle);
                if (hasTitle)
                {
                    titleField.text = title;
                    titleField.color = titleColor ?? Color.white;
                }
            }

            // Set Content (null-safe)
            if (contentField != null)
            {
                contentField.text = content ?? string.Empty;
            }

            // Set Icon (null-safe)
            bool hasIcon = (icon != null);
            if (iconField != null)
            {
                iconField.gameObject.SetActive(hasIcon);
                if (hasIcon)
                {
                    iconField.sprite = icon;
                    iconField.color = iconColor ?? Color.white;
                }
            }

            // Conditionally hide the entire header area
            if (header != null)
            {
                header.SetActive(hasTitle || hasIcon);
            }
        }
    }
}