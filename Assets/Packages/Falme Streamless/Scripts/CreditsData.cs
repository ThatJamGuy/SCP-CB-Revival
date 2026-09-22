namespace FalmeStreamless.Credits
{
    [System.Serializable]
    public class CreditsData
    {
        public string version;
        public float velocity;
        public CreditsItemData[] items;
    }

    [System.Serializable]
    public class CreditsItemData
    {
        public string type;
        public string path;
        public string text;
        public float height;
        public float categorySpacing;
        public float actorsSpacing;
        public string[] actors;
    }
}
