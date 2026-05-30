using System;

namespace PixeLadder.EasyTooltip
{
    using System.Collections;
    using System.Text;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
#endif

    [AddComponentMenu("PixeLadder/Easy Tooltip/Tooltip Manager")]
    public class TooltipManager : MonoBehaviour
    {
        #region Static Instance
        public static TooltipManager Instance { get; private set; }
        #endregion

        #region Fields
        [Header("Core Configuration")]
        [SerializeField] private Tooltip tooltipPrefab;

        [Header("Global Size")]
        [SerializeField, Min(50f)] private float defaultMaxWidth = 350f;

        [Header("Global Animation")]
        [SerializeField, Min(0f)] private float fadeDuration = 0.2f;

        [Header("Global Positioning")]
        public Vector2 defaultMouseOffset = new(0, -20);
        public float defaultFixedGap = 5f;
        [SerializeField] private bool smartFlipping = true;

        [Header("Global Style Defaults")]
        public Color defaultTitleColor = Color.white;
        public Color defaultIconColor = Color.white;
        public float defaultHoverDelay = 0.5f;

        [Space(5)]
        public Color defaultBackgroundColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        public bool defaultShowOutline = true;
        public Color defaultOutlineColor = Color.white;

        // --- Public Accessors ---
        public float DefaultMaxWidth => defaultMaxWidth;

        // --- Private State ---
        private Tooltip tooltipInstance;
        private RectTransform tooltipRect;
        private CanvasGroup canvasGroup;

        private Coroutine activeShowCoroutine;
        private Coroutine activeHideCoroutine;

        // Current Request Context
        private TooltipTrigger currentTrigger; // Changed from Transform to TooltipTrigger
        private TooltipPositionMode currentMode;
        private TooltipAnchor currentAnchor;
        private Vector2 currentOffset;
        private float? currentWidth;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else { Destroy(gameObject); }
        }

        private void Update() {
            // Looks like there is a bug where follow mouse doesn't work, so this is the temporary fix I made lmao
            if (!tooltipInstance || !tooltipInstance.gameObject.activeInHierarchy) return;
            UpdatePosition();
        }

        #endregion

        #region Public API
        // Changed triggerContext (Transform) to trigger (TooltipTrigger)
        public void ShowTooltip(string content, string title, Sprite icon,
            Color titleColor, Color iconColor, Color bgColor, Color outlineColor, bool showOutline,
            float delay, TooltipTrigger trigger,
            TooltipPositionMode mode, TooltipAnchor anchor, Vector2 offset, float? widthOverride)
        {
            if (activeShowCoroutine != null) StopCoroutine(activeShowCoroutine);

            currentTrigger = trigger;
            currentMode = mode;
            currentAnchor = anchor;
            currentOffset = offset;
            currentWidth = widthOverride;

            activeShowCoroutine = StartCoroutine(ShowRoutine(content, title, icon, titleColor, iconColor, bgColor, outlineColor, showOutline, delay));
        }

        public void HideTooltip()
        {
            if (tooltipInstance == null) return;

            // Stop show coroutine so the event doesn't fire if we hid before the delay ended
            if (activeShowCoroutine != null) StopCoroutine(activeShowCoroutine);

            if (activeHideCoroutine != null) StopCoroutine(activeHideCoroutine);

            // Only hide (and fire event) if it's actually active/showing
            if (tooltipInstance.gameObject.activeInHierarchy || (canvasGroup != null && canvasGroup.alpha > 0))
            {
                // Fire the Hide event
                if (currentTrigger != null) currentTrigger.onTooltipHide?.Invoke();

                activeHideCoroutine = StartCoroutine(FadeOut());
            }
        }
        #endregion

        #region Coroutines
        private IEnumerator ShowRoutine(string content, string title, Sprite icon,
            Color titleColor, Color iconColor, Color bgColor, Color outlineColor, bool showOutline,
            float delay)
        {
            // 1. Wait for the Delay
            if (delay > 0) yield return new WaitForSecondsRealtime(delay);

            if (activeHideCoroutine != null) StopCoroutine(activeHideCoroutine);

            // 2. Ensure resources exist
            // We use currentTrigger.transform for context
            if (currentTrigger == null || !EnsureTooltipReady(currentTrigger.transform)) yield break;

            if (canvasGroup != null) canvasGroup.alpha = 0;

            // 3. Prepare Visuals
            yield return ResizeTooltipRoutine(content, title, icon, titleColor, iconColor, bgColor, outlineColor, showOutline);

            tooltipInstance.gameObject.SetActive(true);
            tooltipInstance.transform.SetAsLastSibling();
            UpdatePosition();

            // 4. Fire the Show Event (Syncs with visibility)
            currentTrigger.onTooltipShow?.Invoke();

            activeShowCoroutine = StartCoroutine(FadeIn());
        }

