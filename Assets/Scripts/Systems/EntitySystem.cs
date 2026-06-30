using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntitySystem : MonoBehaviour {
    public static EntitySystem Instance { get; private set; }

    public List<Transform> waypointList;
    private List<IRoamingSCP> activeEntityList = new List<IRoamingSCP>();

    [SerializeField] private GameObject[] standbyEntityList;

    #region Unity Callbacks

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DebugConsole.AddCommand("listentities", "Lists all active entities and their indexes in the standy array.", ListEntities);
        DebugConsole.AddCommand<int, Vector3>("spawnentity", "Spawns the NPC prefab with the given index in the active array at the given position.", SpawnEntity);
        DebugConsole.AddCommand<int, Vector3>("teleportentity", "Teleports an entity by index in the active array to a given position", TeleportEntity);
        DebugConsole.AddCommand<int>("walktome", "Tells an NPC by index in the active array to walk to the player", WalkToMe);
    }

    private void Start() {
        // For testing do this at start, but when map-gen is in then call this after the map has been generated
        InitWaypoints();
    }
    #endregion

    #region Private Methods

    private void InitWaypoints() {
        waypointList = GameObject.FindGameObjectsWithTag("Finish").Select(go => go.transform).ToList();
    }

    private void ListEntities() {
        if (activeEntityList.Count == 0) {
            Debug.Log("No active entities.");
            return;
        }

        var sb = new System.Text.StringBuilder("Active Entities:\n");
        for (int i = 0; i < activeEntityList.Count; i++)
            sb.AppendLine($"  [{i}] {activeEntityList[i].GetType().Name}");

        Debug.Log(sb.ToString());
    }

    private void WalkToMe(int index) {
        if (index < 0 || index >= activeEntityList.Count) {
            Debug.LogWarning($"No entity at index {index}.");
            return;
        }
        activeEntityList[index].WalkTo(Player.Instance.transform.position);
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// Spawn a new instance of an entity from the standbyEntityList
    /// </summary>
    /// <param name="prefabIndex">Index of the prefab to spawn</param>
    /// <param name="position">Position to spawn the entity</param>
    public void SpawnEntity(int prefabIndex, Vector3 position) {
        if (prefabIndex < 0 || prefabIndex >= standbyEntityList.Length) {
            Debug.LogWarning($"No NPC prefab at index {prefabIndex}.");
            return;
        }

        Instantiate(standbyEntityList[prefabIndex], position, Quaternion.identity);
    }

    /// <summary>
    /// Teleport an entity to a new location for events and/or debugging
    /// </summary>
    /// <param name="index">ID of the entity to be teleported</param>
    /// <param name="position">Position to teleport the entity to</param>
    public void TeleportEntity(int index, Vector3 position) {
        if (index < 0 || index >= activeEntityList.Count) {
            Debug.LogWarning($"No entity at index {index}.");
            return;
        }
        activeEntityList[index].Teleport(position);
    }

    /// <summary>
    /// Register an entity to the Active Entity list so it can be tracked
    /// </summary>
    /// <param name="entity">The IRoamingSCP to add to the list</param>
    public void RegisterEntity(IRoamingSCP entity) {
        activeEntityList.Add(entity);
    }
    #endregion
}