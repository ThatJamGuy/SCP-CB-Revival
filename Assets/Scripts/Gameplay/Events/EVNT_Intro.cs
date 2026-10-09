using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

// Welcome to hell

// Player is now possible target. (TODO: Think on whether or not making 173s AI naturally target the closest enemy in the intro as if the class ds were other players)
// If player leaves the chamber area 173 goes up, get's shot at a bit, kill the guard, and escapes through the vent.
// Small delay after vent breaking but will then switch the level geometry and lighting to the post breach version.

public class EVNT_Intro : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private bool developerMode;
    [SerializeField] private bool playVideo;
    [SerializeField] private bool skipIntro;

    [Header("High Priority References")]
    [SerializeField] private GameObject preBreachEnv;
    [SerializeField] private GameObject postBreachEnv;
    [SerializeField] private Door contDoor;
    [SerializeField] private GameObject contLights;
    [SerializeField] private GameObject outContLights;
    [SerializeField] private EVNT_PostBreach postBreachEvent;
    [SerializeField] private GameObject triggersParent;

    [Header("Developer References")]
    [SerializeField] private GameObject runtimeEngine;
    [SerializeField] private GameObject sessionEngine;
    [SerializeField] private GameObject consolePrefab;

    [Header("Audio References")]
    [SerializeField] private Transform ulgrinVoiceSource;
    [SerializeField] private EventReference ulgrinEscortEnd;
    [SerializeField] private EventReference ulgrinByTheWay;
    [SerializeField] private EventReference franklinA;
    [SerializeField] private EventReference franklinB;
    [SerializeField] private EventReference surge;
    [SerializeField] private EventReference balcGuardRadio;
    [SerializeField] private EventReference wtf;
    [SerializeField] private EventReference introDisposables;
    [SerializeField] private EventReference shitShit;
    [SerializeField] private StudioEventEmitter alarm10;
    [SerializeField] private StudioEventEmitter lightBreak;
    [SerializeField] private StudioEventEmitter lightObjBreak;
    [SerializeField] private StudioEventEmitter ventBreak;

    [Header("Scripted References")]
    [SerializeField] private GameObject ulgrin;
    [SerializeField] private GameObject laptopGuy;
    [SerializeField] private GameObject phoneGuy;
    [SerializeField] private Actor_Generic researcher2;
    [SerializeField] private Actor_Generic franklin;
    [SerializeField] private Actor_Generic balconyGuard;
    [SerializeField] private Actor_Generic balconyGuard_2;
    [SerializeField] private Actor_Generic classDA;
    [SerializeField] private Actor_Generic classDB;
    [SerializeField] private GameObject scp173;
    [SerializeField] private IK_MasterComponent classDB_IK;
    [SerializeField] private Transform nav_vend;
    [SerializeField] private Transform navPoint1_A;
    [SerializeField] private Transform navPoint1_B;
    [SerializeField] private Transform navPoint2_B;
    [SerializeField] private Transform nav173_1;
    [SerializeField] private Transform nav173_2;
    [SerializeField] private GameObject chamberEnterTrigger;
    [SerializeField] private GameObject gunLight;

    [Header("Generic References")]
    [SerializeField] private Animator ulgrinAnimator;
    [SerializeField] private Door beforeChamberDoor;
    [SerializeField] private Transform spawnRegular;
    [SerializeField] private Transform spawnSkipIntro;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private VideoPlayer introVideoPlayer;
    [SerializeField] private Animator brightnessFlashAnimator;
    [SerializeField] private GameObject doc173Paper;
    [SerializeField] private GameObject introCanvas;
    [SerializeField] private GameObject skipIntroCanvas;
    [SerializeField] private Transform balconyGuardGunTip;
    [SerializeField] private ParticleSystem elecSparks;
    [SerializeField] private GameObject lightToTurnOff;
    [SerializeField] private LightFlicker lightToFlicker;
    [SerializeField] private Rigidbody lightBody;

    private Coroutine cellCheckRoutine;

    private bool researcher2Ready;
    private bool researcher2AtVend;
    private bool playerInChamber = false;
    private bool enableBlinkSpamming;
    private int warningIndex = 0;
    private float spamTimeElapsed;

    private void Awake() {
        if (developerMode) {
            Instantiate(consolePrefab);
            Instantiate(runtimeEngine);
            Instantiate(sessionEngine);
        }
    }

    private void Start() {
        MusicManager.Instance.StopAllMusic();

        if (!skipIntro) {
            RevivalSessionEngine.canSave = false;
            RevivalSessionEngine.SetZone(0, true);
            RevivalRuntimeEngine.Instance.ChangeDiscordStatus("In a session", "Performing a test");

            Instantiate(playerPrefab, spawnRegular);

            if (playVideo) {
                MusicManager.Instance.SetTrack(MusicManager.MusicTrack.GeneralHorror01);

                Player.Instance.disableInput = true;
                Player.Instance.disableLooking = true;

                StartCoroutine(IntroVideoDelay());
            } else {
                introCanvas.SetActive(false);
                StartCoroutine(EscortEnd());
            }
        } else {
            RevivalSessionEngine.canSave = true;
            RevivalRuntimeEngine.Instance.ChangeDiscordStatus("In a session", "Roaming the facility");

            // Cleanup for intro skipping
            Destroy(franklin.gameObject);
            Destroy(classDA.gameObject);
            Destroy(classDB.gameObject);
            Destroy(researcher2.gameObject);
            Destroy(balconyGuard.gameObject);
            Destroy(triggersParent.gameObject);
            Destroy(ulgrin.gameObject);
            Destroy(laptopGuy.gameObject);
            Destroy(phoneGuy.gameObject);

            preBreachEnv.SetActive(false);
            postBreachEnv.SetActive(true);

            introCanvas.SetActive(false);
            skipIntroCanvas.SetActive(true);
            Instantiate(playerPrefab, spawnSkipIntro);

            postBreachEvent.TriggerPostBreachEvent(true);
        }
    }

    private void Update() {
        // Blink spamming to emulate intense light flickers
        // Why? Baked lighting that's why
        if (enableBlinkSpamming) {
            spamTimeElapsed += Time.deltaTime;

            if (spamTimeElapsed >= Random.Range(0.01f, 0.1f)) {
                Player.Instance.ForceBlink();
                spamTimeElapsed = 0;
            }
        }

        // Set the researcher into idle when he get's to the vending machine
        if (!researcher2AtVend && researcher2Ready && researcher2.gameObject.activeSelf) {
            if (researcher2.actorAgent.remainingDistance < 0.1f) {
                researcher2.SetAnimBool("Idle", true);
                researcher2AtVend = true;
            }
        }
    }

    #region Intro Video
    private void IntroVideoEndReached(VideoPlayer videoPlayer) {
        videoPlayer.loopPointReached -= IntroVideoEndReached;
        videoPlayer.transform.parent.gameObject.SetActive(false);
        brightnessFlashAnimator.SetTrigger("Flash");
        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.legacyLightFlicker);
        Player.Instance.disableInput = false;
        Player.Instance.disableLooking = false;

        StartCoroutine(EscortEnd());
    }

    private IEnumerator IntroVideoDelay() {
        yield return new WaitForSeconds(3);

        introVideoPlayer.Prepare();
        introVideoPlayer.Play();
        introVideoPlayer.loopPointReached += IntroVideoEndReached;
        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.introVideoSound);
    }
    #endregion

    #region Escord End
    private IEnumerator EscortEnd() {
        RevivalSessionEngine.SetZone(0, true);

        if (playVideo)
            InventorySystem.Instance.AddItemToInventory("docori");

        yield return new WaitForSeconds(3);
        ulgrinAnimator.SetTrigger("Cocky");
        AudioManager.PlayOneShot(ulgrinEscortEnd, ulgrinVoiceSource.position);
        yield return new WaitForSeconds(3);
        ulgrinAnimator.SetTrigger("Sigh");
        yield return new WaitForSeconds(4);
        AudioManager.PlayOneShot(ulgrinByTheWay, ulgrinVoiceSource.position);
        ulgrinAnimator.SetTrigger("Act_PaperA");
        yield return new WaitForSeconds(1);
        doc173Paper.SetActive(true);
    }

    public void OnPaperTaken() {
        researcher2.WalkTo(nav_vend.position);
        researcher2Ready = true;
    }
    #endregion

    #region Chamber Sequence Start

    public void OnBeforeChamberEntered() {
        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.chamberStingerA);
    }

    public void OnGotCloserToChamber() {
        classDB_IK.enableHeadIK = true;
        StartCoroutine(IntroChamberBegin());
    }

    public void OnEnteredChamber() {
        playerInChamber = true;
    }

    private IEnumerator IntroChamberBegin() {
        yield return new WaitForSeconds(4.5f);

        balconyGuard.Speak(balcGuardRadio);

        yield return new WaitForSeconds(4.5f);
        franklin.SetAnimTrigger("PressButton");
        yield return new WaitForSeconds(1.2f);
        alarm10.Play();
        classDB_IK.enableHeadIK = false;
        yield return new WaitForSeconds(3);
        MusicManager.Instance.StopAllMusic();
        contDoor.OpenDoor();
        yield return new WaitForSeconds(1);
        classDB.SetAnimTrigger("Nervous");
        yield return new WaitForSeconds(1);
        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.chamberStingerB);
        yield return new WaitForSeconds(1);
        RevivalRuntimeEngine.Instance.GiveAchievement("achv_173");
        yield return new WaitForSeconds(1.5f);
        AudioManager.PlayOneShot(franklinA);
        yield return new WaitForSeconds(5);
        classDB.WalkTo(navPoint1_B.position);
        yield return new WaitForSeconds(1.5f);
        classDA.WalkTo(navPoint1_A.position);
        yield return new WaitForSeconds(3);
        chamberEnterTrigger.SetActive(true);
        StartCoroutine(CheckPlayerInCell());
    }

    private IEnumerator CheckPlayerInCell() {
        yield return new WaitForSeconds(5);

        if (playerInChamber) {
            contDoor.CloseDoor();
            StartCoroutine(InsideChamberSequence());
            yield break;
        }

        if (warningIndex == 2) {
            contDoor.CloseDoor();
            AudioManager.PlayOneShot(franklinB);
            yield return new WaitForSeconds(3);
            balconyGuard.SetAnimBool("aiming", true);
            yield return new WaitForSeconds(4);
            AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.p90Oneshot, balconyGuardGunTip.position);
            Player.Instance.KillPlayer(1, 0.4f, 0.1f, "DEBUG - Killed via gun during the intro");
            RevivalRuntimeEngine.Instance.GiveAchievement("achv_intro");
            yield break;
        }

        warningIndex++;
        AudioManager.PlayOneShot(franklinB);

        yield return new WaitForSeconds(5);
        cellCheckRoutine = StartCoroutine(CheckPlayerInCell());
    }

    #endregion

    #region Chamber Sequence Mid-End

    private IEnumerator InsideChamberSequence() {
        yield return new WaitForSeconds(3);

        AudioManager.PlayOneShot(franklinA);

        yield return new WaitForSeconds(4);
        classDB.WalkTo(navPoint2_B.position);

        yield return new WaitForSeconds(5);

        AudioManager.PlayOneShot(surge);

        yield return new WaitForSeconds(1.1f);

        Player.Instance.ForceBlink(); // Force a blink so 173 can change poses
        scp173.transform.LookAt(classDB.transform.position, Vector3.up); // Rotate towards ClassD-B
        scp173.GetComponent<Animator>().Play("Pose3");
        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.legacyLightFlicker);

        lightBreak.Play();
        lightBody.useGravity = true;
        lightBody.AddForce(transform.forward * 10);
        elecSparks.Play();
        lightToTurnOff.SetActive(false);
        lightToFlicker.SetActive(true);

        //MusicManager.Instance.SetTrack(MusicManager.MusicTrack.SCP_173, 0);

        yield return new WaitForSeconds(0.5f);

        classDB.SetAnimTrigger("Scared");

        yield return new WaitForSeconds(0.5f);

        franklin.PlayAnimation("IdleAction09");
        contDoor.OpenDoor();

        yield return new WaitForSeconds(0.5f);

        balconyGuard.Speak(balcGuardRadio);
        classDA.SetAnimTrigger("LookBehind");

        yield return new WaitForSeconds(1);

        // Problem voice line starts here. Subsequent sequence should last 12 seconds!
        AudioManager.PlayOneShot(franklinA);

        yield return new WaitForSeconds(5);

        classDA.Speak(introDisposables);
        RevivalRuntimeEngine.Instance.ShakeCamera(0, 0.1f, 10);

        yield return new WaitForSeconds(4);

        classDB.SetAnimTrigger("WalkBackScared");

        yield return new WaitForSeconds(1);

        balconyGuard.Speak(wtf);

        yield return new WaitForSeconds(2f);

        // 12 seconds in, blackout !

        contLights.SetActive(false);
        outContLights.SetActive(false);

        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.introBoomA);

        RevivalRuntimeEngine.Instance.ShakeCamera(1f, 0, 4);
        classDB.Speak(introDisposables);

        yield return new WaitForSeconds(0.6f); // Death at 0.7 seconds in the sound

        contLights.SetActive(true);
        outContLights.SetActive(true);

        Player.Instance.ForceBlink();
        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.legacyLightFlicker);

        scp173.transform.position = new Vector3(classDB.transform.position.x, scp173.transform.position.y, classDB.transform.position.z + 1);
        scp173.transform.LookAt(classDB.transform.position, Vector3.up);
        scp173.GetComponent<Animator>().Play("Pose6");
        classDB.PlayAnimation("173Death02");
        classDA.SetAnimTrigger("Scared");

        enableBlinkSpamming = true;
        yield return new WaitForSeconds(0.5f);
        enableBlinkSpamming = false;

        contLights.SetActive(false);
        outContLights.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        contLights.SetActive(true);
        outContLights.SetActive(true);

        Player.Instance.ForceBlink();
        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.legacyLightFlicker);

        scp173.transform.position = new Vector3(classDA.transform.position.x, scp173.transform.position.y, classDA.transform.position.z + 1);
        scp173.transform.LookAt(classDA.transform.position, Vector3.up);
        scp173.GetComponent<Animator>().Play("Pose2");
        classDA.PlayAnimation("173Death01");
        classDA.Speak(AudioManager.Instance.globalAudioContainer.neckBreak);

        enableBlinkSpamming = true;
        yield return new WaitForSeconds(0.5f);
        enableBlinkSpamming = false;

        yield return new WaitForSeconds(0.5f);

        Player.Instance.ForceBlink();
        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.legacyLightFlicker);

        MusicManager.Instance.StopAllMusic();
        scp173.transform.position = new Vector3(Player.Instance.transform.position.x, scp173.transform.position.y, Player.Instance.transform.localPosition.z + 2);

        // Look at the player (Via extra methods because it doesn't want to work normally for some reason)
        Transform scp173Self = scp173.transform;
        Vector3 playerPos = Player.Instance.transform.position;
        playerPos.y = scp173Self.position.y;
        scp173Self.LookAt(playerPos, Vector3.up);

        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.chamberStingerC);

        enableBlinkSpamming = true;
        yield return new WaitForSeconds(0.5f);
        enableBlinkSpamming = false;

        // Get all the main lights flickering
        foreach (Transform child in outContLights.transform) {
            if (child.TryGetComponent<LightFlicker>(out LightFlicker flicker))
                flicker.enabled = true;
        }

        yield return new WaitForSeconds(5);

        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.introBoomA);
        RevivalRuntimeEngine.Instance.ShakeCamera(0.2f, 0, 1);

        contLights.SetActive(false);
        outContLights.SetActive(false);

        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.legacyLightFlicker);

        // Put him up there with the guard
        scp173.GetComponent<Animator>().Play("Pose6");
        scp173.transform.position = nav173_1.transform.position;
        scp173.transform.rotation = nav173_1.transform.rotation;

        balconyGuard.gameObject.SetActive(false);
        balconyGuard_2.gameObject.SetActive(true);

        franklin.gameObject.SetActive(false);

        enableBlinkSpamming = true;
        yield return new WaitForSeconds(0.5f);
        enableBlinkSpamming = false;

        balconyGuard_2.Speak(shitShit);

        yield return new WaitForSeconds(1);

        balconyGuard_2.SetAnimBool("aiming", true);
        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.superShoot, balconyGuard_2.transform.position);
        gunLight.SetActive(true);

        yield return new WaitForSeconds(3.8f);

        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.legacyLightFlicker);
        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.introBoomA);
        RevivalRuntimeEngine.Instance.ShakeCamera(0.2f, 0, 1);
        gunLight.SetActive(false);

        scp173.transform.position = nav173_2.transform.position;
        classDA.Speak(AudioManager.Instance.globalAudioContainer.neckBreak);
        balconyGuard_2.SetAnimTrigger("Die");

        yield return new WaitForSeconds(1);

        RevivalRuntimeEngine.Instance.ShakeCamera(0.2f, 0, 1);
        ventBreak.Play();

        enableBlinkSpamming = true;
        yield return new WaitForSeconds(0.5f);
        enableBlinkSpamming = false;

        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.introBoomA);

        enableBlinkSpamming = true;

        preBreachEnv.SetActive(false);
        postBreachEnv.SetActive(true);

        postBreachEvent.TriggerPostBreachEvent();

        yield return new WaitForSeconds(0.5f);

        enableBlinkSpamming = false;
    }

    #endregion
}