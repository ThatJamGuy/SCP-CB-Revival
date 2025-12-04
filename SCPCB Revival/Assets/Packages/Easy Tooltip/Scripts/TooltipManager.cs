namespace PixeLadder.SimpleTooltip
{
    using System.Collections;
    using System.Text;
    using TMPro;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.UI;

    /// <summary>
    /// A singleton manager that controls the lifecycle of a single tooltip instance.
    /// It handles showing, hiding, positioning, and resizing logic.
    /// </summary>
    public class TooltipManager : MonoBehaviour
    {
        #region Static Instance
        public static TooltipManager Instance { get; private set; }
        #endregion

        #region Fields
        [Header("Core Configuration")]
        [Tooltip("The UI Prefab for the tooltip itself.")]
        [SerializeField] private Tooltip tooltipPrefab;

        [Header("Layout Settings")]
        [Tooltip("The maximum width the tooltip can have before its text starts wrapping.")]
        [SerializeField, Min(50f)] private float maxTooltipWidth = 350f;

        [Header("Animation Settings")]
        [Tooltip("The duration of the fade-in and fade-out animations in seconds.")]
        [SerializeField, Min(0f)] private float fadeDuration = 0.2f;

        [Header("Positioning")]
        [Tooltip("An offset to apply to the tooltip's position relative to the mouse cursor.")]
        [SerializeField] private Vector2 positionOffset = new(0, -20);

        // --- Private State ---
        private Tooltip tooltipInstance;
        private RectTransform tooltipRect;
        private CanvasGroup canvasGroup;
        private Coroutine activeCoroutine;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else { Destroy(gameObject); }
        }

        private void Start()
        {
            // Failsafe to ensure a Canvas exists.
            Canvas rootCanvas = FindFirstObjectByType<Canvas>();
            if (!rootCanvas)
            {
                Debug.LogError("TooltipManager Error: No Canvas found in scene. Please add a Canvas.");
                return;
            }

            // Instantiate and cache components for efficiency.
            GameObject tooltipObj = Instantiate(tooltipPrefab.gameObject, rootCanvas.transform, false);
            tooltipInstance = tooltipObj.GetComponent<Tooltip>();
            tooltipRect = tooltipObj.GetComponent<RectTransform>();
            canvasGroup = tooltipObj.GetComponent<CanvasGroup>();

            tooltipObj.SetActive(false);
        }

        private void Update()
        {
            // Make tooltip follow mouse with offset while active
            if (tooltipInstance != null && tooltipInstance.gameObject.activeInHierarchy)
            {
                PositionTooltip();
            }
        }
        #endregion

        #region Public API
        public void ShowTooltip(string content, string title, Sprite icon, Color titleColor, Color iconColor, float delay)
        {
            if (!tooltipInstance) return;

            // "Last Command Wins": Stop any previous coroutine.
            if (activeCoroutine != null) StopCoroutine(activeCoroutine);
            activeCoroutine = StartCoroutine(ShowRoutine(content, title, icon, titleColor, iconColor, delay));
        }

        public void HideTooltip()
        {
            if (!tooltipInstance) return;

            if (activeCoroutine != null) StopCoroutine(activeCoroutine);
            if (tooltipInstance.gameObject.activeInHierarchy)
                activeCoroutine = StartCoroutine(FadeOut());
        }
        #endregion

        #region Coroutines & Logic
        private IEnumerator ShowRoutine(string content, string title, Sprite icon, Color titleColor, Color iconColor, float delay) {
            // Set alpha to 0 before waiting to prevent a one-frame flicker of old content.
            canvasGroup.alpha = 0;
            yield return new WaitForSecondsRealtime(delay);

            yield return ResizeTooltipRoutine(content, title, icon, titleColor, iconColor);

            tooltipInstance.gameObject.SetActive(true);
            tooltipInstance.transform.SetAsLastSibling();
            PositionTooltip();

            activeCoroutine = StartCoroutine(FadeIn());
        }

        private IEnumerator ResizeTooltipRoutine(string content, string title, Sprite icon, Color titleColor, Color iconColor)
        {
            tooltipInstance.gameObject.SetActive(false);

            float availableTitleWidth = CalculateAvailableWidthForText(tooltipInstance.titleField);
            float availableContentWidth = CalculateAvailableWidthForText(tooltipInstance.contentField);

            string wrappedTitle = WrapText(title, tooltipInstance.titleField, availableTitleWidth);
            string wrappedContent = WrapText(content, tooltipInstance.contentField, availableContentWidth);

            tooltipInstance.SetText(wrappedContent, wrappedTitle, icon, titleColor, iconColor);

            // The robust "triple cycle" resize method.
            for (int i = 0; i < 3; i++)
            {
                tooltipInstance.gameObject.SetActive(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
                yield return new WaitForEndOfFrame();
                tooltipInstance.gameObject.SetActive(false);
            }
        }

        private IEnumerator FadeIn()
        {
            float start = Time.unscaledTime;
            while (Time.unscaledTime < start + fadeDuration)
            {
                if (canvasGroup == null) yield break;
                canvasGroup.alpha = Mathf.Lerp(0, 1, (Time.unscaledTime - start) / fadeDuration);
                yield return null;
            }
            if (canvasGroup != null) canvasGroup.alpha = 1;
        }

        private IEnumerator FadeOut()
        {
            float start = Time.unscaledTime;
            float startAlpha = canvasGroup.alpha;
            while (Time.unscaledTime < start + fadeDuration)
            {
                if (canvasGroup == null) yield break;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, (Time.unscaledTime - start) / fadeDuration);
                yield return null;
            }
            if (canvasGroup != null) canvasGroup.alpha = 0;
            if (tooltipInstance != null) tooltipInstance.gameObject.SetActive(false);
        }
        #endregion

        #region Helper Methods
        private void PositionTooltip()
        {
            float outlineSize = 0;
            Outline outline = tooltipInstance.GetComponentInChildren<Outline>();
            if (outline != null)
            {
                outlineSize = outline.effectDistance.x;
            }

            Vector3 mousePos = Mouse.current.position.ReadValue();
            float scale = tooltipRect.lossyScale.x;

            float totalWidth = (tooltipRect.rect.width + outlineSize) * scale;
            float totalHeight = (tooltipRect.rect.height + outlineSize) * scale;

            float x = Mathf.Clamp(mousePos.x, 0, Screen.width - totalWidth);
            float y = Mathf.Clamp(mousePos.y, totalHeight, Screen.height);

            tooltipRect.position = new Vector3(x + positionOffset.x, y + positionOffset.y, 0);
        }

        private float CalculateAvailableWidthForText(TMP_Text textElement)
        {
            float availableWidth = maxTooltipWidth;
            if (textElement == null) return availableWidth;

            Transform current = textElement.transform;
            while (current != null && current != tooltipInstance.transform)
            {
                if (current.TryGetComponent<LayoutGroup>(out var layoutGroup))
                {
                    availableWidth -= (layoutGroup.padding.left + layoutGroup.padding.right);
                    if (layoutGroup is HorizontalLayoutGroup hlg)
                    {
                        availableWidth -= hlg.spacing * (current.parent.childCount - 1);
                    }
                }
                current = current.parent;
            }
            return availableWidth;
        }

        private string WrapText(string text, TMP_Text tmp, float maxWidth)
        {
            if (string.IsNullOrEmpty(text) || tmp == null) return text;
            if (tmp.GetPreferredValues(text).x <= maxWidth) return text;

            StringBuilder sb = new StringBuilder();
            string[] words = text.Split(' ');
            string line = "";

            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                string testLine = string.IsNullOrEmpty(line) ? word : $"{line} {word}";
                if (tmp.GetPreferredValues(testLine).x > maxWidth && !string.IsNullOrEmpty(line))
                {
                    sb.AppendLine(line);
                    line = word;
                }
                else
                {
                    line = testLine;
                }
            }
            sb.Append(line);
            return sb.ToString();
        }
        #endregion
    }
}