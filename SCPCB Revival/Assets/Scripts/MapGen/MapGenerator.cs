using UnityEngine;
using NaughtyAttributes;
using System.Collections;

namespace vectorarts.scpcbr {
    public class MapGenerator : MonoBehaviour {
        [Header("Core Settings")]
        public string mapSeed;
        public Zone[] zones;

        [Header("Modules")]
        [SerializeField] private GridCreationModule gridModule;
        [SerializeField] private SeedCreationModule seedModule;

        private void Awake() {
            gridModule.Initialize(this);
            seedModule.Initialize(this);
        }

        [Button("Generate Map")]
        public void GenerateMap() {
            PrepareSeed();
        }

        private void PrepareSeed() {
            string seed = seedModule.GetOrCreateSeed();
            Debug.Log($"Using seed: {seed}");

            int seedValue = seedModule.ConvertSeedToInt(seed);
            UnityEngine.Random.InitState(seedValue);
        }

        private void OnDrawGizmos() {
            if (gridModule != null)
            {
                Vector3 origin = transform.position;
                gridModule.DrawGizmos(origin);
            }
        }
    }
}