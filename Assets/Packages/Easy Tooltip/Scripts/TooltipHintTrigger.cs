namespace PixeLadder.EasyTooltip {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using TMPro;
#if ENABLE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
#endif

    /// <summary>
    /// A standalone trigger for creating secondary, lightweight "Action Hints" (e.g., [Shift] Split Stack).
    /// Completely decoupled from the main TooltipManager to allow simultaneous dual-tooltips.
    /// Automatically manages its own static object pool to guarantee zero instantiation lag.
    /// </summary>
    [AddComponentMenu("PixeLadder/Easy Tooltip/Tooltip Hint Trigger")]
    public class TooltipHintTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        #region Fields
        [Header("Configuration")]
        [Tooltip("The UI prefab to spawn. Must contain a TextMeshProUGUI and CanvasGroup component.")]
        [SerializeField] private GameObject hintPrefab;

        [Tooltip("The text to display inside the hint.")]
        [SerializeField] private string hintText;

        [Tooltip("Pixel offset applied to the hint relative to the mouse cursor.")]
        [SerializeField] private Vector2 mouseOffset = new Vector2(0f, -30f);

        [Tooltip("Duration of the fade-in and fade-out animations in seconds.")]
        [SerializeField, Min(0f)] private float fadeDuration = 0.15f;

        // Internal State
        private GameObject activeHint;
        private RectTransform activeRect;
        private CanvasGroup activeCanvasGroup;
        private TextMeshProUGUI activeTextComponent;
        private Coroutine fadeCoroutine;
        private bool isHovering = false;

        // Static Object Pool
        private static readonly Dictionary<GameObject, List<GameObject>> hintPool = new Dictionary<GameObject, List<GameObject>>();
        #endregion

        #region Public Properties
        /// <summary>The text currently displayed in the hint.</summary>
        public string HintText { get => hintText; set => hintText = value; }
        #endregion

        #region Unity Lifecycle
        private void Update() {
            if (isHovering && activeHint != null && activeHint.activeInHierarchy) {
                PositionAtMouse();
                ClampToScreen();
                activeHint.transform.SetAsLastSibling();
            }
        }

        private void OnDisable() {
            if (isHovering) HideHint();
        }
        #endregion

        #region Editor Preview
#if UNITY_EDITOR
        public void EditorPreviewHint() {
            if (hintPrefab == null || string.IsNullOrEmpty(hintText)) return;

            EnsureHintReady();
            if (activeHint == null) return;

            if (!Application.isPlaying) activeHint.hideFlags = HideFlags.HideAndDontSave;

            if (activeTextComponent != null) activeTextComponent.text = hintText;

            activeHint.SetActive(true);
            activeHint.transform.SetAsLastSibling();
            activeRect.pivot = new Vector2(0, 1);

            // Force the Content Size Fitters to update instantly in Edit Mode
            for (int i = 0; i < 3; i++) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(activeRect);
            Canvas.ForceUpdateCanvases();

            RectTransform parentRect = activeHint.transform.parent as RectTransform;
            RectTransform myRect = GetComponent<RectTransform>();
            if (parentRect != null && myRect != null) {
                Vector3[] corners = new Vector3[4];
                myRect.GetWorldCorners(corners);

                Vector3 scaledOffset = new Vector3(mouseOffset.x / parentRect.lossyScale.x, mouseOffset.y / parentRect.lossyScale.y, 0);

                activeHint.transform.localPosition = parentRect.InverseTransformPoint(corners[0]) + scaledOffset;
            }

            if (activeCanvasGroup != null) activeCanvasGroup.alpha = 1f;
        }

        public void EditorHideHint() {
            if (!Application.isPlaying) {
                if (activeHint != null) DestroyImmediate(activeHint);
                if (hintPrefab != null && hintPool.ContainsKey(hintPrefab)) hintPool[hintPrefab].RemoveAll(item => item == null);
                activeHint = null;
            } else {
                HideHint();
            }
        }
#endif
        #endregion

        #region Interface Implementations
        public void OnPointerEnter(PointerEventData eventData) {
            if (hintPrefab == null || string.IsNullOrEmpty(hintText)) return;

            isHovering = true;
            EnsureHintReady();

            if (activeHint == null) return;

            if (activeTextComponent != null) activeTextComponent.text = hintText;

            activeHint.SetActive(true);
            activeHint.transform.SetAsLastSibling();

            // Force pivot to top-left for standard mouse tracking behavior
            activeRect.pivot = new Vector2(0, 1);

            PositionAtMouse();
            ClampToScreen();

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeRoutine(1f));
        }

        public void OnPointerExit(PointerEventData eventData) {
            HideHint();
        }
        #endregion

        #region Core Logic
        private void HideHint() {
            isHovering = false;
            if (activeHint != null && activeHint.activeInHierarchy) {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeRoutine(0f, true));
            }
        }

        private void EnsureHintReady() {
            if (activeHint != null) return;

            if (!hintPool.ContainsKey(hintPrefab)) {
                hintPool[hintPrefab] = new List<GameObject>();
            }

            List<GameObject> pool = hintPool[hintPrefab];

            // Search for an inactive hint in the pool
            foreach (GameObject pooledObj in pool) {
                if (pooledObj != null && !pooledObj.activeInHierarchy) {
                    activeHint = pooledObj;
                    CacheComponents();
                    return;
                }
            }

            // If pool is empty or all are active, instantiate a new one
            Canvas targetCanvas = GetComponentInParent<Canvas>();
            if (targetCanvas != null) targetCanvas = targetCanvas.rootCanvas;
            else targetCanvas = FindAnyObjectByType<Canvas>();

            if (targetCanvas == null) return;

            activeHint = Instantiate(hintPrefab, targetCanvas.transform, false);
            pool.Add(activeHint);

            CacheComponents();
        }

        private void CacheComponents() {
            activeRect = activeHint.GetComponent<RectTransform>();
            activeCanvasGroup = activeHint.GetComponent<CanvasGroup>();
            if (activeCanvasGroup == null) activeCanvasGroup = activeHint.AddComponent<CanvasGroup>();
            activeTextComponent = activeHint.GetComponentInChildren<TextMeshProUGUI>();
        }
        #endregion

        #region Coroutines
        private IEnumerator FadeRoutine(float targetAlpha, bool disableOnComplete = false) {
            if (activeCanvasGroup == null) yield break;

            float startAlpha = activeCanvasGroup.alpha;
            float start = Time.unscaledTime;

            while (Time.unscaledTime < start + fadeDuration) {
                activeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, (Time.unscaledTime - start) / fadeDuration);
                yield return null;
            }

            activeCanvasGroup.alpha = targetAlpha;

            if (disableOnComplete && activeHint != null) {
                activeHint.SetActive(false);
            }
        }
        #endregion

        #region Positioning
        private void PositionAtMouse() {
#if ENABLE_INPUT_SYSTEM
            Vector2 screenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
            Vector2 screenPos = Input.mousePosition;
#endif
            screenPos += mouseOffset;

            Canvas rootCanvas = activeHint.GetComponentInParent<Canvas>();
            Camera uiCamera = (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : rootCanvas.worldCamera;
            RectTransform parentRect = activeHint.transform.parent as RectTransform;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, uiCamera, out Vector2 localPoint)) {
                activeHint.transform.localPosition = new Vector3(localPoint.x, localPoint.y, 0);
            }
        }

        private void ClampToScreen() {
            RectTransform parentRect = activeHint.transform.parent as RectTransform;
            if (parentRect == null) return;

            Vector3 currentLocalPos = activeHint.transform.localPosition;
            Rect parentBounds = parentRect.rect;
            Rect hintBounds = activeRect.rect;
            Vector3 scale = activeHint.transform.localScale;

            float scaledWidth = hintBounds.width * scale.x;
            float scaledHeight = hintBounds.height * scale.y;

            float pivotX = activeRect.pivot.x;
            float pivotY = activeRect.pivot.y;

            float left = currentLocalPos.x - (scaledWidth * pivotX);
            float right = currentLocalPos.x + (scaledWidth * (1f - pivotX));
            float bottom = currentLocalPos.y - (scaledHeight * pivotY);
            float top = currentLocalPos.y + (scaledHeight * (1f - pivotY));

            if (right > parentBounds.xMax) currentLocalPos.x -= (right - parentBounds.xMax);
            else if (left < parentBounds.xMin) currentLocalPos.x += (parentBounds.xMin - left);

            if (top > parentBounds.yMax) currentLocalPos.y -= (top - parentBounds.yMax);
            else if (bottom < parentBounds.yMin) currentLocalPos.y += (parentBounds.yMin - bottom);

            activeHint.transform.localPosition = currentLocalPos;
        }
        #endregion

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() {
            hintPool.Clear();
        }
    }
}