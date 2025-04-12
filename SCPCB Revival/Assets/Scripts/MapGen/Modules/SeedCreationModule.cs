namespace vectorarts.scpcbr {
    [System.Serializable]
    public class SeedCreationModule {
        private MapGenerator generator;
        private const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public void Initialize(MapGenerator generator) => this.generator = generator;

        public string GetOrCreateSeed() => string.IsNullOrEmpty(generator.mapSeed)
            ? generator.mapSeed = GenerateRandomSeed(4)
            : generator.mapSeed;

        private string GenerateRandomSeed(int length) {
            var sb = new System.Text.StringBuilder(length);
            for (int i = 0; i < length; i++)
                sb.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
            return sb.ToString();
        }

        public int ConvertSeedToInt(string seed) {
            int hash = 0;
            if (string.IsNullOrEmpty(seed)) return hash;
            foreach (var c in seed)
                hash = (hash << 5) - hash + c;
            return hash;
        }
    }
}