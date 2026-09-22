using UnityEngine;

namespace FalmeStreamless.Credits {
    public class Scroll : MonoBehaviour {
        [Header("References")]
        [SerializeField] private Staff staff;

        private RectTransform rectTransform;
        private float velocity;
        private bool isScrolling;

        private void Awake() {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable() {
            Pool.onRemovedItem += RemovedItem;
        }

        private void OnDisable() {
            Pool.onRemovedItem -= RemovedItem;
        }

        private void OnDestroy() {
            Pool.onRemovedItem -= RemovedItem;
        }

        private void Update() {
            if (isScrolling)
                ScrollCredits();
        }

        public void Initialize(Vector2 resolution, CreditsData data) {
            FillCreditsData(data);
            ScrollToStart(resolution);
            StartScrolling();
        }

        private void FillCreditsData(CreditsData data) {
            SetScrollVelocity(data.velocity);
            staff.Initialize(data);
        }

        public void ScrollToStart(Vector2 resolution) {
            rectTransform.anchoredPosition = new Vector2(0, -resolution.y);
        }

        public void ScrollCredits() {
            ScrollAdd(velocity * Time.deltaTime);
        }

        public void StartScrolling() {
            isScrolling = true;
        }

        public void StopScrolling() {
            isScrolling = false;
        }

        public void ScrollAdd(float y) {
            y += rectTransform.anchoredPosition.y;
            rectTransform.anchoredPosition = new Vector2(0, y);
        }

        public void SetScrollVelocity(float newVelocity) {
            velocity = newVelocity;
        }

        public void RemovedItem(float height) {
            ScrollAdd(-height);
        }
    }
}
