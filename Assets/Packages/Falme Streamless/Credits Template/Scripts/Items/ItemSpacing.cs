using UnityEngine.UI;

namespace FalmeStreamless.Credits
{
    public class ItemSpacing : CreditsItem
    {
        private LayoutElement layoutElement;

        protected override void Awake()
        {
            base.Awake();
            layoutElement = GetComponent<LayoutElement>();
        }

		public override void Initialize(CreditsItemData data)
		{
			SetHeight(data.height);
		}

        public void SetHeight(float newHeight)
        {
            layoutElement.preferredHeight = newHeight;
        }
    }
}
