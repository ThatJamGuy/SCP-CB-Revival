using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

// Planning (CUT DOWN VERSION FOR v0.0.6A / Itch.io Release):
// 1 - Brightness flash introduces player into the before173 room. Ulgrin starts off with the welp we're here line.
// 2 - Ulgrin hands the player a paper, and then if waiting long enough he urges him into the 173 room.
// 3 - Class d 1 stands facing the chamber door. Class d 2 uses head ik to look at the player. SCF stands by above. Assisting scientist walks on a phone call behind SCF.
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
    [SerializeField] private bool skipIntro;

    [Header("High Priority References")]
    [SerializeField] private GameObject preBreachEnv;
    [SerializeField] private GameObject postBreachEnv;

    [Header("Developer References")]
    [SerializeField] private GameObject inputManager;
    [SerializeField] private GameObject runtimeEngine;
    [SerializeField] private GameObject sessionEngine;

    [Header("Audio References")]
    [SerializeField] private Transform ulgrinVoiceSource;
    [SerializeField] private EventReference ulgrinEscortEndA;
    [SerializeField] private EventReference ulgrinEscortEndB;
    [SerializeField] private EventReference ulgrinByTheWay;

    [Header("Generic References")]
    [SerializeField] private Animator ulgrinAnimator;
    [SerializeField] private Door beforeChamberDoor;
    [SerializeField] private Transform spawnRegular;
    [SerializeField] private Transform spawnSkipIntro;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private VideoPlayer introVideoPlayer;
    [SerializeField] private Animator brightnessFlashAnimator;

    private void Awake() {
        if (developerMode) {
            Instantiate(inputManager);
            Instantiate(runtimeEngine);
            Instantiate(sessionEngine);
        }
    }

    private void Start() {
        MusicManager.Instance.StopAllMusic();

        if (!skipIntro) {
            RevivalSessionEngine.currentZone = 0;
            //MusicManager.Instance.SetTrack(MusicManager.MusicTrack.GeneralHorror01);

            Instantiate(playerPrefab, spawnRegular);

            //Player.Instance.disableInput = true;
            //Player.Instance.disableLooking = true;

            //StartCoroutine(IntroVideoDelay());
            StartCoroutine(EscortEnd());
        } else {
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
        //InventorySystem.Instance.AddItemToInventory("docori"); Uncomment this later when the intro video is back in
        yield return new WaitForSeconds(3);
        AudioManager.PlayOneShot(ulgrinEscortEndA, ulgrinVoiceSource.position);
        yield return new WaitForSeconds(7);
        AudioManager.PlayOneShot(ulgrinEscortEndB, ulgrinVoiceSource.position);
        beforeChamberDoor.OpenDoor();
    }
    #endregion
}