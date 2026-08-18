namespace PixeLadder.EasyTooltip {
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;

    /// <summary>
    /// Add this component to any UI element to instantly give it a tooltip. 
    /// Handles pointer events and allows per-instance style, layout, and prefab overrides.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("PixeLadder/Easy Tooltip/Tooltip Trigger")]
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        #region Fields
        [Header("Content")]
        [Tooltip("Optional: Assign a specific Tooltip prefab to use instead of the global default. Perfect for complex RPG stat cards.")]
        [SerializeField] private Tooltip customPrefab;

        [Tooltip("The header text of the tooltip. Leave empty to hide.")]
        [SerializeField] private string title;

        [Tooltip("The main body text of the tooltip.")]
        [TextArea(3, 10)]
        [SerializeField] private string content;

        [Tooltip("Additional text blocks. If the default prefab is used, separators will be automatically injected between these blocks.")]
        [SerializeField] private List<string> secondaryContent = new List<string>();

        [Tooltip("An optional icon to display next to the title.")]
        [SerializeField] private Sprite icon;

        [Header("Style Overrides")]
        [SerializeField] private bool overrideStyle = false;

        [Tooltip("Color of the title text.")]
        [SerializeField] private Color titleColor = Color.white;

        [Tooltip("Color tint of the icon.")]
        [SerializeField] private Color iconColor = Color.white;

        [Tooltip("Color of the tooltip's main background panel.")]
        [SerializeField] private Color panelColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        [Tooltip("Color tint of the header background layer.")]
        [SerializeField] private Color headerColor = new Color(0, 0, 0, 0);

        [Tooltip("Color tint of the body background layer.")]
        [SerializeField] private Color bodyColor = new Color(0, 0, 0, 0);

        [Tooltip("Optional override sprite for the header background.")]
        [SerializeField] private Sprite headerSprite;

        [Tooltip("Optional override sprite for the body background.")]
        [SerializeField] private Sprite bodySprite;

        [Tooltip("Enable or disable the outline border image.")]
        [SerializeField] private bool showOutline = true;

        [Tooltip("Color tint of the outline border.")]
        [SerializeField] private Color outlineColor = Color.white;

        [Header("Separator Overrides")]
        [SerializeField] private bool overrideSeparators = false;

        [Tooltip("Override height (thickness) of the separator.")]
        [SerializeField] private float separatorHeight = 1f;

        [Tooltip("Override sprite used for auto-injected separators in multi-block tooltips.")]
        [SerializeField] private Sprite separatorSprite;

        [Tooltip("Override color tint for the separator sprite.")]
        [SerializeField] private Color separatorColor = Color.white;

        [Header("Layout & Positioning")]
        [SerializeField] private bool overrideLayout = false;

        [Tooltip("Should the tooltip follow the mouse or stay in a fixed position?")]
        [SerializeField] private TooltipPositionMode positionMode = TooltipPositionMode.FollowMouse;

        [Tooltip("If true, the tooltip will constantly update its position to follow the mouse while hovering.")]
        [SerializeField] private bool continuousTracking = false;

        [Tooltip("The point on the target UI element where the tooltip will be pinned.")]
        [SerializeField] private TooltipAnchor anchorPosition = TooltipAnchor.TopCenter;

        [SerializeField] private bool overrideGap = false;

        [Tooltip("Pixel distance between the UI element and the tooltip when using Fixed positioning mode.")]
        [SerializeField] private float fixedGap = 5f;

        [Tooltip("Optional: Anchor the tooltip to this specific Transform instead of the object you are hovering over. Perfect for Map markers.")]
        [SerializeField] private Transform targetOverride;

        [Tooltip("Additional X/Y pixel offset applied to the tooltip.")]
        [SerializeField] private Vector2 additionalOffset = Vector2.zero;

        [Header("Screen Constraints")]
        [SerializeField] private bool overrideConstraints = false;

        [Tooltip("If true, the tooltip will be forced to stay within screen boundaries.")]
        [SerializeField] private bool enableClamping = true;

        [Tooltip("If true, fixed tooltips will automatically flip if they go off-screen.")]
        [SerializeField] private bool smartFlipping = true;

        [Header("Size & Constraints")]
        [SerializeField] private bool overrideSize = false;

        [Tooltip("The maximum width in pixels before the text wraps to a new line.")]
        [SerializeField, Min(50f)] private float maxWidth = 350f;

        [Header("Animation & Timing")]
        [SerializeField] private bool overrideTiming = false;

        [Tooltip("Time in seconds the user must hover before the tooltip appears.")]
        [SerializeField, Min(0f)] private float hoverDelay = 0.5f;

        [Tooltip("Duration of the fade-in and fade-out animations in seconds.")]
        [SerializeField, Min(0f)] private float fadeDuration = 0.2f;

        [Header("Events")]
        [Tooltip("Fired when the tooltip becomes fully visible.")]
        public UnityEvent onTooltipShow;

        [Tooltip("Fired when the tooltip begins hiding.")]
        public UnityEvent onTooltipHide;
        #endregion

        #region Public Properties
        /// <summary>The custom prefab used for this specific trigger. Can be null to use the default.</summary>
        public Tooltip CustomPrefab { get => customPrefab; set => customPrefab = value; }

        /// <summary>The header text of the tooltip.</summary>
        public string Title { get => title; set => title = value; }

        /// <summary>The main body text of the tooltip.</summary>
        public string Content { get => content; set => content = value; }

        /// <summary>Additional text blocks separated by auto-injected images.</summary>
        public List<string> SecondaryContent { get => secondaryContent; set => secondaryContent = value; }

        /// <summary>An optional icon to display next to the title.</summary>
        public Sprite Icon { get => icon; set => icon = value; }

        public Color TitleColor { get => (overrideStyle || TooltipManager.Instance == null) ? titleColor : TooltipManager.Instance.defaultTitleColor; set { titleColor = value; overrideStyle = true; } }
        public Color IconColor { get => (overrideStyle || TooltipManager.Instance == null) ? iconColor : TooltipManager.Instance.defaultIconColor; set { iconColor = value; overrideStyle = true; } }
        public Color PanelColor { get => (overrideStyle || TooltipManager.Instance == null) ? panelColor : TooltipManager.Instance.defaultPanelColor; set { panelColor = value; overrideStyle = true; } }
        public Color HeaderColor { get => (overrideStyle || TooltipManager.Instance == null) ? headerColor : TooltipManager.Instance.defaultHeaderColor; set { headerColor = value; overrideStyle = true; } }
        public Color BodyColor { get => (overrideStyle || TooltipManager.Instance == null) ? bodyColor : TooltipManager.Instance.defaultBodyColor; set { bodyColor = value; overrideStyle = true; } }
        public Sprite HeaderSprite { get => overrideStyle ? headerSprite : null; set { headerSprite = value; overrideStyle = true; } }
        public Sprite BodySprite { get => overrideStyle ? bodySprite : null; set { bodySprite = value; overrideStyle = true; } }
        public Color OutlineColor { get => (overrideStyle || TooltipManager.Instance == null) ? outlineColor : TooltipManager.Instance.defaultOutlineColor; set { outlineColor = value; overrideStyle = true; } }
        public bool ShowOutline { get => (overrideStyle || TooltipManager.Instance == null) ? showOutline : TooltipManager.Instance.defaultShowOutline; set { showOutline = value; overrideStyle = true; } }

        public Sprite SeparatorSprite { get => (overrideSeparators || TooltipManager.Instance == null) ? separatorSprite : TooltipManager.Instance.defaultSeparatorSprite; set { separatorSprite = value; overrideSeparators = true; } }
        public Color SeparatorColor { get => (overrideSeparators || TooltipManager.Instance == null) ? separatorColor : TooltipManager.Instance.defaultSeparatorColor; set { separatorColor = value; overrideSeparators = true; } }
        public float SeparatorHeight { get => (overrideSeparators || TooltipManager.Instance == null) ? separatorHeight : TooltipManager.Instance.defaultSeparatorHeight; set { separatorHeight = value; overrideSeparators = true; } }

        public TooltipPositionMode PositionMode { get => positionMode; set { positionMode = value; overrideLayout = true; } }
        public bool ContinuousTracking { get => (overrideLayout || TooltipManager.Instance == null) ? continuousTracking : TooltipManager.Instance.defaultContinuousTracking; set { continuousTracking = value; overrideLayout = true; } }
        public TooltipAnchor AnchorPosition { get => anchorPosition; set { anchorPosition = value; overrideLayout = true; } }
        public float FixedGap { get => (overrideGap || TooltipManager.Instance == null) ? fixedGap : TooltipManager.Instance.defaultFixedGap; set { fixedGap = value; overrideGap = true; } }
        public Transform TargetOverride { get => targetOverride; set { targetOverride = value; overrideLayout = true; } }
        public Vector2 AdditionalOffset { get => additionalOffset; set { additionalOffset = value; overrideLayout = true; } }
        public bool EnableClamping { get => (overrideConstraints || TooltipManager.Instance == null) ? enableClamping : TooltipManager.Instance.defaultClamping; set { enableClamping = value; overrideConstraints = true; } }
        public bool SmartFlipping { get => (overrideConstraints || TooltipManager.Instance == null) ? smartFlipping : TooltipManager.Instance.smartFlipping; set { smartFlipping = value; overrideConstraints = true; } }

        public float MaxWidth { get => (overrideSize || TooltipManager.Instance == null) ? maxWidth : TooltipManager.Instance.DefaultMaxWidth; set { maxWidth = value; overrideSize = true; } }

        public float HoverDelay { get => (overrideTiming || TooltipManager.Instance == null) ? hoverDelay : TooltipManager.Instance.defaultHoverDelay; set { hoverDelay = value; overrideTiming = true; } }
        public float FadeDuration { get => (overrideTiming || TooltipManager.Instance == null) ? fadeDuration : TooltipManager.Instance.defaultFadeDuration; set { fadeDuration = value; overrideTiming = true; } }
        #endregion

        #region Lifecycle
        private void Reset() => EnsureManagerExists();
        private void OnEnable() { if (Application.isPlaying) EnsureManagerExists(); }
        #endregion

        #region Editor Preview
#if UNITY_EDITOR
        public void EditorPreviewTooltip() {
            TooltipManager manager = TooltipManager.Instance;
            if (manager == null) manager = FindAnyObjectByType<TooltipManager>();
            if (manager == null) {
                EnsureManagerExists();
                manager = FindAnyObjectByType<TooltipManager>();
            }
            if (manager == null) return;

            List<string> fullContentList = new List<string>();
            if (!string.IsNullOrEmpty(content)) fullContentList.Add(content);
            if (secondaryContent != null) fullContentList.AddRange(secondaryContent);

            var finalMode = overrideLayout ? positionMode : TooltipPositionMode.FollowMouse;
            var finalAnchor = overrideLayout ? anchorPosition : TooltipAnchor.TopCenter;
            var finalGap = overrideGap ? fixedGap : (TooltipManager.Instance != null ? TooltipManager.Instance.defaultFixedGap : 5f);
            var finalTarget = overrideLayout ? targetOverride : null;
            var finalOffset = overrideLayout ? additionalOffset : Vector2.zero;
            var finalContinuous = overrideLayout ? continuousTracking : (TooltipManager.Instance != null && TooltipManager.Instance.defaultContinuousTracking);
            float? finalWidth = overrideSize ? maxWidth : null;

            bool finalClamp = overrideConstraints ? enableClamping : (TooltipManager.Instance == null || TooltipManager.Instance.defaultClamping);
            bool finalFlip = overrideConstraints ? smartFlipping : (TooltipManager.Instance == null || TooltipManager.Instance.smartFlipping);

            manager.ShowTooltip(
                fullContentList, title, icon,
                TitleColor, IconColor,
                PanelColor, HeaderColor, BodyColor,
                HeaderSprite, BodySprite, SeparatorSprite, SeparatorColor, SeparatorHeight,
                OutlineColor, ShowOutline, 0f, 0f,
                this, finalTarget, customPrefab,
                finalMode, finalAnchor, finalGap, finalOffset, finalWidth, finalContinuous,
                finalClamp, finalFlip
            );
        }

        public void EditorHideTooltip() {
            TooltipManager manager = TooltipManager.Instance != null ? TooltipManager.Instance : FindAnyObjectByType<TooltipManager>();
            if (manager != null) {
                manager.HideTooltip();
            }
        }
#endif
        #endregion

        #region Public API
        /// <summary>
        /// Instantly forces the tooltip to appear. Can be called via UnityEvents (e.g. Button OnClick) or code.
        /// </summary>
        public void ShowTooltip() {
            if (TooltipManager.Instance == null) return;

            List<string> fullContentList = new List<string>();
            if (!string.IsNullOrEmpty(content)) fullContentList.Add(content);
            if (secondaryContent != null) fullContentList.AddRange(secondaryContent);

            var finalMode = overrideLayout ? positionMode : TooltipPositionMode.FollowMouse;
            var finalAnchor = overrideLayout ? anchorPosition : TooltipAnchor.TopCenter;

            if (!Application.isPlaying && finalMode == TooltipPositionMode.FollowMouse) {
                finalMode = TooltipPositionMode.Fixed;
                finalAnchor = TooltipAnchor.BottomCenter;
            }

            var finalGap = overrideGap ? fixedGap : (TooltipManager.Instance != null ? TooltipManager.Instance.defaultFixedGap : 5f);
            var finalTarget = overrideLayout ? targetOverride : null;
            var finalOffset = overrideLayout ? additionalOffset : Vector2.zero;
            var finalContinuous = overrideLayout ? continuousTracking : (TooltipManager.Instance != null && TooltipManager.Instance.defaultContinuousTracking);
            float? finalWidth = overrideSize ? maxWidth : null;
            bool finalClamp = overrideConstraints ? enableClamping : (TooltipManager.Instance == null || TooltipManager.Instance.defaultClamping);
            bool finalFlip = overrideConstraints ? smartFlipping : (TooltipManager.Instance == null || TooltipManager.Instance.smartFlipping);

            TooltipManager.Instance.ShowTooltip(
                fullContentList, title, icon,
                TitleColor, IconColor,
                PanelColor, HeaderColor, BodyColor,
                HeaderSprite, BodySprite, SeparatorSprite, SeparatorColor, SeparatorHeight,
                OutlineColor, ShowOutline, HoverDelay, FadeDuration,
                this, finalTarget, customPrefab,
                finalMode, finalAnchor, finalGap, finalOffset, finalWidth, finalContinuous,
                finalClamp, finalFlip
            );
        }

        /// <summary>
        /// Instantly forces the tooltip to fade out and hide.
        /// </summary>
        public void HideTooltip() {
            if (TooltipManager.Instance != null) {
                TooltipManager.Instance.HideTooltip();
            }
        }
        #endregion

        #region Interface Implementations
        public void OnPointerEnter(PointerEventData eventData) {
            ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData) {
            HideTooltip();
        }
        #endregion

        #region Gizmos & Helpers
        /// <summary>
        /// Draws a cyan sphere in the Scene View to visualize where the Fixed tooltip will anchor.
        /// </summary>
        private void OnDrawGizmosSelected() {
            if (!overrideLayout || positionMode != TooltipPositionMode.Fixed) return;

            Transform anchorTarget = targetOverride != null ? targetOverride : transform;
            RectTransform rect = anchorTarget.GetComponent<RectTransform>();
            if (rect == null) return;

            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector3 target = Vector3.zero;

            switch (anchorPosition) {
                case TooltipAnchor.TopCenter: target = (corners[1] + corners[2]) / 2f; break;
                case TooltipAnchor.TopLeft: target = corners[1]; break;
                case TooltipAnchor.TopRight: target = corners[2]; break;
                case TooltipAnchor.BottomCenter: target = (corners[0] + corners[3]) / 2f; break;
                case TooltipAnchor.BottomLeft: target = corners[0]; break;
                case TooltipAnchor.BottomRight: target = corners[3]; break;
                case TooltipAnchor.LeftCenter: target = (corners[0] + corners[1]) / 2f; break;
                case TooltipAnchor.LeftTop: target = corners[1]; break;
                case TooltipAnchor.LeftBottom: target = corners[0]; break;
                case TooltipAnchor.RightCenter: target = (corners[2] + corners[3]) / 2f; break;
                case TooltipAnchor.RightTop: target = corners[2]; break;
                case TooltipAnchor.RightBottom: target = corners[3]; break;
            }
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(target, 0.1f);
        }

        /// <summary>
        /// Instantly adds a tooltip to any UI GameObject at runtime via code.
        /// </summary>
        /// <param name="target">The UI GameObject that will trigger the tooltip when hovered.</param>
        /// <param name="content">The main body text of the tooltip.</param>
        /// <param name="title">Optional: The header text.</param>
        /// <param name="icon">Optional: The sprite to display next to the title.</param>
        /// <returns>The created TooltipTrigger component.</returns>
        public static TooltipTrigger AddTooltip(GameObject target, string content, string title = "", Sprite icon = null) {
            if (target == null) return null;
            EnsureManagerExists();

            TooltipTrigger trigger = target.GetComponent<TooltipTrigger>() ?? target.AddComponent<TooltipTrigger>();
            trigger.Content = content;
            trigger.Title = title;
            trigger.Icon = icon;
            return trigger;
        }

        /// <summary>
        /// Instantly adds a multi-block tooltip to any UI GameObject at runtime via code.
        /// </summary>
        /// <param name="target">The UI GameObject that will trigger the tooltip when hovered.</param>
        /// <param name="textBlocks">A list of strings. The system will automatically inject separators between these blocks.</param>
        /// <param name="title">Optional: The header text.</param>
        /// <param name="icon">Optional: The sprite to display next to the title.</param>
        /// <returns>The created TooltipTrigger component.</returns>
        public static TooltipTrigger AddTooltip(GameObject target, List<string> textBlocks, string title = "", Sprite icon = null) {
            if (target == null) return null;
            EnsureManagerExists();

            TooltipTrigger trigger = target.GetComponent<TooltipTrigger>() ?? target.AddComponent<TooltipTrigger>();

            if (textBlocks != null && textBlocks.Count > 0) {
                trigger.Content = textBlocks[0];
                if (textBlocks.Count > 1) {
                    trigger.SecondaryContent = textBlocks.GetRange(1, textBlocks.Count - 1);
                }
            }

            trigger.Title = title;
            trigger.Icon = icon;
            return trigger;
        }

        private static void EnsureManagerExists() {
            if (TooltipManager.Instance != null || FindAnyObjectByType<TooltipManager>() != null) return;
            GameObject managerPrefab = Resources.Load<GameObject>("TooltipManager");
            if (managerPrefab != null) {
                GameObject managerInstance = Instantiate(managerPrefab);
                managerInstance.name = "TooltipManager (Auto-Generated)";
            }
        }
        #endregion
    }
}