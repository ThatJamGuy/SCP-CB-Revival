using System.Threading.Tasks;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Events;

namespace vectorarts.scpcbr {
    public class MapGenerator : MonoBehaviour {
        [Header("Core Settings")]
        public string mapSeed;
        public Zone[] zones;

        [Header("Modules")]
        [SerializeField] private GridCreationModule gridModule;
        [SerializeField] private SeedCreationModule seedModule;
        [SerializeField] private RoomPlacementModule roomModule;
        [SerializeField] private RoomInstantiationModule instantiationModule;

        [Header("Generation Options")]
        [SerializeField] private bool instantiateRoomsOnGeneration = true;

        [Header("Events")]
        public UnityEvent OnMapFinishedGenerating;

        private const string GENERATE_ON_LOAD_KEY = "GenerateMapOnLoad";

        private void Awake() {
            InitializeModules();
        }

        private async void Start() {
            if (PlayerPrefs.GetInt(GENERATE_ON_LOAD_KEY, 0) == 1)
            {
                PlayerPrefs.DeleteKey(GENERATE_ON_LOAD_KEY);
                await Task.Yield();
                GenerateMap();
            }
        }

        private void InitializeModules() {
            gridModule.Initialize(this);
            seedModule.Initialize(this);
            roomModule.Initialize(this, gridModule);
            instantiationModule.Initialize(this, gridModule, roomModule);
        }

        [Button("Generate Map")]
        public async void GenerateMap() {
            InitializeModules();
            PrepareSeed();

            roomModule.GenerateRooms();

            if (instantiateRoomsOnGeneration) {
                await InstantiateRooms();
                await InstantiateDoors();
            }

            OnMapFinishedGenerating?.Invoke();
        }

        [Button("Instantiate Rooms")]
        public async Task InstantiateRooms() {
            await instantiationModule.InstantiateAllRooms();
            OnMapFinishedGenerating?.Invoke();
        }

        [Button("Instantiate Doors")]
        public async Task InstantiateDoors() {
            await instantiationModule.InstantiateAllDoors();
        }

        private void PrepareSeed() {
            string seed = seedModule.GetOrCreateSeed();
            Debug.Log($"Using seed: {seed}");

            int seedValue = seedModule.ConvertSeedToInt(seed);
            Random.InitState(seedValue);
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