        private IEnumerator ResizeTooltipRoutine(string content, string title, Sprite icon,
            Color titleColor, Color iconColor, Color bgColor, Color outlineColor, bool showOutline)
        {
            tooltipInstance.gameObject.SetActive(false);

            float targetMax = currentWidth ?? defaultMaxWidth;

            float availableTitleWidth = CalculateAvailableWidthForText(tooltipInstance.titleField, targetMax);
            float availableContentWidth = CalculateAvailableWidthForText(tooltipInstance.contentField, targetMax);

            string wrappedTitle = WrapText(title, tooltipInstance.titleField, availableTitleWidth);
            string wrappedContent = WrapText(content, tooltipInstance.contentField, availableContentWidth);

            tooltipInstance.SetContent(wrappedContent, wrappedTitle, icon, titleColor, iconColor);
            tooltipInstance.SetStyle(bgColor, outlineColor, showOutline);

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

        #region Positioning Logic
        private void UpdatePosition()
        {
            if (tooltipInstance == null) return;

            if (currentMode == TooltipPositionMode.FollowMouse)
            {
                tooltipRect.pivot = new Vector2(0, 1);
                PositionAtMouse();
                ClampToScreen();
            }
            else
            {
                tooltipRect.pivot = new Vector2(0.5f, 0.5f);
                Vector3 preferredPos = CalculateFixedPosition(currentAnchor);
                tooltipInstance.transform.localPosition = preferredPos;

                if (smartFlipping && IsOutOfBounds(tooltipRect))
                {
                    TooltipAnchor flippedAnchor = GetOppositeAnchor(currentAnchor);
                    Vector3 flippedPos = CalculateFixedPosition(flippedAnchor);

                    tooltipInstance.transform.localPosition = flippedPos;
                    if (IsOutOfBounds(tooltipRect))
                    {
                        tooltipInstance.transform.localPosition = preferredPos;
                    }
                }
                ClampToScreen();
            }
        }

        private void PositionAtMouse()
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 screenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
            Vector2 screenPos = Input.mousePosition;
#endif
            screenPos += defaultMouseOffset + currentOffset;
            ScreenToLocal(screenPos, out Vector2 localPoint);
            tooltipInstance.transform.localPosition = new Vector3(localPoint.x, localPoint.y, 0);
        }

        private Vector3 CalculateFixedPosition(TooltipAnchor anchor)
        {
            if (currentTrigger == null) return Vector3.zero;
            RectTransform triggerRect = currentTrigger.GetComponent<RectTransform>();
            if (triggerRect == null) return Vector3.zero;

            Vector3[] corners = new Vector3[4];
            triggerRect.GetWorldCorners(corners);

            Vector3 worldTarget = Vector3.zero;
            Vector2 dir = Vector2.zero;

            float tipHeight = tooltipRect.rect.height * tooltipRect.localScale.y;
            float tipWidth = tooltipRect.rect.width * tooltipRect.localScale.x;
            float gap = defaultFixedGap;

            switch (anchor)
            {
                case TooltipAnchor.TopCenter: worldTarget = (corners[1] + corners[2]) / 2f; dir = new(0, tipHeight / 2 + gap); break;
                case TooltipAnchor.TopLeft: worldTarget = corners[1]; dir = new(tipWidth / 2, tipHeight / 2 + gap); break;
                case TooltipAnchor.TopRight: worldTarget = corners[2]; dir = new(-tipWidth / 2, tipHeight / 2 + gap); break;
                case TooltipAnchor.BottomCenter: worldTarget = (corners[0] + corners[3]) / 2f; dir = new(0, -(tipHeight / 2 + gap)); break;
                case TooltipAnchor.BottomLeft: worldTarget = corners[0]; dir = new(tipWidth / 2, -(tipHeight / 2 + gap)); break;
                case TooltipAnchor.BottomRight: worldTarget = corners[3]; dir = new(-tipWidth / 2, -(tipHeight / 2 + gap)); break;
                case TooltipAnchor.LeftCenter: worldTarget = (corners[0] + corners[1]) / 2f; dir = new(-(tipWidth / 2 + gap), 0); break;
                case TooltipAnchor.LeftTop: worldTarget = corners[1]; dir = new(-(tipWidth / 2 + gap), -tipHeight / 2); break;
                case TooltipAnchor.LeftBottom: worldTarget = corners[0]; dir = new(-(tipWidth / 2 + gap), tipHeight / 2); break;
                case TooltipAnchor.RightCenter: worldTarget = (corners[2] + corners[3]) / 2f; dir = new(tipWidth / 2 + gap, 0); break;
                case TooltipAnchor.RightTop: worldTarget = corners[2]; dir = new(tipWidth / 2 + gap, -tipHeight / 2); break;
                case TooltipAnchor.RightBottom: worldTarget = corners[3]; dir = new(tipWidth / 2 + gap, tipHeight / 2); break;
            }

            Canvas rootCanvas = tooltipInstance.GetComponentInParent<Canvas>();
            Camera uiCamera = (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : rootCanvas.worldCamera;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldTarget);

            if (ScreenToLocal(screenPoint, out Vector2 localPoint))
            {
                return new Vector3(localPoint.x + dir.x + currentOffset.x, localPoint.y + dir.y + currentOffset.y, 0);
            }
            return Vector3.zero;
        }

