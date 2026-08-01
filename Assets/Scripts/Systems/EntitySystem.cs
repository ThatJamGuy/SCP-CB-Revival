using EditorAttributes;
using IngameDebugConsole;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Entity {
    public string entityIdentifier; // Console identifier, ie. scp173
    public GameObject entityPrefab; // GameObject prefab for the entity
    public bool disabled; // Toggle for if the entity was disabled via console commands or something
    [ReadOnly] public int activeInstances = 0; // Int instead of bool in case player decides to do some funny business in the console, so just check for more than zero isntead
}

public class EntitySystem : MonoBehaviour {
    public static EntitySystem Instance { get; private set; }

    [SerializeField] private bool devFindWpOnStart;

    [SerializeField] private Entity[] entities;

    [ReadOnly] public List<Transform> cachedWaypoints = new List<Transform>();

    #region Unity Callbacks

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DebugLogConsole.AddCommand("print_closest_waypoint", "Prints the Vector3 location of the closest AI waypoint to the player into the console.", DebugPrintClosestWaypoint);
        DebugLogConsole.AddCommand<string>("spawn_entity", "Spawns an entity at the closest waypoint to the player.", DebugSpawnEntity);
    }

    private void Start() {

        // Will later probably manually call this from the map generator for ordering reasons
        if (devFindWpOnStart)
            FindWaypointsInScene();
    }

    #endregion

    #region Private Methods

    private void DebugPrintClosestWaypoint() {
        Debug.Log(GetClosestWaypointToPlayer().position);
    }

    private void DebugSpawnEntity(string entityIdentifier) {
        SpawnEntity(entityIdentifier, GetClosestWaypointToPlayer());
    }

    #endregion

    #region Public Methods

    public void FindWaypointsInScene() {
        GameObject[] waypointObjs = GameObject.FindGameObjectsWithTag("Respawn");

        foreach (GameObject waypointObj in waypointObjs) {
            cachedWaypoints.Add(waypointObj.transform);
        }
    }

    public void SpawnEntity(string entityIdentifier, Transform transform) {
        foreach (Entity entity in entities) {
            if (entity.entityIdentifier == entityIdentifier) {
                Instantiate(entity.entityPrefab, transform.position, transform.rotation);
                entity.activeInstances++;
            }
        }
    }

    #endregion

    #region Public Utilities

    public Transform GetClosestWaypointToPlayer() {
        Transform contendingWaypoint = null;
        Transform closestWaypoint = null;
        float contendingDistance = -1;
        float currentDistance = -1;

        // For each cached waypoint get it's distance to the player.
        // If contendingDistance is -1 set contendingDistance to the determined distance and contendeingWaypoint to that waypoint
        foreach (Transform waypoint in cachedWaypoints) {
            currentDistance = Vector3.Distance(waypoint.position, Player.Instance.transform.position);

            if (currentDistance < contendingDistance || contendingDistance == -1) {
                contendingDistance = currentDistance;
                contendingWaypoint = waypoint;
            }
        }

        closestWaypoint = contendingWaypoint;

        return closestWaypoint;
    }

    #endregion
}