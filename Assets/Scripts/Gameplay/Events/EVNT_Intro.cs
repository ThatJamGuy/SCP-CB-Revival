using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

// Welcome to hell

// 6 - SCF urges to approach 173. class d 2 does so. After d 2 reaches close to 173 a light breaks, then the door opens.
// 7 - SCF says his line and the lights go out on queue, allowing 173 to kill a guy. Lights go on briefly and then out again allowing 173 to kill another guy.
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
    [SerializeField] private StudioEventEmitter alarm10;
    [SerializeField] private StudioEventEmitter lightBreak;
    [SerializeField] private StudioEventEmitter lightObjBreak;

    [Header("Scripted References")]
    [SerializeField] private Actor_Generic franklin;
    [SerializeField] private Actor_Generic balconyGuard;
    [SerializeField] private Actor_Generic classDA;
    [SerializeField] private Actor_Generic classDB;
    [SerializeField] private GameObject scp173;
    [SerializeField] private IK_MasterComponent classDB_IK;
    [SerializeField] private Transform navPoint1_A;
    [SerializeField] private Transform navPoint1_B;
    [SerializeField] private Transform navPoint2_B;
    [SerializeField] private GameObject chamberEnterTrigger;

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
    [SerializeField] private Transform balconyGuardGunTip;
    [SerializeField] private ParticleSystem elecSparks;
    [SerializeField] private GameObject lightToTurnOff;
    [SerializeField] private LightFlicker lightToFlicker;
    [SerializeField] private Rigidbody lightBody;

    private Coroutine cellCheckRoutine;

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

            introCanvas.SetActive(false);
            Instantiate(playerPrefab, spawnSkipIntro);
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

        RevivalRuntimeEngine.Instance.ShakeCamera(1f, 0, 4);
        classDB.Speak(introDisposables);

        yield return new WaitForSeconds(0.6f); // Death at 0.7 seconds in the sound

        Player.Instance.ForceBlink();
        scp173.transform.position = new Vector3(classDB.transform.position.x, scp173.transform.position.y, classDB.transform.position.z + 1);
        scp173.transform.LookAt(classDB.transform.position, Vector3.up);
        scp173.GetComponent<Animator>().Play("Pose6");
        classDB.PlayAnimation("173Death02");
        classDA.SetAnimTrigger("Scared");

        enableBlinkSpamming = true;
        yield return new WaitForSeconds(0.5f);
        enableBlinkSpamming = false;

        yield return new WaitForSeconds(0.5f);

        Player.Instance.ForceBlink();
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
        MusicManager.Instance.StopAllMusic();
        scp173.transform.position = new Vector3(Player.Instance.transform.position.x, scp173.transform.position.y, Player.Instance.transform.position.z + 1);
        scp173.transform.LookAt(new Vector3(Player.Instance.transform.position.x, transform.position.y, Player.Instance.transform.position.z));
        AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.chamberStingerC);

        enableBlinkSpamming = true;
        yield return new WaitForSeconds(0.5f);
        enableBlinkSpamming = false;

        yield return new WaitForSeconds(5);
    }

    #endregion
}