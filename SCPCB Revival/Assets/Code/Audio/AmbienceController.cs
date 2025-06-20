using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceController : MonoBehaviour {
    public static AmbienceController Instance { get; private set; }

    [Header("Zone")]
    public int currentZone;

    [Header("Ambience Clips")]
    [SerializeField] AudioClip[] zone0Ambience;
    [SerializeField] AudioClip[] zone1Ambience;
    [SerializeField] AudioClip[] zone2Ambience;
    [SerializeField] AudioClip[] zone3Ambience;
    [SerializeField] AudioClip[] commotionSounds;

    [Header("Player")]
    [SerializeField] Transform player;

    private int currentCommotionIndex;
    private Dictionary<int, AudioClip[]> zoneAmbienceMap;
    private Coroutine ambienceCoroutine;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() {
        InitializeZoneAmbienceMap();
        StartAmbience();
        PlayNextCommotion();
    }

    private void InitializeZoneAmbienceMap() {
        zoneAmbienceMap = new Dictionary<int, AudioClip[]>
        {
            { 0, zone0Ambience },
            { 1, zone1Ambience },
            { 2, zone2Ambience },
            { 3, zone3Ambience }
        };
    }

    private void StartAmbience() {
        if (ambienceCoroutine != null) StopCoroutine(ambienceCoroutine);
        ambienceCoroutine = StartCoroutine(Ambience());
    }

    private IEnumerator Ambience() {
        while (true) {
            if (zoneAmbienceMap.ContainsKey(currentZone) && zoneAmbienceMap[currentZone].Length > 0) {
                var clips = zoneAmbienceMap[currentZone];
                var clip = clips[Random.Range(0, clips.Length)];
                PlaySpatialClip(clip, RandomPositionAroundPlayer());
                yield return new WaitForSeconds(Random.Range(10, 30));
            }
            else yield return null;
        }
    }

    public void PlayNextCommotion() {
        if (currentCommotionIndex < commotionSounds.Length) {
            PlaySpatialClip(commotionSounds[currentCommotionIndex], RandomPositionAroundPlayer());
            currentCommotionIndex++;
            Invoke(nameof(PlayNextCommotion), 10f);
        }
    }

    private void PlaySpatialClip(AudioClip clip, Vector3 position) {
        var tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;
        var source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 0.5f;
        source.minDistance = 5f;
        source.maxDistance = 25f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.Play();
        Destroy(tempGO, clip.length + 0.5f);
    }

    private Vector3 RandomPositionAroundPlayer() {
        var offset = Random.onUnitSphere * Random.Range(5f, 15f);
        offset.y = Mathf.Clamp(offset.y, 0.5f, 3f);
        return player.position + offset;
    }

    public void ChangeZone(int newZone) {
        currentZone = newZone;
        StartAmbience();
    }
}