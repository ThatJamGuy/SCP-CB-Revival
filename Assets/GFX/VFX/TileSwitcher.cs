using UnityEngine;

public class TileSwitcher : MonoBehaviour {
    [SerializeField] private Material targetMaterial;
    [SerializeField] private float switchInterval = 1.0f;
    [SerializeField] private int totalTiles;
    [SerializeField] private float tileSizeX;
    [SerializeField] private float tileSizeY;

    private float timer;
    private int currentTileIndex = 0;

    void Start() {
        if (targetMaterial == null)
            targetMaterial = GetComponent<Renderer>().material;

        targetMaterial.SetInt("_TileIndex", 0);
        targetMaterial.SetVector("_TileSize", new Vector4(tileSizeX, tileSizeY, 0, 0));
        targetMaterial.SetFloat("_TileCount", totalTiles);
    }

    void Update() {
        timer += Time.deltaTime;
        if (timer >= switchInterval) {
            timer = 0;
            currentTileIndex = (currentTileIndex + 1) % totalTiles;
            targetMaterial.SetFloat("_TileIndex", currentTileIndex);
        }
    }
}