        private void ClampToScreen()
        {
            RectTransform parentRect = tooltipInstance.transform.parent as RectTransform;
            Vector3 currentLocalPos = tooltipInstance.transform.localPosition;
            Rect parentBounds = parentRect.rect;
            Rect tooltipBounds = tooltipRect.rect;
            Vector3 scale = tooltipInstance.transform.localScale;

            float scaledWidth = tooltipBounds.width * scale.x;
            float scaledHeight = tooltipBounds.height * scale.y;

            float pivotX = tooltipRect.pivot.x;
            float pivotY = tooltipRect.pivot.y;

            float left = currentLocalPos.x - (scaledWidth * pivotX);
            float right = currentLocalPos.x + (scaledWidth * (1f - pivotX));
            float bottom = currentLocalPos.y - (scaledHeight * pivotY);
            float top = currentLocalPos.y + (scaledHeight * (1f - pivotY));

            if (right > parentBounds.xMax) currentLocalPos.x -= (right - parentBounds.xMax);
            else if (left < parentBounds.xMin) currentLocalPos.x += (parentBounds.xMin - left);

            if (top > parentBounds.yMax) currentLocalPos.y -= (top - parentBounds.yMax);
            else if (bottom < parentBounds.yMin) currentLocalPos.y += (parentBounds.yMin - bottom);

            tooltipInstance.transform.localPosition = currentLocalPos;
        }

        private TooltipAnchor GetOppositeAnchor(TooltipAnchor anchor)
        {
            return anchor switch
            {
                TooltipAnchor.TopCenter => TooltipAnchor.BottomCenter,
                TooltipAnchor.TopLeft => TooltipAnchor.BottomLeft,
                TooltipAnchor.TopRight => TooltipAnchor.BottomRight,
                TooltipAnchor.BottomCenter => TooltipAnchor.TopCenter,
                TooltipAnchor.BottomLeft => TooltipAnchor.TopLeft,
                TooltipAnchor.BottomRight => TooltipAnchor.TopRight,
                TooltipAnchor.LeftCenter => TooltipAnchor.RightCenter,
                TooltipAnchor.LeftTop => TooltipAnchor.RightTop,
                TooltipAnchor.LeftBottom => TooltipAnchor.RightBottom,
                TooltipAnchor.RightCenter => TooltipAnchor.LeftCenter,
                TooltipAnchor.RightTop => TooltipAnchor.LeftTop,
                TooltipAnchor.RightBottom => TooltipAnchor.LeftBottom,
                _ => anchor
            };
        }

        private bool IsOutOfBounds(RectTransform rect)
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            foreach (var corner in corners)
            {
                if (corner.x < 0 || corner.x > Screen.width || corner.y < 0 || corner.y > Screen.height) return true;
            }
            return false;
        }

        private bool EnsureTooltipReady(Transform triggerContext)
        {
            Canvas targetCanvas = null;
            if (triggerContext != null)
            {
                Canvas foundCanvas = triggerContext.GetComponentInParent<Canvas>();
                if (foundCanvas != null) targetCanvas = foundCanvas.rootCanvas;
            }
            if (targetCanvas == null) targetCanvas = FindFirstObjectByType<Canvas>();
            if (targetCanvas == null) return false;

            if (tooltipInstance == null)
            {
                GameObject tooltipObj = Instantiate(tooltipPrefab.gameObject, targetCanvas.transform, false);
                tooltipInstance = tooltipObj.GetComponent<Tooltip>();
                tooltipRect = tooltipObj.GetComponent<RectTransform>();
                canvasGroup = tooltipObj.GetComponent<CanvasGroup>();
                tooltipObj.SetActive(false);
            }

            if (tooltipInstance.transform.parent != targetCanvas.transform)
            {
                tooltipInstance.transform.SetParent(targetCanvas.transform);
                tooltipInstance.transform.localScale = Vector3.one;
            }
            return true;
        }

        private bool ScreenToLocal(Vector2 screenPos, out Vector2 localPoint)
        {
            Canvas rootCanvas = tooltipInstance.GetComponentInParent<Canvas>();
            Camera uiCamera = (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : rootCanvas.worldCamera;
            RectTransform parentRect = tooltipInstance.transform.parent as RectTransform;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, uiCamera, out localPoint);
        }

        private float CalculateAvailableWidthForText(TMP_Text textElement, float maxWidth)
        {
            float availableWidth = maxWidth;
            if (textElement == null) return availableWidth;
            Transform current = textElement.transform;
            while (current != null && current != tooltipInstance.transform)
            {
                if (current.TryGetComponent<LayoutGroup>(out var layoutGroup))
                {
                    availableWidth -= (layoutGroup.padding.left + layoutGroup.padding.right);
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
                else line = testLine;
            }
            sb.Append(line);
            return sb.ToString();
        }
        #endregion
    }
}