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

    // SCP 173 range config: Closer and more frequent encounters
    private const float SPAWN_173_MIN_SQR = 15 * 15;
    private const float SPAWN_173_MAX_SQR = 50 * 50;

    // SCP 049 range config: Further and less frequent.
    // spawnRate should also be higher so he can freeroam more
    private const float SPAWN_049_MIN_SQR = 30 * 30;
    private const float SPAWN_049_MAX_SQR = 80 * 80;

    private float spawn049Timer;
    private float spawn173Timer;

    #region Unity Callbacks

    private void Awake() {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DebugLogConsole.AddCommand("print_closest_waypoint", "Prints the Vector3 location of the closest AI waypoint to the player into the console.", DebugPrintClosestWaypoint);
        DebugLogConsole.AddCommand<string>("spawn_entity", "Spawns an entity at the closest waypoint to the player.", DebugSpawnEntity);

        // Cache some entities for the spawning systems to prevent repeated foreach checks
        for (int i = 0; i < entities.Length; i++) {
            Entity entity = entities[i];
            if (entity.entityIdentifier == "scp173") cached173Entity = entity;
            else if (entity.entityIdentifier == "scp049") cached049Entity = entity;
        }
    }

    private void Start() {

        // Kept for testing reasons, map generator automatically calls this
        if (devFindWpOnStart)
            FindWaypointsInScene();
    }

    private void Update() {
        if (CanAutoSpawn(cached173Entity))
            HandleAutoSpawning(cached173Entity, ref spawn173Timer, SPAWN_173_MIN_SQR, SPAWN_173_MAX_SQR);

        if (CanAutoSpawn(cached049Entity))
            HandleAutoSpawning(cached049Entity, ref spawn049Timer, SPAWN_049_MIN_SQR, SPAWN_049_MAX_SQR);
    }

    #endregion

    #region Private Methods

    // Shared spawn/relocation loop for entities driven by a timer
    // Once the timer passes the spawnRate minus other difficulty factors, a random valid waypoint is picked and the entity is moved/spawned there
    // (IE. on Keter for 173 it would be 15 (def spawnRate) - 3 (Keter ODF) = 12 Final spawnRate)
    private void HandleAutoSpawning(Entity entity, ref float timer, float minSqr, float maxSqr) {
        timer += Time.deltaTime;
        if (timer < (entity.spawnRate - RevivalSessionEngine.otherDifficultyFactor)) return;

        timer = 0f;

        if (TryPickRandomWaypoint(minSqr, maxSqr, out Transform chosen))
            MoveEntity(entity, chosen);
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

    #region Private Extras

    private static bool CanAutoSpawn(Entity entity) {
        return entity != null && !entity.chasingPlayer && !entity.disabled;
    }

    // Picks a uniformly random waypoint whose squared distance to the player is within (minSqr, maxSqr)
    private bool TryPickRandomWaypoint(float minSqr, float maxSqr, out Transform chosen) {
        Vector3 playerPos = Player.Instance.transform.position;
        chosen = null;
        int candidates = 0;

        for (int i = 0; i < cachedWaypoints.Count; i++) {
            Transform wp = cachedWaypoints[i];
            float sqr = (wp.position - playerPos).sqrMagnitude;
            if (sqr <= minSqr || sqr >= maxSqr) continue;

            candidates++;
            if (Random.Range(0, candidates) == 0) chosen = wp;
        }

        return chosen != null;
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

    public static bool AnyEntityChasingPlayer() {
        var entities = EntitySystem.Instance.entities;
        for (int i = 0; i < entities.Length; i++) {
            if (entities[i].chasingPlayer) return true;
        }
        return false;
    }

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