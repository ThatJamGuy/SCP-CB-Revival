using EditorAttributes;
using IngameDebugConsole;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Key:
// 173, not disabled, spawnrate 7

[System.Serializable]
public class Entity {
    public string entityIdentifier; // Console identifier, ie. scp173
    public GameObject entityPrefab; // GameObject prefab for the entity
    public bool disabled; // Toggle for if the entity was disabled via console commands or something
    public float spawnRate; // How many seconds until the initial spawn / relocation
    [ReadOnly] public int activeInstances = 0; // Int instead of bool in case player decides to do some funny business in the console, so just check for more than zero isntead
    [ReadOnly] public GameObject latestActiveInstance; // The latest active instance. Used to get the entity when requesting it for scripted sequences and stuff
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

        // Kept for testing reasons, map generator automatically calls this
        if (devFindWpOnStart)
            FindWaypointsInScene();
    }

    #endregion

    #region Private Methods

    private void Entity173Spawner() {
        // Every spawnRate seconds, relocate scp173 to a random waypoint at least 10m away from the player and max 20m from the player
        // If an scp173 instance doesn't exist, spawn one in at the determined location
    }

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

    public void MoveEntity(string entityIdentifier, Transform transform) {
        foreach (Entity entity in entities) {
            if (entity.entityIdentifier == entityIdentifier) {
                if (entity.activeInstances >= 1) {
                    entity.latestActiveInstance.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent);
                    agent.Warp(transform.position);
                } else {
                    SpawnEntity(entityIdentifier, transform);
                }
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