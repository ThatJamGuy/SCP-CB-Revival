using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace vectorarts.scpcbr {
    [System.Serializable]
    public class GridCreationModule {
        [Header("Grid Settings")]
        public int perZoneGridWidth = 10;
        public int perZoneGridHeight = 10;
        public float cellSize = 20.5f;

        [Header("Debug")]
        public bool showGizmos = true;
        public bool showLabels = false;

        private MapGenerator generator;
        private readonly Dictionary<Vector2Int, Cell> globalGrid = new();
        private readonly Dictionary<int, List<Cell>> zoneCells = new();

        public void Initialize(MapGenerator generator) {
            this.generator = generator;
            GenerateGlobalGrid();
        }

        private void GenerateGlobalGrid() {
            globalGrid.Clear();
            zoneCells.Clear();

            if (generator.zones?.Length == 0) return;

            int currentY = 0;

            foreach (var zone in generator.zones) {
                var cells = new List<Cell>();
                zoneCells[zone.zoneID] = cells;

                for (int y = 0; y < perZoneGridHeight; y++) {
                    for (int x = 0; x < perZoneGridWidth; x++) {
                        Vector2Int pos = new(x, currentY + y);
                        var cell = new Cell(pos, zone.zoneID);
                        globalGrid[pos] = cell;
                        cells.Add(cell);
                    }
                }

                currentY += perZoneGridHeight;

                if (zone.connectsToNextZone || (zone.surfaceExitRooms?.Length ?? 0) > 0) {
                    for (int x = 0; x < perZoneGridWidth; x++) {
                        Vector2Int pos = new(x, currentY);
                        globalGrid[pos] = new Cell(pos, -1, true);
                    }
                    currentY += 1;
                }
            }
        }

        public Cell GetCell(Vector2Int position) => globalGrid.TryGetValue(position, out var cell) ? cell : null;
        public List<Cell> GetCellsInZone(int zoneID) => zoneCells.TryGetValue(zoneID, out var list) ? list : null;

        public void DrawGizmos(Vector3 origin) {
            if (!showGizmos || globalGrid.Count == 0) return;

            foreach (var (pos, cell) in globalGrid) {
                Vector3 worldPos = origin + new Vector3(pos.x * cellSize, 0, pos.y * cellSize);

                Gizmos.color = cell.isConnector ? Color.gray :
                               cell.occupied ? Color.red :
                               Color.HSVToRGB((cell.zoneID * 0.1f) % 1f, 0.7f, 1f);

                Gizmos.DrawWireCube(worldPos, Vector3.one * (cellSize * 0.9f));

#if UNITY_EDITOR
                if (showLabels)
                    Handles.Label(worldPos + Vector3.up * 0.5f, $"Z{cell.zoneID}\n({pos.x},{pos.y})");
#endif
            }
        }

        [System.Serializable]
        public class Cell {
            public Vector2Int position;
            public int zoneID;
            public bool occupied;
            public bool isConnector;

            public Cell(Vector2Int position, int zoneID, bool isConnector = false) {
                this.position = position;
                this.zoneID = zoneID;
                this.isConnector = isConnector;
                this.occupied = false;
            }
        }
    }
}