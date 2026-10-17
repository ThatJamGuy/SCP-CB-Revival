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
    public bool chasingPlayer; // Toggle for if the entity is chasing the player.
    public float spawnRate; // How many seconds until the initial spawn / relocation
    [ReadOnly] public int activeInstances = 0; // Int instead of bool in case player decides to do some funny business in the console, so just check for more than zero isntead
    [ReadOnly] public GameObject latestActiveInstance; // The latest active instance. Used to get the entity when requesting it for scripted sequences and stuff
    [ReadOnly] public NavMeshAgent latestAgent;
}

public class EntitySystem : MonoBehaviour {
    public static EntitySystem Instance { get; private set; }

    [SerializeField] private bool devFindWpOnStart;
    public Entity[] entities;
    [ReadOnly] public List<Transform> cachedWaypoints = new List<Transform>();
    [ReadOnly] public Entity cached173Entity;
    [ReadOnly] public Entity cached049Entity;

    private const float SPAWN_MIN_SQR = 15 * 15;
    private const float SPAWN_MAX_SQR = 50 * 50;

    private float spawn173Timer;

    #region Unity Callbacks

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DebugLogConsole.AddCommand("print_closest_waypoint", "Prints the Vector3 location of the closest AI waypoint to the player into the console.", DebugPrintClosestWaypoint);
        DebugLogConsole.AddCommand<string>("spawn_entity", "Spawns an entity at the closest waypoint to the player.", DebugSpawnEntity);

        // Cache some entities for the spawning systems to prevent repeated foreach checks
        foreach (Entity entity in entities) {
            if (entity.entityIdentifier == "scp173") {
                cached173Entity = entity;
            }
            if (entity.entityIdentifier == "scp049") {
                cached049Entity = entity;
            }
        }
    }

    private void Start() {

        // Kept for testing reasons, map generator automatically calls this
        if (devFindWpOnStart)
            FindWaypointsInScene();
    }

    private void Update() {
        if (cached173Entity != null && !cached173Entity.chasingPlayer && !cached173Entity.disabled) {
            Entity173Spawner();
        }
    }

    #endregion

    #region Private Methods

    private void Entity173Spawner() {
        spawn173Timer += Time.deltaTime;

        // Handle the finding of valid waypoints for 173 spawning if the timer reaches over the spawnRate minus other difficulty factors
        // (IE. on Keter for 173 it would be 15 (def spawnRate) - 3 (Keter ODF) = 12 Final spawnRate)
        if (spawn173Timer < (cached173Entity.spawnRate - RevivalSessionEngine.otherDifficultyFactor)) return;

        spawn173Timer = 0f;

        Vector3 playerPos = Player.Instance.transform.position;
        Transform chosen = null;
        int candidates = 0;

        // Compare the distance of every waypoint to the player
        // If it's min 10m and max 30m from the player it's a valid candidate for spawning
        for (int i = 0; i < cachedWaypoints.Count; i++) {
            Transform wp = cachedWaypoints[i];
            float sqr = (wp.position - playerPos).sqrMagnitude;
            if (sqr <= SPAWN_MIN_SQR || sqr >= SPAWN_MAX_SQR) continue;

            candidates++;
            if (Random.Range(0, candidates) == 0) chosen = wp;
        }

        // Move/spawn 173 at the chosen waypoint
        if (chosen != null) MoveEntity(cached173Entity, chosen);
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

    // Spawn an entity passing in a string id and spawnpoint transform
    public void SpawnEntity(string entityIdentifier, Transform spawnPoint) {
        for (int i = 0; i < entities.Length; i++) {
            if (entities[i].entityIdentifier == entityIdentifier) {
                SpawnEntity(entities[i], spawnPoint);
                return;
            }
        }
    }

    // Move an entity passing in a string id and spawnpoint transform
    public void MoveEntity(string entityIdentifier, Transform spawnPoint) {
        for (int i = 0; i < entities.Length; i++) {
            if (entities[i].entityIdentifier == entityIdentifier) {
                MoveEntity(entities[i], spawnPoint);
                return;
            }
        }
    }

    // Move an entity passing in an entity reference and spawnpoint transform
    public void MoveEntity(Entity entity, Transform spawnPoint) {
        if (entity.chasingPlayer || entity.disabled) return;

        if (entity.latestActiveInstance != null) {
            if (entity.latestAgent != null) entity.latestAgent.Warp(spawnPoint.position);
        } else SpawnEntity(entity, spawnPoint);
    }

    // Call this from any entity that get's destroyed, just to keep things clean
    // Removes it from it's active instance list and set's the "latest" vars to null
    public void UnregisterEntity(Entity entity, GameObject instance) {
        if (entity.activeInstances > 0) entity.activeInstances--;
        if (entity.latestActiveInstance == instance) {
            entity.latestActiveInstance = null;
            entity.latestAgent = null;
        }
    }

    #endregion

    #region Public Extras

    // Moved this to an extra helper thing for a cleaner reusable setup
    public GameObject SpawnEntity(Entity entity, Transform spawnPoint) {
        GameObject instance = Instantiate(entity.entityPrefab, spawnPoint.position, spawnPoint.rotation);
        entity.activeInstances++;
        entity.latestActiveInstance = instance;
        entity.latestAgent = instance.GetComponent<NavMeshAgent>();
        return instance;
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