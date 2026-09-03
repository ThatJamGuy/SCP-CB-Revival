using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

// 4 - SCF presses a button. The doors open, playing the chamber 173 stinger. class d 2 shakes his limbs around a bit as if nervous. They are urged to enter.
// 5 - The two ds enter at different speeds and arrive at different locations. The door closes with a box collider preventing the player from leaving once they enter.
// 5.1 - If the player does not enter for a period of time, a random threat1 line is chosen for SCF. Same period goes by a threat2 line. Same period the player is shot.
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
    [SerializeField] private GameObject inputManager;
    [SerializeField] private GameObject runtimeEngine;
    [SerializeField] private GameObject sessionEngine;
    [SerializeField] private GameObject consolePrefab;

    [Header("Audio References")]
    [SerializeField] private Transform ulgrinVoiceSource;
    [SerializeField] private EventReference ulgrinEscortEnd;
    [SerializeField] private EventReference ulgrinByTheWay;
    [SerializeField] private EventReference franklinA;
    [SerializeField] private EventReference franklinB;

    [Header("Scripted References")]
    [SerializeField] private Actor_Generic franklin;
    [SerializeField] private Actor_Generic classDA;
    [SerializeField] private Actor_Generic classDB;
    [SerializeField] private IK_MasterComponent classDB_IK;
    [SerializeField] private Transform navPoint1_A;
    [SerializeField] private Transform navPoint1_B;

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

    private bool playerNotInChamber = true;
    private int playerBadBoyIndex = 0;

    private void Awake() {
        if (developerMode) {
            Instantiate(consolePrefab);
            Instantiate(inputManager);
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

    #region Intro Video
    private void IntroVideoEndReached(VideoPlayer videoPlayer) {
        videoPlayer.transform.parent.gameObject.SetActive(false);
        brightnessFlashAnimator.SetTrigger("Flash");
        AudioManager.PlayOneShot(AudioEventsHolder.Instance.legacyLightFlicker);
        Player.Instance.disableInput = false;
        Player.Instance.disableLooking = false;

        StartCoroutine(EscortEnd());
    }

    private IEnumerator IntroVideoDelay() {
        yield return new WaitForSeconds(3);

        introVideoPlayer.Prepare();
        introVideoPlayer.Play();
        introVideoPlayer.loopPointReached += IntroVideoEndReached;
        AudioManager.PlayOneShot(AudioEventsHolder.Instance.introVideoSound);
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
        //ulgrinAnimator.SetTrigger("Acknowledge");
        ulgrinAnimator.SetTrigger("Act_PaperA");
        yield return new WaitForSeconds(1);
        doc173Paper.SetActive(true);
    }
    #endregion

    #region Chamber Sequence Start


    public void OnBeforeChamberEntered() {
        MusicManager.Instance.SetTrack(MusicManager.MusicTrack.SCP_173, 0);
        AudioManager.PlayOneShot(AudioEventsHolder.Instance.chamberStingerA);
    }

    public void OnGotCloserToChamber() {
        classDB_IK.enableHeadIK = true;
        StartCoroutine(IntroChamberBegin());
    }

    private IEnumerator IntroChamberBegin() {
        yield return new WaitForSeconds(4);
        franklin.SetAnimTrigger("PressButton");
        yield return new WaitForSeconds(1.2f);
        contDoor.OpenDoor();
        classDB_IK.enableHeadIK = false;
        yield return new WaitForSeconds(1);
        classDB.SetAnimTrigger("Nervous");
        yield return new WaitForSeconds(1);
        AudioManager.PlayOneShot(AudioEventsHolder.Instance.chamberStingerB);
        yield return new WaitForSeconds(2);
        AudioManager.PlayOneShot(franklinA);
        yield return new WaitForSeconds(5);
        classDB.WalkTo(navPoint1_B.position);
        yield return new WaitForSeconds(2f);
        classDA.WalkTo(navPoint1_A.position);
    }

    private IEnumerator CheckPlayerInCell() {
        yield return new WaitForSeconds(10);

        if (playerNotInChamber) {
            playerBadBoyIndex++;
            AudioManager.PlayOneShot(franklinB);
        }
    }

    #endregion
}