namespace PixeLadder.EasyTooltip {
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
#endif

    public enum TooltipState {
        Idle,
        Delay,
        FadingIn,
        Visible,
        FadingOut
    }

    /// <summary>
    /// The core brain of the Easy Tooltip system. 
    /// Handles instantiation, object pooling for custom prefabs, animations, screen boundaries, and global default settings.
    /// This is an auto-generated Singleton.
    /// </summary>
    [AddComponentMenu("PixeLadder/Easy Tooltip/Tooltip Manager")]
    public class TooltipManager : MonoBehaviour {
        #region Static Instance
        public static TooltipManager Instance { get; private set; }
        #endregion

        #region Fields
        [Header("Core Configuration")]
        [Tooltip("The default visual tooltip prefab instantiated at runtime.")]
        [SerializeField] private Tooltip tooltipPrefab;

        [Header("Global Size")]
        [Tooltip("Default maximum width of the tooltip before the text is forced to wrap to a new line.")]
        [SerializeField, Min(50f)] private float defaultMaxWidth = 350f;

        [Header("Global Animation")]
        [Tooltip("Duration of the fade-in and fade-out animations in seconds.")]
        [SerializeField, Min(0f)] public float defaultFadeDuration = 0.2f;

        [Tooltip("Default time in seconds a user must hover over the trigger before the tooltip begins to show.")]
        public float defaultHoverDelay = 0.5f;

        [Header("Global Positioning")]
        [Tooltip("Base pixel offset applied to the tooltip when set to Follow Mouse mode.")]
        public Vector2 defaultMouseOffset = new(0, -20);

        [Tooltip("Pixel distance between the UI element and the tooltip when using Fixed positioning mode.")]
        public float defaultFixedGap = 5f;

        [Tooltip("If true, tooltips will automatically be clamped to stay within screen boundaries.")]
        public bool defaultClamping = true;

        [Tooltip("If true, fixed tooltips will automatically flip to the opposite side if they go off-screen.")]
        public bool smartFlipping = true;

        [Tooltip("If true, tooltips in Follow mode will continuously track the cursor movement while hovered.")]
        public bool defaultContinuousTracking = false;

        [Header("Global Style Defaults")]
        [Tooltip("Default color of the title text.")]
        public Color defaultTitleColor = Color.white;

        [Tooltip("Default color tint of the icon.")]
        public Color defaultIconColor = Color.white;

        [Space(5)]
        [Tooltip("Default color of the main background panel.")]
        public Color defaultPanelColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        [Tooltip("Default color tint of the header background layer.")]
        public Color defaultHeaderColor = new Color(0, 0, 0, 0);

        [Tooltip("Default color tint of the body background layer.")]
        public Color defaultBodyColor = new Color(0, 0, 0, 0);

        [Space(5)]
        [Tooltip("Default sprite used for auto-injected separators in multi-block tooltips.")]
        public Sprite defaultSeparatorSprite;

        [Tooltip("Default color tint of the separator sprite.")]
        public Color defaultSeparatorColor = Color.white;

        [Tooltip("Default height (thickness) of the auto-injected separators.")]
        public float defaultSeparatorHeight = 1f;

        [Space(5)]
        [Tooltip("Default toggle for the outline border visibility.")]
        public bool defaultShowOutline = true;

        [Tooltip("Default color tint of the outline border.")]
        public Color defaultOutlineColor = Color.white;

        public float DefaultMaxWidth => defaultMaxWidth;
        public TooltipState CurrentState { get; private set; } = TooltipState.Idle;
        public int CustomPoolCount => customTooltipPool.Count;
        public TooltipAnchor CurrentRenderedAnchor { get; private set; }
        public bool IsClamped { get; private set; }

        // Internal State Tracking
        private Tooltip defaultTooltipInstance;
        private Tooltip activeTooltipInstance;
        private RectTransform activeTooltipRect;
        private CanvasGroup activeCanvasGroup;

