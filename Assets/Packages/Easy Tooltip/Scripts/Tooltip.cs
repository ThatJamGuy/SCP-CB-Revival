namespace PixeLadder.EasyTooltip
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using System.Collections.Generic;

    /// <summary>
    /// Attached to the actual visual Tooltip Prefab. 
    /// Handles the dynamic assignment of text, icons, colors, and in-place pooling of secondary content blocks.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("PixeLadder/Easy Tooltip/Tooltip View")]
    public class Tooltip : MonoBehaviour
    {
        [Header("Primary Content References")]
        [Tooltip("Parent object for title and icon. Used to toggle the entire header block if no title/icon exists.")]
        [SerializeField] private GameObject headerContainer;

        [Tooltip("Text component used to display the title.")]
        [SerializeField] public TextMeshProUGUI titleField;

        [Tooltip("Image component used to display the optional header icon.")]
        [SerializeField] public Image iconField;

        [Tooltip("The parent container for all body content (main text, secondary texts, and separators).")]
        [SerializeField] private Transform bodyContainer;

        [Tooltip("Text component used to display the main tooltip content.")]
        [SerializeField] public TextMeshProUGUI mainContentField;

        [Header("Dynamic Content Prefabs")]
        [Tooltip("Prefab used to instantiate secondary text blocks when dealing with multi-section tooltips.")]
        [SerializeField] private TextMeshProUGUI textBlockPrefab;

        [Tooltip("Prefab used to instantiate separators between text blocks.")]
        [SerializeField] private Image separatorPrefab;

        [Header("Style References")]
        [Tooltip("The main background image of the entire tooltip panel.")]
        [SerializeField] private Image panelBackground;

        [Tooltip("The background image specific to the Header container.")]
        [SerializeField] private Image headerBackground;

        [Tooltip("The background image specific to the Body container.")]
        [SerializeField] private Image bodyBackground;

        [Tooltip("The separate sliced image used for the border/outline. (Hidden if outline is disabled).")]
        [SerializeField] private Image outlineImage;

        // In-Place Object Pools to prevent runtime Garbage Collection
        private readonly List<TextMeshProUGUI> textPool = new List<TextMeshProUGUI>();
        private readonly List<Image> separatorPool = new List<Image>();

        /// <summary>
        /// Populates the UI fields with primary content, and dynamically handles in-place pooling 
        /// for any additional secondary text blocks and separators.
        /// </summary>
        /// <param name="contentBlocks">List of strings to populate the body.</param>
        /// <param name="title">Optional header string.</param>
        /// <param name="icon">Optional header icon.</param>
        /// <param name="titleColor">Color for the title text.</param>
        /// <param name="iconColor">Tint for the icon.</param>
        /// <param name="separatorSprite">Sprite used for injected dividers.</param>
        /// <param name="separatorColor">Tint for the injected dividers.</param>
        public void SetContent(List<string> contentBlocks, string title = "", Sprite icon = null,
            Color? titleColor = null, Color? iconColor = null,
            Sprite separatorSprite = null, Color? separatorColor = null, float separatorHeight = 1f)
        {
            // --- 1. Process Header (Title & Icon) ---
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

            if (headerContainer != null)
            {
                headerContainer.SetActive(hasTitle || hasIcon);
            }

            // --- 2. Process Body Content (In-Place Pooling) ---
            bool hasBodyContent = false;
            if (contentBlocks != null && contentBlocks.Count > 0)
            {
                for (int i = 0; i < contentBlocks.Count; i++)
                {
                    if (!string.IsNullOrEmpty(contentBlocks[i]))
                    {
                        hasBodyContent = true;
                        break;
                    }
                }
            }

            if (bodyContainer != null)
            {
                bodyContainer.gameObject.SetActive(hasBodyContent);
            }

            if (!hasBodyContent)
            {
                // Fallback if list is totally empty or contains only empty strings
                if (mainContentField != null) mainContentField.gameObject.SetActive(false);
                DisableUnusedPoolItems(0, 0);
                return;
            }

            // A. Set the first block to the main static text field
            if (mainContentField != null)
            {
                mainContentField.gameObject.SetActive(true);
                mainContentField.text = contentBlocks[0];
            }

            // B. Determine how many extra texts and separators we need
            int extraBlocksNeeded = contentBlocks.Count - 1;
            int separatorsNeeded = extraBlocksNeeded > 0 ? extraBlocksNeeded : 0;

            // Expand the pools if necessary
            EnsurePoolCapacity(extraBlocksNeeded, separatorsNeeded);

            // C. Enable and configure required items
            for (int i = 0; i < extraBlocksNeeded; i++)
            {
                // Enable Separator
                Image sep = separatorPool[i];
                sep.gameObject.SetActive(true);
                sep.transform.SetAsLastSibling(); // Ensure correct order in VerticalLayoutGroup
                if (separatorSprite != null) sep.sprite = separatorSprite;
                sep.color = separatorColor ?? Color.white;

                // Force Layout System to respect the separator height
                LayoutElement sepLayout = sep.GetComponent<LayoutElement>();
                if (sepLayout == null) sepLayout = sep.gameObject.AddComponent<LayoutElement>();
                sepLayout.minHeight = separatorHeight;
                sepLayout.preferredHeight = separatorHeight;
                sep.rectTransform.sizeDelta = new Vector2(sep.rectTransform.sizeDelta.x, separatorHeight);

                // Enable Text Block
                TextMeshProUGUI txt = textPool[i];
                txt.gameObject.SetActive(true);
                txt.transform.SetAsLastSibling(); // Ensure correct order below separator
                txt.text = contentBlocks[i + 1];  // Offset by 1 since index 0 is mainContentField
            }

            // D. Safely disable any pooled items we didn't use this time
            DisableUnusedPoolItems(extraBlocksNeeded, separatorsNeeded);
        }

        /// <summary>
        /// Applies the requested colors, sprites, and toggles to the background layers and outline.
        /// </summary>
        /// <param name="panelColor">Main background color.</param>
        /// <param name="headerColor">Header section background tint.</param>
        /// <param name="bodyColor">Body section background tint.</param>
        /// <param name="headerSprite">Optional sliced sprite for the header background.</param>
        /// <param name="bodySprite">Optional sliced sprite for the body background.</param>
        /// <param name="outlineColor">Tint applied to the outline border.</param>
        /// <param name="showOutline">Whether the outline image should be active.</param>
        public void SetStyle(
            Color panelColor, Color headerColor, Color bodyColor,
            Sprite headerSprite, Sprite bodySprite,
            Color outlineColor, bool showOutline)
        {
            // Apply Panel Background
            if (panelBackground != null)
            {
                panelBackground.color = panelColor;
            }

            // Apply Header Background
            if (headerBackground != null)
            {
                bool useHeaderBg = headerSprite != null || headerColor.a > 0;
                headerBackground.gameObject.SetActive(useHeaderBg);
                if (useHeaderBg)
                {
                    headerBackground.sprite = headerSprite;
                    headerBackground.color = headerColor;
                }
            }

            // Apply Body Background
            if (bodyBackground != null)
            {
                bool useBodyBg = bodySprite != null || bodyColor.a > 0;
                bodyBackground.gameObject.SetActive(useBodyBg);
                if (useBodyBg)
                {
                    bodyBackground.sprite = bodySprite;
                    bodyBackground.color = bodyColor;
                }
            }

            // Apply Outline
            if (outlineImage != null)
            {
                outlineImage.gameObject.SetActive(showOutline);
                if (showOutline)
                {
                    outlineImage.color = outlineColor;
                }
            }
        }

        /// <summary>
        /// Ensures the in-place object pools have enough instantiated elements to satisfy the current request.
        /// </summary>
        private void EnsurePoolCapacity(int requiredTexts, int requiredSeparators)
        {
            if (bodyContainer == null) return;

            // Expand Text Pool
            while (textPool.Count < requiredTexts)
            {
                if (textBlockPrefab == null) break;
                TextMeshProUGUI newText = Instantiate(textBlockPrefab, bodyContainer);
                newText.gameObject.SetActive(false);
                textPool.Add(newText);
            }

            // Expand Separator Pool
            while (separatorPool.Count < requiredSeparators)
            {
                if (separatorPrefab == null) break;
                Image newSep = Instantiate(separatorPrefab, bodyContainer);
                newSep.gameObject.SetActive(false);
                separatorPool.Add(newSep);
            }
        }

        /// <summary>
        /// Disables any instantiated texts or separators that exceed the currently required amount.
        /// </summary>
        private void DisableUnusedPoolItems(int activeTexts, int activeSeparators)
        {
            for (int i = activeTexts; i < textPool.Count; i++)
            {
                textPool[i].gameObject.SetActive(false);
            }

            for (int i = activeSeparators; i < separatorPool.Count; i++)
            {
                separatorPool[i].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Retrieves all active text components (Main + Pooled) for width calculation and wrapping logic.
        /// </summary>
        public List<TextMeshProUGUI> GetActiveTextElements()
        {
            List<TextMeshProUGUI> activeTexts = new List<TextMeshProUGUI>();
            if (mainContentField != null && mainContentField.gameObject.activeInHierarchy)
            {
                activeTexts.Add(mainContentField);
            }

            foreach (var txt in textPool)
            {
                if (txt.gameObject.activeInHierarchy) activeTexts.Add(txt);
            }
            return activeTexts;
        }
    }
}