using UnityEngine;
using NaughtyAttributes;
using System.Collections;

namespace vectorarts.scpcbr {
    public class MapGenerator : MonoBehaviour {
        public int mapSeed;
        public Zone[] zones;

        [Header("Grid Settings")]
        public int perZoneGridWidth;
        public int perZoneGridHeight;
        public float cellSize = 20.5f;
    }
}