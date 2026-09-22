using TMPro;

namespace FalmeStreamless.Credits
{
    public class ItemCategory : CreditsItem
    {
        private TextMeshProUGUI label;

        protected override void Awake()
        {
            base.Awake();
            label = GetComponent<TextMeshProUGUI>();
        }

        public override void Initialize(CreditsItemData category)
        {
            SetText(category.text);
        }

        private void SetText(string newText)
        {
            label.text = newText;
        }
    }
}
