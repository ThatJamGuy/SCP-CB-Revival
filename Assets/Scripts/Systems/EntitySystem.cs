using EditorAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Entity {
    public string entityIdentifier; // Console identifier, ie. scp173
    public GameObject entityPrefab; // GameObject prefab for the entity
    public bool disabled; // Toggle for if the entity was disabled via console commands or something
    public bool chasingPlayer; // Toggle for if the entity is chasing the player.
    public float spawnRate; // How many seconds until the initial spawn / relocation
    [ReadOnly] public int activeInstances = 0; // Int instead of bool in case player decides to do some funny business in the console, so just check for more than zero isntead
    [ReadOnly] public GameObject latestActiveInstance; // The latest active instance. Used to get the entity when requesting it for scripted sequences and stuff
    [ReadOnly] public NavMeshAgent latestAgent; // Similar to lastActiveInstance but specifically for the navigation agent
}

// PURPOSE: Handle the spawning and management of entities that free roam randomly
// This will NOT be used for entities in later planned areas as they are not so random like these
public class EntitySystem : MonoBehaviour {
    public static EntitySystem Instance { get; private set; }

    public Entity[] entities;

    [Header("SCP Specific Settings")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private GameObject scp106_SpawnedEffect;

    [HideInInspector] public List<Transform> cachedWaypoints = new List<Transform>();
    [HideInInspector] public Entity cached173Entity;
    [HideInInspector] public Entity cached106Entity;
    [HideInInspector] public Entity cached049Entity;

    // SCP 173 range config: Closer and more frequent encounters
    private const float SPAWN_173_MIN_SQR = 20 * 20;
    private const float SPAWN_173_MAX_SQR = 60 * 60;

    // SCP 049 range config: Further and less frequent.
    // spawnRate should also be higher so he can freeroam more
    private const float SPAWN_049_MIN_SQR = 30 * 30;
    private const float SPAWN_049_MAX_SQR = 80 * 80;

    // SCP 106 everything config: Specific to this guy since he works very different
    // Mostly just copied and pasted from the old system rn, so may not integrate the best with new EntitySystem features
    private const float SPAWN_106_RADIUS = 3;
    private const float SPAWN_106_EFFECT_DURATION = 7;

    private GameObject pending106Effect;

    private Vector3 pending106Position;

    private float spawn049Timer;
    private float spawn106Timer;
    private float spawn106EffectTimer;
    private float spawn173Timer;

    private bool is106Spawning;

    #region Unity Lifecycle

    private void Awake() {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Cache some entities for the spawning systems to prevent repeated foreach checks
        for (int i = 0; i < entities.Length; i++) {
            Entity entity = entities[i];
            if (entity.entityIdentifier == "scp173") cached173Entity = entity;
            else if (entity.entityIdentifier == "scp106") cached106Entity = entity;
            else if (entity.entityIdentifier == "scp049") cached049Entity = entity;
        }
    }

    private void Update() {
        if (CanAutoSpawn(cached173Entity))
            HandleAutoSpawning(cached173Entity, ref spawn173Timer, SPAWN_173_MIN_SQR, SPAWN_173_MAX_SQR);

        if (CanAutoSpawn(cached049Entity))
            HandleAutoSpawning(cached049Entity, ref spawn049Timer, SPAWN_049_MIN_SQR, SPAWN_049_MAX_SQR);

        if (cached106Entity != null)
            Handle106Spawning(cached106Entity);
    }

    private void OnDestroy() {
        Cancel106Spawn();
    }

    #endregion

    #region Private Methods

    // Shared spawn/relocation loop for entities driven by a timer
    // Once the timer passes the spawnRate minus other difficulty factors, a random valid waypoint is picked and the entity is moved/spawned there
    // (IE. on Keter for 173 it would be 15 (def spawnRate) - 3 (Keter ODF) = 12 Final spawnRate)
    private void HandleAutoSpawning(Entity entity, ref float timer, float minSqr, float maxSqr) {
        timer += Time.deltaTime;
        if (timer < (entity.spawnRate - RevivalSessionEngine.otherDifficultyFactor)) return;

        timer = 0;

        if (TryPickRandomWaypoint(minSqr, maxSqr, out Transform chosen))
            MoveEntity(entity, chosen);
    }

    // SCP 106 gets his own spawning method because he uses the old spawning system to find a valid floor point near the player
    private void Handle106Spawning(Entity entity) {
        // 106 is already out (or disabled): abort any half finished spawn and don't count up
        if (entity.disabled || entity.activeInstances > 0) {
            if (is106Spawning) Cancel106Spawn();
            return;
        }

        // Effect is playing, wait for it to finish, then spawn 106 at the SAME position
        if (is106Spawning) {
            if (Player.Instance.isDead) { Cancel106Spawn(); return; }

            spawn106EffectTimer += Time.deltaTime;
            if (spawn106EffectTimer < SPAWN_106_EFFECT_DURATION) return;

            SpawnEntity(entity, pending106Position, Player.Instance.transform.rotation);

            // Effect prefab is left to clean itself up, same as the old spawner
            pending106Effect = null;
            is106Spawning = false;
            spawn106EffectTimer = 0f;
            return;
        }

        // Wait for the spawn interval
        spawn106Timer += Time.deltaTime;
        if (spawn106Timer < entity.spawnRate) return;

        spawn106Timer = 0f; // Reset regardless, so a failed check waits a full interval like the old coroutine

        if (Player.Instance.isDead) return;
        if (!TryGetRandomGroundPosition(out pending106Position)) return;

        pending106Effect = Instantiate(scp106_SpawnedEffect, pending106Position, Quaternion.identity);
        is106Spawning = true;
        spawn106EffectTimer = 0f;
    }

    private void Cancel106Spawn() {
        if (pending106Effect != null) Destroy(pending106Effect);
        pending106Effect = null;
        is106Spawning = false;
        spawn106EffectTimer = 0f;
    }

    #endregion

    #region Public Methods

    public void FindWaypointsInScene() {
        GameObject[] waypointObjs = GameObject.FindGameObjectsWithTag("Respawn");

        foreach (GameObject waypointObj in waypointObjs) {
            cachedWaypoints.Add(waypointObj.transform);
        }
    }

    public void DebugSpawnEntity(string entityIdentifier) {
        Debug.Log($"<color=#00ffff>Spawned entity: {entityIdentifier}</color>");
        SpawnEntity(entityIdentifier, GetClosestWaypointToPlayer());
    }

    // Spawn an entity passing in a string id and spawnpoint transform
    public void SpawnEntity(string entityIdentifier, Transform spawnPoint) {
        for (int i = 0; i < entities.Length; i++) {
            if (entities[i].entityIdentifier == entityIdentifier) {
                SpawnEntity(entities[i], spawnPoint);
                return;
            }
        }

        Debug.LogWarning("An entity with that identifier was not found.");
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
    public void MoveEntity(Entity entity, Transform spawnPoint, bool overrideCaseCheck = false) {
        if (entity.disabled) return;
        if (!overrideCaseCheck && entity.chasingPlayer) return;

        if (entity.latestActiveInstance != null) {
            if (entity.latestAgent != null) entity.latestAgent.Warp(spawnPoint.position);
        } else SpawnEntity(entity, spawnPoint);

        // Mild safeguard so that the scp can't leave an event area before the player encounters the scp
        spawn049Timer = 0;
        spawn173Timer = 0;
    }

    // Call this from any entity that get's destroyed, just to keep things clean
    // Removes it from it's active instance list and set's the "latest" vars to null
    public void UnregisterEntity(Entity entity, GameObject instance) {
        if (entity.activeInstances > 0) entity.activeInstances--;
        if (entity.latestActiveInstance == instance) {
            entity.latestActiveInstance = null;
            entity.latestAgent = null;
            entity.chasingPlayer = false;
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

    private bool TryGetRandomGroundPosition(out Vector3 groundPos) {
        groundPos = Player.Instance.transform.position;

        for (int i = 0; i < 10; i++) {
            Vector3 randomOffset = Random.insideUnitCircle * SPAWN_106_RADIUS;
            Vector3 candidate = Player.Instance.transform.position + new Vector3(randomOffset.x, 0, randomOffset.y);
            Vector3 rayStart = candidate + Vector3.up * 1f;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 1f + 50f, groundMask)) {
                Vector3 validPoint = hit.point;
                if (ValidSpawnRadiusOnNav(validPoint, 5f)) {
                    groundPos = validPoint;
                    return true;
                }
            }
        }

        return false;
    }

    private bool ValidSpawnRadiusOnNav(Vector3 position, float radius) {
        int samples = 8;
        float angleStep = 360 / samples;

        for (int i = 0; i < samples; i++) {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 checkPos = position + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

            if (!NavMesh.SamplePosition(checkPos, out NavMeshHit hit, 0.5f, NavMesh.AllAreas)) return false;
        }

        return NavMesh.SamplePosition(position, out NavMeshHit centerHit, 0.5f, NavMesh.AllAreas);
    }

    #endregion

    #region Public Extras

    public GameObject SpawnEntity(Entity entity, Transform spawnPoint) {
        return SpawnEntity(entity, spawnPoint.position, spawnPoint.rotation);
    }

    public GameObject SpawnEntity(Entity entity, Vector3 position, Quaternion rotation) {
        GameObject instance = Instantiate(entity.entityPrefab, position, rotation);
        entity.activeInstances++;
        entity.latestActiveInstance = instance;
        entity.latestAgent = instance.GetComponent<NavMeshAgent>();
        return instance;
    }

    public static bool AnyEntityChasingPlayer() {
        var entities = Instance.entities;
        for (int i = 0; i < entities.Length; i++) {
            if (entities[i].chasingPlayer) return true;
        }
        return false;
    }

    public Transform GetClosestWaypointToPlayer() {
        Vector3 playerPos = Player.Instance.transform.position;
        Transform closestWaypoint = null;
        float closestSqr = float.MaxValue;

        for (int i = 0; i < cachedWaypoints.Count; i++) {
            float sqr = (cachedWaypoints[i].position - playerPos).sqrMagnitude;
            if (sqr < closestSqr) {
                closestSqr = sqr;
                closestWaypoint = cachedWaypoints[i];
            }
        }

        return closestWaypoint;
    }

    #endregion
}