using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceController : MonoBehaviour {
    public static AmbienceController Instance { get; private set; }

    [Header("Zone")]
    public int currentZone;
    [SerializeField] private float ambienceInterval = 30f;

    [Header("Commotion")]
    [SerializeField] private float commotionInterval = 10f;
    [SerializeField] private int commotionCount = 27;

    private EventReference zoneAmbienceEvent;
    private EventReference commotionEvent;

    private Dictionary<int, EventReference> zoneAmbienceMap;
    private Coroutine ambienceCoroutine;
    private Transform[] player;

    private int currentCommotionIndex = 0;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    //private void OnEnable() {
    //    DevConsole.singleton.AddCommand(new ActionCommand(PlayCommotionEvent) { className = "Event" });
    //}

    private void Start() {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        player = new Transform[playerObjects.Length];
        for (int i = 0; i < playerObjects.Length; i++) {
            player[i] = playerObjects[i].transform;
        }

        if (FMODEvents.instance != null) {
            zoneAmbienceEvent = FMODEvents.instance.zone1Ambience;
            commotionEvent = FMODEvents.instance.commotionSounds;
        }
        else {
            Debug.LogWarning("FMODEvents.instance is null - ambience will not play.");
            zoneAmbienceEvent = default;
            commotionEvent = default;
        }

        InitializeZoneAmbienceMap();
        StartAmbience();
    }

    private void InitializeZoneAmbienceMap() {
        zoneAmbienceMap = new Dictionary<int, EventReference>
        {
                { 0, zoneAmbienceEvent },
                { 1, zoneAmbienceEvent },
                { 2, zoneAmbienceEvent },
                { 3, zoneAmbienceEvent }
            };
    }

    private void StartAmbience() {
        if (ambienceCoroutine != null) StopCoroutine(ambienceCoroutine);
        ambienceCoroutine = StartCoroutine(AmbienceWatcher());
    }

    private IEnumerator AmbienceWatcher() {
        while (true) {
            EventReference ev = default;
            if (zoneAmbienceMap != null && zoneAmbienceMap.ContainsKey(currentZone)) ev = zoneAmbienceMap[currentZone];

            if (ev.IsNull) {
                yield return new WaitForSeconds(1f);
                continue;
            }

            try {
                var instance = RuntimeManager.CreateInstance(ev);
                instance.set3DAttributes(RuntimeUtils.To3DAttributes(RandomPositionAroundPlayer()));
                instance.start();
                instance.release();
            }
            catch (System.Exception ex) {
                Debug.LogWarning($"Failed to play ambience event: {ex.Message}");
            }

            yield return new WaitForSeconds(Mathf.Max(0.01f, ambienceInterval));
        }
    }

    private IEnumerator CommotionSequence() {
        while (currentCommotionIndex < commotionCount) {
            PlayNextCommotion();
            yield return new WaitForSeconds(commotionInterval);
            currentCommotionIndex++;
        }
    }

    public void PlayCommotionEvent() {
        StartCoroutine(CommotionSequence());
    }

    private void PlayNextCommotion() {
        if (commotionEvent.IsNull) return;
        if (AudioManager.instance == null) return;

        AudioManager.instance.PlaySound(commotionEvent, RandomPositionAroundPlayer());
    }

    private Vector3 RandomPositionAroundPlayer() {
        if (player == null || player.Length == 0) return Vector3.zero;
        Transform randomPlayer = player[Random.Range(0, player.Length)];
        var offset = Random.onUnitSphere * Random.Range(5f, 15f);
        offset.y = Mathf.Clamp(offset.y, 0.5f, 3f);
        return randomPlayer.position + offset;
    }

    private void OnDisable() {
        CancelInvoke(nameof(PlayNextCommotion));
    }

    private void OnDestroy() {
        CancelInvoke(nameof(PlayNextCommotion));
    }
}