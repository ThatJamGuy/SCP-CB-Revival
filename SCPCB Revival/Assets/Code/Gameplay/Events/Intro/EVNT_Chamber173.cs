using scpcbr;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class EVNT_Chamber173 : MonoBehaviour {
    [Header("NPCs")]
    public GameObject npc173;
    public GameObject npcFranklin;
    public GameObject npcJohn;
    public GameObject npcDClass1;
    public GameObject npcDClass2;
    public GameObject guard;

    [Header("Sound")]
    public AudioSource intercomSource;
    public AudioClip[] intercomClips;
    public AudioClip[] refuseEnterClips;
    public AudioSource revealStinger;
    public AudioSource dClass2Speak;
    public AudioSource flickerOffSource;
    public AudioSource bangSound;
    public AudioSource dClass1Speak;
    public AudioClip neckBreak1;
    public AudioClip neckBreak2;
    public AudioSource guardVoice;
    public AudioClip[] wtfs;
    public AudioClip ohShit;
    public AudioSource horror;
    public AudioSource ventBreak;
    public AudioSource modernAlarm;

    [Header("Objects")]
    public Door chamberDoor;
    public GameObject killPlayerGunTrigger;
    public Animator flickerOffAnimator;
    public GameObject flickerVFX;
    public GameObject gunshotLight;
    public GameObject currChamber;
    public GameObject nextChamber;
    public GameObject introFog;
    public GameObject gameplayFog;
    public Animator flickerFogAnimator;
    public GameObject flickerOffObjects;
    public EVNT_IntroShakes introShakes;

    [Header("Nodes")]
    public Transform enterNode1;
    public Transform enterNode2;
    public Transform apprachNode1;
    public Transform killNode1;
    public Transform killNode2;
    public Transform killNode3;
    public Transform killNode4;

    [Header("Lightmaps")]
    public MeshRenderer roomMesh;
    public Texture2D[] brightLightMap, darkLightMap;

    private LightmapData[] darkLightMapData, brightLightMapData;

    private bool isInChamber = false;
    private int checkCount = 0;
    private int checkTime = 6;

    private void Start() {
        List<LightmapData> dlightmap = new List<LightmapData>();

        for(int i = 0; i < darkLightMap.Length; i++) {
            LightmapData data = new LightmapData();
            data.lightmapColor = darkLightMap[i];
            dlightmap.Add(data);
        } 
        
        darkLightMapData = dlightmap.ToArray();

        List<LightmapData> blightmap = new List<LightmapData>();

        for(int i = 0; i < brightLightMap.Length; i++) {
            LightmapData data = new LightmapData();
            data.lightmapColor = brightLightMap[i];
            blightmap.Add(data);
        }

        brightLightMapData = blightmap.ToArray();
    }

    public void ApplyLightmap(MeshRenderer targetRenderer, Texture2D[] lightmapTextures, int lightmapIndex) {
        var currentLightmaps = LightmapSettings.lightmaps;
        if (lightmapIndex >= currentLightmaps.Length) {
            var updatedLightmaps = new List<LightmapData>(currentLightmaps);
            for (int i = currentLightmaps.Length; i <= lightmapIndex; i++) {
                updatedLightmaps.Add(new LightmapData());
            }
            currentLightmaps = updatedLightmaps.ToArray();
        }

        for (int i = 0; i < lightmapTextures.Length; i++) {
            var data = new LightmapData();
            data.lightmapColor = lightmapTextures[i];
            currentLightmaps[lightmapIndex + i] = data;
        }

        LightmapSettings.lightmaps = currentLightmaps;
        targetRenderer.lightmapIndex = lightmapIndex;
    }

    public void TriggerStartEnterChamber() {
        MusicPlayer.Instance.StartMusicByName("SCP-173 Chamber");
    }

    public void TriggerEnterChamber() {
        StartCoroutine(EnterChamberPart());
    }

    public void TriggerInChamber() {
        isInChamber = true;
    }

    private IEnumerator EnterChamberPart() {
        yield return new WaitForSeconds(5);
        npcFranklin.GetComponent<Animator>().SetTrigger("PushButton");
        yield return new WaitForSeconds(1);
        chamberDoor.OpenDoor();
        npcDClass1.GetComponent<Animator>().SetTrigger("ShakeItOff");
        yield return new WaitForSeconds(2);
        revealStinger.Play();
        yield return new WaitForSeconds(3);
        intercomSource.PlayOneShot(intercomClips[0]);
        yield return new WaitForSeconds(5);
        npcDClass1.GetComponent<NPC_RootMotionAgent>().WalkToPosition(enterNode2.position);
        yield return new WaitForSeconds(1);
        npcDClass2.GetComponent<NPC_RootMotionAgent>().WalkToPosition(enterNode1.position);
        StartCoroutine(CheckEnterChamber());
    }

    private IEnumerator CheckEnterChamber() {
        yield return new WaitForSeconds(checkTime);
        if(!isInChamber) {
            intercomSource.PlayOneShot(refuseEnterClips[checkCount]);
            if(checkCount == 1) {
                checkCount++;
                yield return new WaitForSeconds(checkTime+4);
                if(!isInChamber) {
                    chamberDoor.CloseDoor();
                    intercomSource.PlayOneShot(refuseEnterClips[checkCount]);
                    yield return new WaitForSeconds(5);
                    guard.GetComponent<Animator>().SetTrigger("Aim");
                    yield return new WaitForSeconds(2);
                    killPlayerGunTrigger.SetActive(true);
                } else {
                    StartCoroutine(InChamberPart());
                }
            } else {
                checkCount++;
                yield return new WaitForSeconds(3);
                StartCoroutine(CheckEnterChamber());
            }
        } else {
            StartCoroutine(InChamberPart());
        }
    }

    private IEnumerator InChamberPart() {
        chamberDoor.CloseDoor();
        yield return new WaitForSeconds(4);
        intercomSource.clip = intercomClips[1];
        intercomSource.Play();
        yield return new WaitForSeconds(3);
        npcDClass1.GetComponent<NPC_RootMotionAgent>().WalkToPosition(apprachNode1.position);
        yield return new WaitForSeconds(4);
        flickerVFX.SetActive(true);
        flickerOffSource.Play();
        flickerOffAnimator.SetTrigger("FlickerOff");
        yield return new WaitForSeconds(1);
        chamberDoor.OpenDoor();
        npcDClass2.GetComponent<Animator>().SetTrigger("LookBehind");
        yield return new WaitForSeconds(2);
        intercomSource.clip = intercomClips[2];
        intercomSource.Play();
        yield return new WaitForSeconds(3);
        GlobalCameraShake.Instance.ShakeCamera(0f, 0.03f, 10);
        yield return new WaitForSeconds(2);
        dClass2Speak.Play();
        yield return new WaitForSeconds(2);
        npcDClass1.GetComponent<NPC_RootMotionAgent>().ToggleAgent();
        npcDClass1.GetComponent<Animator>().SetTrigger("WalkBack");
        yield return new WaitForSeconds(3);
        guardVoice.clip = wtfs[Random.Range(0, wtfs.Length)];
        guardVoice.Play();
        yield return new WaitForSeconds(2);
        GlobalCameraShake.Instance.ShakeCamera(0.5f, 0f, 4);
        dClass1Speak.Play();
        flickerFogAnimator.SetTrigger("Flicker");
        horror.Play();
        bangSound.Play();
        flickerFogAnimator.SetTrigger("Flicker");
        npc173.transform.position = killNode1.position;
        npc173.transform.rotation = killNode1.rotation;
        yield return new WaitForSeconds(0.8f);
        npcDClass1.GetComponent<Animator>().SetTrigger("173Die1");
        npcDClass2.GetComponent<Animator>().SetTrigger("FallBack");
        yield return new WaitForSeconds(1);
        flickerFogAnimator.SetTrigger("Flicker");
        yield return new WaitForSeconds(0.5f);
        npc173.transform.position = killNode2.position;
        npc173.transform.rotation = killNode2.rotation;
        npcDClass2.GetComponent<Animator>().SetTrigger("173Die2");
        dClass2Speak.clip = neckBreak1;
        dClass2Speak.Play();
        yield return new WaitForSeconds(4);
        flickerFogAnimator.SetTrigger("Flicker");
        flickerOffObjects.SetActive(false);
        GlobalCameraShake.Instance.ShakeCamera(0.1f, 0f, 3);
        bangSound.Play();
        ApplyLightmap(roomMesh, darkLightMap, 0);
        introFog.SetActive(false);
        npc173.transform.position = killNode3.position;
        npc173.transform.rotation = killNode3.rotation;
        guard.transform.LookAt(npc173.transform);
        guardVoice.clip = ohShit;
        guardVoice.Play();
        guard.GetComponent<Animator>().SetTrigger("Aim");
        yield return new WaitForSeconds(1);
        gunshotLight.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        introFog.SetActive(false);
        gameplayFog.SetActive(true);
        flickerFogAnimator.SetTrigger("Flicker");
        bangSound.Play();
        npc173.transform.position = killNode4.position;
        guard.GetComponent<Animator>().SetTrigger("Die173");
        guardVoice.clip = neckBreak2;
        guardVoice.Play();
        gunshotLight.SetActive(false);
        yield return new WaitForSeconds(1);
        flickerFogAnimator.SetTrigger("Flicker");
        GlobalCameraShake.Instance.ShakeCamera(0.1f, 0f, 3);
        currChamber.SetActive(false);
        nextChamber.SetActive(true);
        ventBreak.Play();
        introShakes.TriggerIntroShakes();
        yield return new WaitForSeconds(1);
        MusicPlayer.Instance.StartMusicByName("The Breach");
        modernAlarm.Play();
    }
}