        // Object Pool for Custom Prefab Overrides
        private readonly Dictionary<Tooltip, Tooltip> customTooltipPool = new Dictionary<Tooltip, Tooltip>();

        private Coroutine activeShowCoroutine;
        private Coroutine activeHideCoroutine;

        // Current Request Context
        private TooltipTrigger currentTrigger;
        private Transform currentTargetOverride;
        private TooltipPositionMode currentMode;
        private TooltipAnchor currentAnchor;
        private Vector2 currentOffset;
        private float? currentWidth;
        private float currentFadeDuration;
        private bool currentContinuousTracking;
        private float currentFixedGap;
        private bool currentClamp;
        private bool currentFlip;
        #endregion

        #region Unity Lifecycle
        private void Awake() {
            if (Instance == null) {
                Instance = this;
                //if (Application.isPlaying) DontDestroyOnLoad(gameObject);
            } else {
                if (Application.isPlaying) Destroy(gameObject);
                else DestroyImmediate(gameObject);
            }
        }

        private void Update() {
            if (currentMode == TooltipPositionMode.FollowMouse && currentContinuousTracking) {
                if (activeTooltipInstance != null && activeTooltipInstance.gameObject.activeInHierarchy) {
                    PositionAtMouse();
                    if (currentClamp) ClampToScreen();
                }
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// Requests the manager to build, position, and animate the tooltip using the provided data and overrides.
        /// </summary>
        /// <param name="contentBlocks">List of text blocks to display.</param>
        /// <param name="title">Header text.</param>
        /// <param name="icon">Header sprite.</param>
        /// <param name="trigger">The component that triggered this tooltip.</param>
        /// <param name="targetOverride">Optional transform to anchor to instead of the trigger.</param>
        /// <param name="customPrefab">Optional specific Tooltip prefab to instantiate.</param>
        /// <param name="mode">Follow Mouse or Fixed positioning.</param>
        /// <param name="anchor">The pivot point when using Fixed positioning.</param>
        /// <param name="continuous">If true, constantly tracks the mouse while hovered.</param>
        public void ShowTooltip(List<string> contentBlocks, string title, Sprite icon,
            Color titleColor, Color iconColor,
            Color panelColor, Color headerColor, Color bodyColor,
            Sprite headerSprite, Sprite bodySprite,
            Sprite sepSprite, Color sepColor, float sepHeight,
            Color outlineColor, bool showOutline,
            float delay, float fadeDur, TooltipTrigger trigger, Transform targetOverride, Tooltip customPrefab,
            TooltipPositionMode mode, TooltipAnchor anchor, float gap, Vector2 offset, float? widthOverride, bool continuous,
            bool clamp, bool flip) {
            if (activeShowCoroutine != null) StopCoroutine(activeShowCoroutine);

            if (!Application.isPlaying) {
                currentTrigger = trigger;
                currentTargetOverride = targetOverride;
                currentMode = mode;
                currentAnchor = anchor;
                currentOffset = offset;
                currentWidth = widthOverride;
                currentContinuousTracking = continuous;
                currentFadeDuration = fadeDur;
                currentFixedGap = gap;
                currentClamp = clamp;
                currentFlip = flip;

                if (!EnsureTooltipReady(currentTrigger != null ? currentTrigger.transform : null, customPrefab)) return;

                if (activeCanvasGroup != null) activeCanvasGroup.alpha = 1;
                activeTooltipInstance.gameObject.SetActive(true);
                activeTooltipInstance.transform.SetAsLastSibling();

                float targetMax = currentWidth ?? defaultMaxWidth;
                activeTooltipInstance.SetContent(contentBlocks, title, icon, titleColor, iconColor, sepSprite, sepColor, sepHeight);
                activeTooltipInstance.SetStyle(panelColor, headerColor, bodyColor, headerSprite, bodySprite, outlineColor, showOutline);

                float availableTitleWidth = CalculateAvailableWidthForText(activeTooltipInstance.titleField, targetMax);
                string wrappedTitle = WrapText(title, activeTooltipInstance.titleField, availableTitleWidth);
                if (activeTooltipInstance.titleField != null) activeTooltipInstance.titleField.text = wrappedTitle;

                List<TextMeshProUGUI> activeTexts = activeTooltipInstance.GetActiveTextElements();
                foreach (var txtElement in activeTexts) {
                    float availableWidth = CalculateAvailableWidthForText(txtElement, targetMax);
                    txtElement.text = WrapText(txtElement.text, txtElement, availableWidth);
                }

                float maxCalculatedWidth = activeTooltipInstance.titleField != null ? activeTooltipInstance.titleField.preferredWidth : 0f;

                if (activeTooltipInstance.titleField != null) {
                    HorizontalLayoutGroup hlg = activeTooltipInstance.titleField.transform.parent.GetComponent<HorizontalLayoutGroup>();
                    if (hlg != null) {
                        maxCalculatedWidth += hlg.padding.left + hlg.padding.right;
                        if (icon != null && activeTooltipInstance.iconField != null && activeTooltipInstance.iconField.gameObject.activeSelf) {
                            maxCalculatedWidth += activeTooltipInstance.iconField.rectTransform.rect.width + hlg.spacing;
                        }
                    }
                }

                foreach (var txtElement in activeTexts) {
                    if (txtElement.preferredWidth > maxCalculatedWidth) maxCalculatedWidth = txtElement.preferredWidth;
                }

                LayoutElement layoutElement = activeTooltipInstance.GetComponent<LayoutElement>();
                if (layoutElement == null) layoutElement = activeTooltipInstance.gameObject.AddComponent<LayoutElement>();

                layoutElement.minWidth = Mathf.Min(maxCalculatedWidth, targetMax);

                // Unity UI quirk: Rebuilding multiple times ensures nested ContentSizeFitters calculate correct bounds.
                for (int i = 0; i < 3; i++) LayoutRebuilder.ForceRebuildLayoutImmediate(activeTooltipRect);

                Canvas.ForceUpdateCanvases();

                UpdatePosition();
                CurrentState = TooltipState.Visible;
                return;
            }

            activeShowCoroutine = StartCoroutine(ShowRoutine(contentBlocks, title, icon,
                titleColor, iconColor,
                panelColor, headerColor, bodyColor,
                headerSprite, bodySprite, sepSprite, sepColor, sepHeight,
                outlineColor, showOutline, delay, customPrefab,
                trigger, targetOverride, mode, anchor, gap, offset, widthOverride, continuous, clamp, flip, fadeDur));
        }

        /// <summary>
        /// Fades out and disables the currently active tooltip.
        /// </summary>
        public void HideTooltip() {
            CurrentState = TooltipState.Idle;

            if (activeShowCoroutine != null) StopCoroutine(activeShowCoroutine);

            if (activeTooltipInstance == null) return;

            if (!Application.isPlaying) {
                DestroyImmediate(activeTooltipInstance.gameObject);
                if (activeTooltipInstance == defaultTooltipInstance) defaultTooltipInstance = null;
                if (currentTrigger != null && currentTrigger.CustomPrefab != null) customTooltipPool.Remove(currentTrigger.CustomPrefab);
                activeTooltipInstance = null;
                return;
            }

            if (activeTooltipInstance.gameObject.activeInHierarchy || (activeCanvasGroup != null && activeCanvasGroup.alpha > 0)) {
                if (currentTrigger != null) currentTrigger.onTooltipHide?.Invoke();
                if (activeHideCoroutine != null) StopCoroutine(activeHideCoroutine);
                activeHideCoroutine = StartCoroutine(FadeOut(activeCanvasGroup, activeTooltipInstance, currentFadeDuration));
            }
        }
        #endregion

        #region Coroutines
        private IEnumerator ShowRoutine(List<string> contentBlocks, string title, Sprite icon,
            Color titleColor, Color iconColor,
            Color panelColor, Color headerColor, Color bodyColor,
            Sprite headerSprite, Sprite bodySprite, Sprite sepSprite, Color sepColor, float sepHeight,
            Color outlineColor, bool showOutline, float delay, Tooltip customPrefab,
            TooltipTrigger trigger, Transform targetOverride, TooltipPositionMode mode, TooltipAnchor anchor,
            float gap, Vector2 offset, float? widthOverride, bool continuous, bool clamp, bool flip, float fadeDur) {
            CurrentState = TooltipState.Delay;

            if (delay > 0) {
                yield return new WaitForSeconds(delay);
                if (CurrentState != TooltipState.Delay) yield break;
            }

            if (activeHideCoroutine != null) StopCoroutine(activeHideCoroutine);

            currentTrigger = trigger;
            currentTargetOverride = targetOverride;
            currentMode = mode;
            currentAnchor = anchor;
            currentOffset = offset;
            currentWidth = widthOverride;
            currentContinuousTracking = continuous;
            currentFadeDuration = fadeDur;
            currentFixedGap = gap;
            currentClamp = clamp;
            currentFlip = flip;

            if (!EnsureTooltipReady(currentTrigger != null ? currentTrigger.transform : null, customPrefab)) yield break;

            if (activeCanvasGroup != null) activeCanvasGroup.alpha = 0;

            yield return ResizeTooltipRoutine(contentBlocks, title, icon,
                titleColor, iconColor, panelColor, headerColor, bodyColor,
                headerSprite, bodySprite, sepSprite, sepColor, sepHeight, outlineColor, showOutline);

            activeTooltipInstance.gameObject.SetActive(true);
            activeTooltipInstance.transform.SetAsLastSibling();
            UpdatePosition();

            if (currentTrigger != null) currentTrigger.onTooltipShow?.Invoke();

            CurrentState = TooltipState.FadingIn;
            activeShowCoroutine = StartCoroutine(FadeIn());
        }

        private IEnumerator ResizeTooltipRoutine(List<string> contentBlocks, string title, Sprite icon,
            Color titleColor, Color iconColor, Color panelColor, Color headerColor, Color bodyColor,
            Sprite headerSprite, Sprite bodySprite, Sprite sepSprite, Color sepColor, float sepHeight,
            Color outlineColor, bool showOutline) {
            activeTooltipInstance.gameObject.SetActive(true);

            float targetMax = currentWidth ?? defaultMaxWidth;

            activeTooltipInstance.SetContent(contentBlocks, title, icon, titleColor, iconColor, sepSprite, sepColor, sepHeight);
            activeTooltipInstance.SetStyle(panelColor, headerColor, bodyColor, headerSprite, bodySprite, outlineColor, showOutline);

            float availableTitleWidth = CalculateAvailableWidthForText(activeTooltipInstance.titleField, targetMax);

            string wrappedTitle = WrapText(title, activeTooltipInstance.titleField, availableTitleWidth);
            if (activeTooltipInstance.titleField != null) activeTooltipInstance.titleField.text = wrappedTitle;

            List<TextMeshProUGUI> activeTexts = activeTooltipInstance.GetActiveTextElements();
            foreach (var txtElement in activeTexts) {
                float availableWidth = CalculateAvailableWidthForText(txtElement, targetMax);
                txtElement.text = WrapText(txtElement.text, txtElement, availableWidth);
            }

            float maxCalculatedWidth = activeTooltipInstance.titleField != null ? activeTooltipInstance.titleField.preferredWidth : 0f;

            if (activeTooltipInstance.titleField != null) {
                HorizontalLayoutGroup hlg = activeTooltipInstance.titleField.transform.parent.GetComponent<HorizontalLayoutGroup>();
                if (hlg != null) {
                    maxCalculatedWidth += hlg.padding.left + hlg.padding.right;
                    if (icon != null && activeTooltipInstance.iconField != null && activeTooltipInstance.iconField.gameObject.activeSelf) {
                        maxCalculatedWidth += activeTooltipInstance.iconField.rectTransform.rect.width + hlg.spacing;
                    }
                }
            }

            foreach (var txtElement in activeTexts) {
                if (txtElement.preferredWidth > maxCalculatedWidth) maxCalculatedWidth = txtElement.preferredWidth;
            }

            LayoutElement layoutElement = activeTooltipInstance.GetComponent<LayoutElement>();
            if (layoutElement == null) layoutElement = activeTooltipInstance.gameObject.AddComponent<LayoutElement>();

            layoutElement.minWidth = Mathf.Min(maxCalculatedWidth, targetMax);

            for (int i = 0; i < 3; i++) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(activeTooltipRect);
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator FadeIn() {
            float start = Time.unscaledTime;
            while (Time.unscaledTime < start + currentFadeDuration) {
                if (activeCanvasGroup == null) yield break;
                activeCanvasGroup.alpha = Mathf.Lerp(0, 1, (Time.unscaledTime - start) / currentFadeDuration);
                yield return null;
            }
            if (activeCanvasGroup != null) activeCanvasGroup.alpha = 1;
            CurrentState = TooltipState.Visible;
        }

        private IEnumerator FadeOut(CanvasGroup targetGroup, Tooltip targetTooltip, float duration) {
            if (CurrentState != TooltipState.Delay) CurrentState = TooltipState.FadingOut;

            float start = Time.unscaledTime;
            float startAlpha = targetGroup != null ? targetGroup.alpha : 1f;

            while (Time.unscaledTime < start + duration) {
                if (targetGroup == null) yield break;
                targetGroup.alpha = Mathf.Lerp(startAlpha, 0, (Time.unscaledTime - start) / duration);
                yield return null;
            }

            if (targetGroup != null) targetGroup.alpha = 0;
            if (targetTooltip != null) targetTooltip.gameObject.SetActive(false);

            if (CurrentState == TooltipState.FadingOut) {
                CurrentState = TooltipState.Idle;
            }
        }
        #endregion

        #region Positioning Logic
        private void UpdatePosition() {
            if (activeTooltipInstance == null) return;

            activeTooltipRect.anchorMin = new Vector2(0.5f, 0.5f);
            activeTooltipRect.anchorMax = new Vector2(0.5f, 0.5f);

            TooltipPositionMode modeToUse = currentMode;
            TooltipAnchor anchorToUse = currentAnchor;

            if (!Application.isPlaying && modeToUse == TooltipPositionMode.FollowMouse) {
                modeToUse = TooltipPositionMode.Fixed;
                anchorToUse = TooltipAnchor.BottomLeft;
            }

            if (modeToUse == TooltipPositionMode.FollowMouse) {
                activeTooltipRect.pivot = new Vector2(0, 1);
                PositionAtMouse();

                if (!Application.isPlaying) Canvas.ForceUpdateCanvases();
                if (currentClamp) ClampToScreen();
            } else {
                activeTooltipRect.pivot = new Vector2(0.5f, 0.5f);
                Vector3 preferredPos = CalculateFixedPosition(anchorToUse);
                CurrentRenderedAnchor = anchorToUse;

                activeTooltipInstance.transform.localPosition = preferredPos;

                if (!Application.isPlaying) Canvas.ForceUpdateCanvases();

                if (currentFlip && IsOutOfBounds(activeTooltipRect, out bool hitX, out bool hitY)) {
                    TooltipAnchor targetAnchor = anchorToUse;
                    if (hitX) targetAnchor = FlipHorizontal(targetAnchor);
                    if (hitY) targetAnchor = FlipVertical(targetAnchor);

                    Vector3 flippedPos = CalculateFixedPosition(targetAnchor);
                    activeTooltipInstance.transform.localPosition = flippedPos;
                    CurrentRenderedAnchor = targetAnchor;
                }

                if (currentClamp) ClampToScreen();
            }
        }

        private void PositionAtMouse() {
#if ENABLE_INPUT_SYSTEM
            Vector2 screenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
            Vector2 screenPos = Input.mousePosition;
#endif
            screenPos += defaultMouseOffset + currentOffset;
            ScreenToLocal(screenPos, out Vector2 localPoint);
            activeTooltipInstance.transform.localPosition = new Vector3(localPoint.x, localPoint.y, 0);
        }

        private Vector3 CalculateFixedPosition(TooltipAnchor anchor) {
            Transform targetTransform = currentTargetOverride != null ? currentTargetOverride : (currentTrigger != null ? currentTrigger.transform : null);
            if (targetTransform == null) return activeTooltipRect.localPosition;

            RectTransform targetRect = targetTransform.GetComponent<RectTransform>();
            if (targetRect == null) return activeTooltipRect.localPosition;

            RectTransform parentRect = activeTooltipInstance.transform.parent as RectTransform;
            if (parentRect == null) return activeTooltipRect.localPosition;

            Vector3[] corners = new Vector3[4];
            targetRect.GetWorldCorners(corners);

            Vector3 localBottomLeft = parentRect.InverseTransformPoint(corners[0]);
            Vector3 localTopLeft = parentRect.InverseTransformPoint(corners[1]);
            Vector3 localTopRight = parentRect.InverseTransformPoint(corners[2]);
            Vector3 localBottomRight = parentRect.InverseTransformPoint(corners[3]);

            Vector3 localTarget = Vector3.zero;
            Vector2 dir = Vector2.zero;

            float tipHeight = activeTooltipRect.rect.height;
            float tipWidth = activeTooltipRect.rect.width;
            float gap = currentFixedGap;

            switch (anchor) {
                case TooltipAnchor.TopCenter: localTarget = (localTopLeft + localTopRight) / 2f; dir = new(0, tipHeight / 2 + gap); break;
                case TooltipAnchor.TopLeft: localTarget = localTopLeft; dir = new(tipWidth / 2, tipHeight / 2 + gap); break;
                case TooltipAnchor.TopRight: localTarget = localTopRight; dir = new(-tipWidth / 2, tipHeight / 2 + gap); break;
                case TooltipAnchor.BottomCenter: localTarget = (localBottomLeft + localBottomRight) / 2f; dir = new(0, -(tipHeight / 2 + gap)); break;
                case TooltipAnchor.BottomLeft: localTarget = localBottomLeft; dir = new(tipWidth / 2, -(tipHeight / 2 + gap)); break;
                case TooltipAnchor.BottomRight: localTarget = localBottomRight; dir = new(-tipWidth / 2, -(tipHeight / 2 + gap)); break;
                case TooltipAnchor.LeftCenter: localTarget = (localBottomLeft + localTopLeft) / 2f; dir = new(-(tipWidth / 2 + gap), 0); break;
                case TooltipAnchor.LeftTop: localTarget = localTopLeft; dir = new(-(tipWidth / 2 + gap), -tipHeight / 2); break;
                case TooltipAnchor.LeftBottom: localTarget = localBottomLeft; dir = new(-(tipWidth / 2 + gap), tipHeight / 2); break;
                case TooltipAnchor.RightCenter: localTarget = (localBottomRight + localTopRight) / 2f; dir = new(tipWidth / 2 + gap, 0); break;
                case TooltipAnchor.RightTop: localTarget = localTopRight; dir = new(tipWidth / 2 + gap, -tipHeight / 2); break;
                case TooltipAnchor.RightBottom: localTarget = localBottomRight; dir = new(tipWidth / 2 + gap, tipHeight / 2); break;
            }

            return localTarget + new Vector3(dir.x + currentOffset.x, dir.y + currentOffset.y, 0);
        }

        private void ClampToScreen() {
            RectTransform parentRect = activeTooltipInstance.transform.parent as RectTransform;
            Vector3 currentLocalPos = activeTooltipInstance.transform.localPosition;
            Rect parentBounds = parentRect.rect;
            Rect tooltipBounds = activeTooltipRect.rect;
            Vector3 scale = activeTooltipInstance.transform.localScale;

            float scaledWidth = tooltipBounds.width * scale.x;
            float scaledHeight = tooltipBounds.height * scale.y;

            float pivotX = activeTooltipRect.pivot.x;
            float pivotY = activeTooltipRect.pivot.y;

            float left = currentLocalPos.x - (scaledWidth * pivotX);
            float right = currentLocalPos.x + (scaledWidth * (1f - pivotX));
            float bottom = currentLocalPos.y - (scaledHeight * pivotY);
            float top = currentLocalPos.y + (scaledHeight * (1f - pivotY));

            IsClamped = false;

            if (right > parentBounds.xMax) { currentLocalPos.x -= (right - parentBounds.xMax); IsClamped = true; } else if (left < parentBounds.xMin) { currentLocalPos.x += (parentBounds.xMin - left); IsClamped = true; }

            if (top > parentBounds.yMax) { currentLocalPos.y -= (top - parentBounds.yMax); IsClamped = true; } else if (bottom < parentBounds.yMin) { currentLocalPos.y += (parentBounds.yMin - bottom); IsClamped = true; }

            activeTooltipInstance.transform.localPosition = currentLocalPos;
        }

        private TooltipAnchor FlipHorizontal(TooltipAnchor anchor) {
            return anchor switch {
                TooltipAnchor.TopLeft => TooltipAnchor.TopRight,
                TooltipAnchor.TopRight => TooltipAnchor.TopLeft,
                TooltipAnchor.BottomLeft => TooltipAnchor.BottomRight,
                TooltipAnchor.BottomRight => TooltipAnchor.BottomLeft,
                TooltipAnchor.LeftCenter => TooltipAnchor.RightCenter,
                TooltipAnchor.LeftTop => TooltipAnchor.RightTop,
                TooltipAnchor.LeftBottom => TooltipAnchor.RightBottom,
                TooltipAnchor.RightCenter => TooltipAnchor.LeftCenter,
                TooltipAnchor.RightTop => TooltipAnchor.LeftTop,
                TooltipAnchor.RightBottom => TooltipAnchor.LeftBottom,
                _ => anchor
            };
        }

        private TooltipAnchor FlipVertical(TooltipAnchor anchor) {
            return anchor switch {
                TooltipAnchor.TopCenter => TooltipAnchor.BottomCenter,
                TooltipAnchor.TopLeft => TooltipAnchor.BottomLeft,
                TooltipAnchor.TopRight => TooltipAnchor.BottomRight,
                TooltipAnchor.BottomCenter => TooltipAnchor.TopCenter,
                TooltipAnchor.BottomLeft => TooltipAnchor.TopLeft,
                TooltipAnchor.BottomRight => TooltipAnchor.TopRight,
                TooltipAnchor.LeftTop => TooltipAnchor.LeftBottom,
                TooltipAnchor.LeftBottom => TooltipAnchor.LeftTop,
                TooltipAnchor.RightTop => TooltipAnchor.RightBottom,
                TooltipAnchor.RightBottom => TooltipAnchor.RightTop,
                _ => anchor
            };
        }

        private bool IsOutOfBounds(RectTransform rect, out bool hitHorizontal, out bool hitVertical) {
            hitHorizontal = false;
            hitVertical = false;

            RectTransform parentRect = rect.parent as RectTransform;
            if (parentRect == null) return false;

            Rect parentBounds = parentRect.rect;
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);

            foreach (var corner in corners) {
                Vector3 localCorner = parentRect.InverseTransformPoint(corner);
                if (localCorner.x < parentBounds.xMin || localCorner.x > parentBounds.xMax) hitHorizontal = true;
                if (localCorner.y < parentBounds.yMin || localCorner.y > parentBounds.yMax) hitVertical = true;
            }

            return hitHorizontal || hitVertical;
        }

        private bool EnsureTooltipReady(Transform triggerContext, Tooltip customPrefab) {
            Canvas targetCanvas = null;
            if (triggerContext != null) {
                Canvas foundCanvas = triggerContext.GetComponentInParent<Canvas>();
                if (foundCanvas != null) targetCanvas = foundCanvas.rootCanvas;
            }

            if (targetCanvas == null) targetCanvas = FindAnyObjectByType<Canvas>();
            if (targetCanvas == null) return false;

            Tooltip previousTooltip = activeTooltipInstance;

            if (customPrefab != null) {
                if (!customTooltipPool.TryGetValue(customPrefab, out Tooltip pooledInstance) || pooledInstance == null) {
                    GameObject newObj = Instantiate(customPrefab.gameObject, targetCanvas.transform, false);
                    if (!Application.isPlaying) newObj.hideFlags = HideFlags.HideAndDontSave;
                    pooledInstance = newObj.GetComponent<Tooltip>();
                    customTooltipPool[customPrefab] = pooledInstance;
                }
                activeTooltipInstance = pooledInstance;
            } else {
                if (defaultTooltipInstance == null) {
                    GameObject tooltipObj = Instantiate(tooltipPrefab.gameObject, targetCanvas.transform, false);
                    if (!Application.isPlaying) tooltipObj.hideFlags = HideFlags.HideAndDontSave;
                    defaultTooltipInstance = tooltipObj.GetComponent<Tooltip>();
                }
                activeTooltipInstance = defaultTooltipInstance;
            }

            if (previousTooltip != null && previousTooltip != activeTooltipInstance) {
                previousTooltip.gameObject.SetActive(false);
            }

            activeTooltipRect = activeTooltipInstance.GetComponent<RectTransform>();
            activeCanvasGroup = activeTooltipInstance.GetComponent<CanvasGroup>();
            activeTooltipInstance.gameObject.SetActive(false);

            if (activeTooltipInstance.transform.parent != targetCanvas.transform) {
                activeTooltipInstance.transform.SetParent(targetCanvas.transform);
                activeTooltipInstance.transform.localScale = Vector3.one;
            }
            return true;
        }

        private bool ScreenToLocal(Vector2 screenPos, out Vector2 localPoint) {
            Canvas rootCanvas = activeTooltipInstance.GetComponentInParent<Canvas>();
            Camera uiCamera = (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : rootCanvas.worldCamera;
            RectTransform parentRect = activeTooltipInstance.transform.parent as RectTransform;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, uiCamera, out localPoint);
        }

        private float CalculateAvailableWidthForText(TMP_Text textElement, float maxWidth) {
            float availableWidth = maxWidth;
            if (textElement == null) return availableWidth;

            Transform current = textElement.transform;
            while (current != null && current != activeTooltipInstance.transform) {
                if (current.TryGetComponent<LayoutGroup>(out var layoutGroup)) {
                    availableWidth -= (layoutGroup.padding.left + layoutGroup.padding.right);
                }
                current = current.parent;
            }
            return availableWidth;
        }

        private string WrapText(string text, TMP_Text tmp, float maxWidth) {
            if (string.IsNullOrEmpty(text) || tmp == null) return text;
            if (tmp.GetPreferredValues(text).x <= maxWidth) return text;

            StringBuilder sb = new StringBuilder();
            string[] words = text.Split(' ');
            string line = "";

            for (int i = 0; i < words.Length; i++) {
                string word = words[i];
                string testLine = string.IsNullOrEmpty(line) ? word : $"{line} {word}";

                if (tmp.GetPreferredValues(testLine).x > maxWidth && !string.IsNullOrEmpty(line)) {
                    sb.AppendLine(line);
                    line = word;
                } else line = testLine;
            }
            sb.Append(line);
            return sb.ToString();
        }
        #endregion

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() {
            Instance = null;
        }
    }
}