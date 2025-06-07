using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

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
        public Dictionary<int, int> zoneStartY { get; private set; } = new();

        public void Initialize(MapGenerator generator) {
            this.generator = generator;
            GenerateGlobalGrid();
        }

        private void GenerateGlobalGrid() {
            globalGrid.Clear();
            zoneCells.Clear();
            zoneStartY.Clear();

            if (generator.zones?.Length == 0) return;

            int currentY = 0;

            for (int z = 0; z < generator.zones.Length; z++)
            {
                var zone = generator.zones[z];
                zoneStartY[zone.zoneID] = currentY;
                var cells = new List<Cell>();
                zoneCells[zone.zoneID] = cells;

                // Main zone area
                for (int y = 0; y < perZoneGridHeight; y++)
                {
                    for (int x = 0; x < perZoneGridWidth; x++)
                    {
                        Vector2Int pos = new(x, currentY + y);
                        var cell = new Cell(pos, zone.zoneID);
                        globalGrid[pos] = cell;
                        cells.Add(cell);
                    }
                }
                currentY += perZoneGridHeight;

                // Connector area (except for last zone)
                if (z < generator.zones.Length - 1 || zone.surfaceExitRooms?.Length > 0)
                {
                    for (int x = 0; x < perZoneGridWidth; x++)
                    {
                        Vector2Int pos = new(x, currentY);
                        var connectorCell = new Cell(pos, zone.zoneID, true);
                        globalGrid[pos] = connectorCell;
                        cells.Add(connectorCell);
                    }
                    currentY += 1;
                }
            }
        }

        public Cell GetCell(Vector2Int position) => globalGrid.TryGetValue(position, out var cell) ? cell : null;
        public List<Cell> GetCellsInZone(int zoneID) => zoneCells.TryGetValue(zoneID, out var list) ? list : null;

        public IEnumerable<Cell> GetAllCells() {
            return globalGrid.Values;
        }

        public Vector3 GetWorldPosition(Vector2Int gridPosition) {
            return generator.transform.position + new Vector3(gridPosition.x * cellSize, 0, gridPosition.y * cellSize);
        }

        public int GetZoneEndY(int zoneID) {
            if (!zoneStartY.TryGetValue(zoneID, out int startY))
            {
                Debug.LogError($"Zone {zoneID} not found in zoneStartY dictionary");
                return 0;
            }

            var zone = generator.zones.FirstOrDefault(z => z.zoneID == zoneID);
            if (zone == null)
            {
                Debug.LogError($"Zone {zoneID} not found in generator.zones");
                return 0;
            }

            int endY = startY + perZoneGridHeight;
            return endY;
        }

        public bool IsPositionWithinGrid(Vector2Int pos) {
            return pos.x >= 0 && pos.y >= 0 && pos.x < perZoneGridWidth && pos.y < perZoneGridHeight;
        }

        public void DrawGizmos(Vector3 origin) {
            if (!showGizmos || globalGrid.Count == 0) return;

            foreach (var (pos, cell) in globalGrid)
            {
                Vector3 worldPos = GetWorldPosition(pos);

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