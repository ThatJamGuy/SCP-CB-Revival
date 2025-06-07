using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceController : MonoBehaviour
{
    public static AmbienceController Instance { get; private set; }

    [Header("Zone")]
    [Tooltip("Zone 0 - Intro, Zone 1 - Light Containment Zone, Zone 2 - Heavy Containment Zone, Zone 3 - Entrance Zone")]
    public int currentZone;

    [Header("Ambience Clips")]
    [SerializeField] AudioClip[] zone0Ambience;
    [SerializeField] AudioClip[] zone1Ambience;
    [SerializeField] AudioClip[] zone2Ambience;
    [SerializeField] AudioClip[] zone3Ambience;
    [SerializeField] AudioClip[] commotionSounds;

    [Header("Ambiance Sources")]
    [SerializeField] AudioSource ambienceSource;
    [SerializeField] AudioSource commotionSource;

    private int currentCommotionIndex = 0;
    private Dictionary<int, AudioClip[]> zoneAmbienceMap;
    private Coroutine ambienceCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeZoneAmbienceMap();
        StartAmbience();
    }

    private void InitializeZoneAmbienceMap()
    {
        zoneAmbienceMap = new Dictionary<int, AudioClip[]>
        {
            { 0, zone0Ambience },
            { 1, zone1Ambience },
            { 2, zone2Ambience },
            { 3, zone3Ambience }
        };
    }

    public void PlayNextCommotion()
    {
        if (currentCommotionIndex < commotionSounds.Length)
        {
            commotionSource.clip = commotionSounds[currentCommotionIndex];
            commotionSource.Play();
            currentCommotionIndex++;
            Invoke(nameof(PlayNextCommotion), 15f);
        }
        else
        {
            commotionSource.Stop();
        }
    }

    private void StartAmbience()
    {
        if (ambienceCoroutine != null)
        {
            StopCoroutine(ambienceCoroutine);
        }
        ambienceCoroutine = StartCoroutine(Ambience());
    }

    private IEnumerator Ambience()
    {
        while (true)
        {
            if (zoneAmbienceMap.ContainsKey(currentZone) && zoneAmbienceMap[currentZone].Length > 0)
            {
                int rand = Random.Range(0, zoneAmbienceMap[currentZone].Length);
                ambienceSource.panStereo = Random.Range(-0.9f, 0.9f);
                ambienceSource.clip = zoneAmbienceMap[currentZone][rand];
                ambienceSource.Play();
                yield return new WaitForSeconds(Random.Range(10, 30));
            }
            else
            {
                yield return null;
            }
        }
    }

    public void ChangeZone(int newZone)
    {
        currentZone = newZone;
        StartAmbience();
    }
}
