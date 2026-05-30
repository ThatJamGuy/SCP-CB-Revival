namespace PixeLadder.EasyTooltip
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;

    [ExecuteAlways]
    [AddComponentMenu("PixeLadder/Easy Tooltip/Tooltip Trigger")]
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Fields
        [Header("Content")]
        [SerializeField] private string title;
        [TextArea(3, 10)]
        [SerializeField] private string content;
        [SerializeField] private Sprite icon;

        // -- Overrides managed by Custom Editor --
        [SerializeField] private bool overrideStyle = false;
        [SerializeField] private Color titleColor = Color.white;
        [SerializeField] private Color iconColor = Color.white;
        [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        [SerializeField] private bool showOutline = true;
        [SerializeField] private Color outlineColor = Color.white;

        [SerializeField] private bool overrideLayout = false;
        [SerializeField] private TooltipPositionMode positionMode = TooltipPositionMode.FollowMouse;
        [SerializeField] private TooltipAnchor anchorPosition = TooltipAnchor.TopCenter;
        [SerializeField] private Vector2 additionalOffset = Vector2.zero;

        [SerializeField] private bool overrideSize = false;
        [SerializeField, Min(50f)] private float maxWidth = 350f;

        [SerializeField] private bool overrideTimer = false;
        [SerializeField, Min(0f)] private float hoverDelay = 0.5f;

        [Header("Events")]
        public UnityEvent onTooltipShow;
        public UnityEvent onTooltipHide;
        #endregion

        #region Public Properties
        // Data
        public string Title { get => title; set => title = value; }
        public string Content { get => content; set => content = value; }
        public Sprite Icon { get => icon; set => icon = value; }

        // Styles (Setters auto-enable overrides)
        public Color TitleColor
        {
            get => (overrideStyle || TooltipManager.Instance == null) ? titleColor : TooltipManager.Instance.defaultTitleColor;
            set { titleColor = value; overrideStyle = true; }
        }
        public Color IconColor
        {
            get => (overrideStyle || TooltipManager.Instance == null) ? iconColor : TooltipManager.Instance.defaultIconColor;
            set { iconColor = value; overrideStyle = true; }
        }
        public Color BackgroundColor
        {
            get => (overrideStyle || TooltipManager.Instance == null) ? backgroundColor : TooltipManager.Instance.defaultBackgroundColor;
            set { backgroundColor = value; overrideStyle = true; }
        }
        public Color OutlineColor
        {
            get => (overrideStyle || TooltipManager.Instance == null) ? outlineColor : TooltipManager.Instance.defaultOutlineColor;
            set { outlineColor = value; overrideStyle = true; }
        }
        public bool ShowOutline
        {
            get => (overrideStyle || TooltipManager.Instance == null) ? showOutline : TooltipManager.Instance.defaultShowOutline;
            set { showOutline = value; overrideStyle = true; }
        }

        // Layout (Setters auto-enable overrides)
        public TooltipPositionMode PositionMode
        {
            get => positionMode;
            set { positionMode = value; overrideLayout = true; }
        }
        public TooltipAnchor AnchorPosition
        {
            get => anchorPosition;
            set { anchorPosition = value; overrideLayout = true; }
        }
        public Vector2 AdditionalOffset
        {
            get => additionalOffset;
            set { additionalOffset = value; overrideLayout = true; }
        }

        // Timer & Size (Setters auto-enable overrides)
        public float HoverDelay
        {
            get => (overrideTimer || TooltipManager.Instance == null) ? hoverDelay : TooltipManager.Instance.defaultHoverDelay;
            set { hoverDelay = value; overrideTimer = true; }
        }
        public float MaxWidth
        {
            get => (overrideSize || TooltipManager.Instance == null) ? maxWidth : TooltipManager.Instance.DefaultMaxWidth;
            set { maxWidth = value; overrideSize = true; }
        }
        #endregion

        #region Lifecycle
        private void Reset() => EnsureManagerExists();
        private void OnEnable() { if (Application.isPlaying) EnsureManagerExists(); }
        #endregion

        #region Interface Implementations
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (TooltipManager.Instance == null) return;

            // Resolve Values
            var finalMode = overrideLayout ? positionMode : TooltipPositionMode.FollowMouse;
            var finalAnchor = overrideLayout ? anchorPosition : TooltipAnchor.TopCenter;
            var finalOffset = overrideLayout ? additionalOffset : Vector2.zero;
            float? finalWidth = overrideSize ? maxWidth : null;

            // We pass 'this' so the Manager can call the Events at the correct time
            TooltipManager.Instance.ShowTooltip(
                content, title, icon,
                TitleColor, IconColor, BackgroundColor, OutlineColor, ShowOutline,
                HoverDelay,
                this,
                finalMode, finalAnchor, finalOffset, finalWidth
            );
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.HideTooltip();
            }
        }
        #endregion

        #region Gizmos & Helpers
        private void OnDrawGizmosSelected()
        {
            if (!overrideLayout || positionMode != TooltipPositionMode.Fixed) return;
            RectTransform rect = GetComponent<RectTransform>();
            if (rect == null) return;

            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector3 target = Vector3.zero;

            switch (anchorPosition)
            {
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
            Gizmos.DrawWireSphere(target, 5f);
        }

        public static TooltipTrigger AddTooltip(GameObject target, string content, string title = "", Sprite icon = null)
        {
            if (target == null) return null;
            EnsureManagerExists();
            TooltipTrigger trigger = target.GetComponent<TooltipTrigger>() ?? target.AddComponent<TooltipTrigger>();
            trigger.Content = content;
            trigger.Title = title;
            trigger.Icon = icon;
            return trigger;
        }

        private static void EnsureManagerExists()
        {
            if (TooltipManager.Instance != null || FindFirstObjectByType<TooltipManager>() != null) return;
            GameObject managerPrefab = Resources.Load<GameObject>("TooltipManager");
            if (managerPrefab != null)
            {
                GameObject managerInstance = Instantiate(managerPrefab);
                managerInstance.name = "TooltipManager (Auto-Generated)";
            }
        }
        #endregion
    }
}