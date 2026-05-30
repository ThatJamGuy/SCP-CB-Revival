namespace PixeLadder.EasyTooltip
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("PixeLadder/Easy Tooltip/Tooltip View")]
    public class Tooltip : MonoBehaviour
    {
        [Header("Content References")]
        [Tooltip("Parent object for title/icon.")]
        [SerializeField] private GameObject header;
        [SerializeField] public TextMeshProUGUI titleField;
        [SerializeField] public TextMeshProUGUI contentField;
        [SerializeField] private Image iconField;

        [Header("Style References")]
        [Tooltip("The main background image.")]
        [SerializeField] private Image backgroundImage;

        [Tooltip("The separate sliced image used for the border/outline.")]
        [SerializeField] private Image outlineImage;

        public void SetContent(string content, string title = "", Sprite icon = null, Color? titleColor = null, Color? iconColor = null)
        {
            // 1. Title
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

            // 2. Content
            if (contentField != null)
            {
                contentField.text = content ?? string.Empty;
            }

            // 3. Icon
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

            // 4. Header Container
            if (header != null) header.SetActive(hasTitle || hasIcon);
        }

        public void SetStyle(Color backgroundColor, Color outlineColor, bool showOutline)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }

            if (outlineImage != null)
            {
                // Toggle the GameObject to ensure efficient rendering
                outlineImage.gameObject.SetActive(showOutline);

                if (showOutline)
                {
                    outlineImage.color = outlineColor;
                }
            }
        }
    }
}