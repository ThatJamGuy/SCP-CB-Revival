using UnityEngine;
using TMPro;
using System.Text;

namespace FalmeStreamless.Credits
{
    public class ItemActor : CreditsItem
    {
        private TextMeshProUGUI label;

        protected override void Awake()
        {
            base.Awake();
            label = GetComponent<TextMeshProUGUI>();
        }

		public override void Initialize(CreditsItemData data)
		{
			StringBuilder builder = new StringBuilder();

			for(int a=0; a<data.actors.Length; a++)
				builder.AppendLine(data.actors[a]);

			SetText(builder.ToString());
		}

        public void SetText(string newText)
        {
            label.text = newText;
        }

        public Vector2 BottomItemPosition()
        {
            return (Vector2)rectTransform.position - rectTransform.sizeDelta;
        }
    }
}
