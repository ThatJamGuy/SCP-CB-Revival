using UnityEngine;
using UnityEngine.UI;

namespace FalmeStreamless.Credits
{
    public class ItemImage : CreditsItem
    {
        private Image image;
        private LayoutElement layoutElement;
		private Sprite failsafeImage;

        protected override void Awake()
        {
            base.Awake();
            image = GetComponent<Image>();
            layoutElement = GetComponent<LayoutElement>();
			this.failsafeImage = image.sprite;
        }

        public override void Initialize(CreditsItemData image)
        {
            SetImage(image.path);
            SetHeight(image.height);
        }

        public void SetImage(string path)
        {
            string streamingPath = System.IO.Path.Combine(Application.streamingAssetsPath, path);

            try
            {
                byte[] pngBytes = System.IO.File.ReadAllBytes(streamingPath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(pngBytes);
                Sprite fromTex = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                image.sprite = fromTex;
            }
            catch (System.Exception)
            {
                string result = string.Format("Credits Template: Not possible to read image at {0}", streamingPath);
				image.sprite = this.failsafeImage;
                Debug.LogError(result);
            }
        }

        public void SetHeight(float height)
        {
            if (height < 0) height = 0;
            layoutElement.preferredHeight = height;
        }
    }
}
