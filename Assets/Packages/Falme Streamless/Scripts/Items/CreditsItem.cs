using UnityEngine;

namespace FalmeStreamless.Credits
{
    public abstract class CreditsItem : MonoBehaviour
    {
        protected RectTransform rectTransform;
        protected Pool pool;
        protected float lastYPosition;
		protected string id;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        protected virtual void Update()
        {
            if (hasPassedTopBorder())
                this.pool.Release(id, this);

            lastYPosition = rectTransform.position.y;
        }

		public virtual void Initialize(CreditsItemData data) { }

        public void SetPool(Pool pool)
        {
            this.pool = pool;
        }

		public void SetId(string id)
		{
			this.id = id;
		}

        public bool hasPassedTopBorder()
        {
            bool previousPositionBelowTopBorder =
                (lastYPosition - (GetHeight() / 2)) <= Screen.height;
            bool currentPositionAboveTopBorder =
                (rectTransform.position.y - (GetHeight() / 2)) > Screen.height;

            return previousPositionBelowTopBorder && currentPositionAboveTopBorder;
        }

		public bool hasPassedBottomBorder()
		{
            return (rectTransform.position.y - (GetHeight() / 2)) > 0;
		}

        public float GetHeight() => rectTransform.sizeDelta.y;
    }
}
