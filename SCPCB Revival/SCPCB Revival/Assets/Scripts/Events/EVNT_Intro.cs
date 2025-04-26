using System.Collections;
using UnityEngine;

public class EVNT_Intro : MonoBehaviour
{
    [SerializeField] private GameObject generatedMapParent;
    [SerializeField] private bool enablePartOne;

    [Header("Intro Part One")]
    public bool readyForEscort;

    [SerializeField] private GameObject cellDoor;
    [SerializeField] private NPC_Puppet guard01;
    [SerializeField] private NPC_Puppet guard02;
    [SerializeField] private NPC_Puppet classDEscortGuard01;
    [SerializeField] private NPC_Puppet classDEscortGuard02;
    [SerializeField] private AudioClip[] vibingGuardMusic;
    [SerializeField] private AudioClip exitCell;
    [SerializeField] private AudioClip[] refuseExitCell;
    [SerializeField] private AudioClip[] refuseExitGas;
    [SerializeField] private AudioClip[] escortBegin;
    [SerializeField] private AudioClip refuseToCooperateMusic;
    [SerializeField] private AudioSource autoVoxKillSource;
    [SerializeField] private GameObject[] cellLights;
    [SerializeField] private AudioSource gasValvlesOpen;
    [SerializeField] private GameObject[] gas;

    [Header("Audio")]
    [SerializeField] private AudioSource beforeDoorOpenAudio;
    [SerializeField] private AudioSource vibingGuardSource;
    [SerializeField] private AudioClip[] ulgrinSpeeches;
    [SerializeField] private AudioClip[] otherGuardSpeeches;

    [SerializeField] private NPC_Puppet walkingFranklin;
    [SerializeField] private GameObject scFranklinp1;
    [SerializeField] private GameObject scFranklinp2;
    [SerializeField] private GameObject scFranklinDoor1;

    [Header("Intro Part 2")]
    public bool readyForApproach173;

    [SerializeField] private NPC_Puppet scFranklin;
    [SerializeField] private GameObject otherGuy;
    [SerializeField] private NPC_Puppet guardGuy;
    [SerializeField] private NPC_Puppet dclass1;
    [SerializeField] private NPC_Puppet dclass2;
    [SerializeField] private Transform node1;
    [SerializeField] private Transform node2;
    [SerializeField] private Transform node3;
    [SerializeField] private Transform node4;
    [SerializeField] private Transform scpNode1;
    [SerializeField] private Transform scpNode2;
    [SerializeField] private Transform scpNode3;
    [SerializeField] private Door contDoor;
    [SerializeField] private GameObject introTrigger02;
    [SerializeField] private GameObject introTrigger03;
    [SerializeField] private AudioClip announcementEnter;
    [SerializeField] private AudioClip announcementApproach173;
    [SerializeField] private AudioClip announcementDoorProblem;
    [SerializeField] private AudioClip doorBreakSFX;
    [SerializeField] private AudioClip iDontLikeThis;
    [SerializeField] private AudioClip[] wtfIsHappening;
    [SerializeField] private AudioClip ohShitShitShit;
    [SerializeField] private AudioClip neckSnap;
    [SerializeField] private AudioClip neckSnap2;
    [SerializeField] private AudioClip gunshot;
    [SerializeField] private GameObject gunShotLight;
    [SerializeField] private AudioSource gunShotSource;
    [SerializeField] private AudioSource bangSounds;
    [SerializeField] private AudioSource ventBreakSFX;
    [SerializeField] private Animator lightingAnimator;
    [SerializeField] private GameObject emergencyLights;
    [SerializeField] private GameObject scp173;
    [SerializeField] private GameObject scp173_2;
    [SerializeField] private Animator gunLightLoop;

    [Header("PA System")]
    public bool canDoIntroPA = false;
    [SerializeField] private AudioSource paSystemSource;
    [SerializeField] private AudioClip[] scriptedLines;
    [SerializeField] private AudioClip scriptedVoiceLine5;

    [Header("Other Stuff")]
    [SerializeField] private AudioSource alarmSource;
    [SerializeField] private AudioSource announcementSource;
    [SerializeField] private AudioClip breachAnnouncement;
    [SerializeField] private GameObject cont173Intro;
    [SerializeField] private GameObject cont173;
    [SerializeField] private GameObject cont173Lighting;

    [Header("Electricians But Actually Dumb Retards")]
    [SerializeField] private Animator electricianAnimator;
    [SerializeField] private Animator electricianAnimator2;
    [SerializeField] private AudioSource breakerExplode;
    [SerializeField] private Animator lightsAnimator;
    [SerializeField] private GameObject expodeVFX;
    [SerializeField] private float time1;
    [SerializeField] private float time2;
    [SerializeField] private float time3;

    [Header("SCFranklin And Ulgrin Escape")]
    [SerializeField] private NPC_Puppet scFranklinPostBreach;
    [SerializeField] private NPC_Puppet ulgrinPostBreach;
    [SerializeField] private AudioClip ufEvnt_01;
    [SerializeField] private AudioClip ufEvnt_02;
    [SerializeField] private Door eventDoor;
    [SerializeField] private GameObject scpOneSevenThreeOhThree;
    [SerializeField] private Animator ContLightingFlicker;
    [SerializeField] private AudioSource lightFlickerSource;
    [SerializeField] private AudioClip[] lightFlickerSounds;

    [Header("Skip Intro")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform skipIntroTransform;
    [SerializeField] private AudioReverbZone playerReverb;
    [SerializeField] private GameObject emergenctTeleportTrigger;
    //[SerializeField] private GameObject roomRenderer;

    private void Start()
    {
        if (GameSettings.Instance != null)
        {
            enablePartOne = !GameSettings.Instance.skipIntro;
        }
        else
        {
            enablePartOne = true;
        }

        if (!enablePartOne) return;

        vibingGuardSource.clip = vibingGuardMusic[Random.Range(0, vibingGuardMusic.Length)];
        vibingGuardSource.Play();

        StartCoroutine(IntroSequencePartOne());
        StartCoroutine(PASystem());
    }

    public void UnloadArea(GameObject area) => Destroy(area);

    public void EscortReady() => readyForEscort = true;

    public void Approach173() => readyForApproach173 = true;

    public void StartPartTwo() => StartCoroutine(IntroSequencePartTwo());
    public void StartElecricianEvent() => StartCoroutine(ElectricianEvent());
    public void StartFranklinUlgrinEvent() => StartCoroutine(FranklinUlgrinEvent());

    public void FranklinWalk() => StartCoroutine(FranklinWalksToDoor());

    public void SkipIntro()
    {
        cont173Intro.SetActive(false);
        cont173.SetActive(true);
        announcementSource.clip = breachAnnouncement;
        announcementSource.Play();
        alarmSource.Play();
        playerReverb.gameObject.SetActive(true);
        playerController.transform.position = skipIntroTransform.position;
        //roomRenderer.SetActive(true);
        generatedMapParent.SetActive(true);
        cont173Lighting.SetActive(true);
        AmbienceController.Instance.ChangeZone(1);
        StartCoroutine(BringTheFellow());
        StartCoroutine(SkipIntroShakes());
    }

    public void GuardSpeech()
    {
        // Play a random speech from the ulgrin speech array, then find the matching one in the other guard array and play that.
        if (guard01.isActiveAndEnabled && guard02.isActiveAndEnabled)
        {
            int randomIndex = Random.Range(0, ulgrinSpeeches.Length);
            guard01.ToggleLookAtCamera(false);
            guard02.ToggleLookAtCamera(false);
            guard01.Say(ulgrinSpeeches[randomIndex]);
            guard02.Say(otherGuardSpeeches[randomIndex]);
        }
    }

    private IEnumerator PASystem()
    {
        while (canDoIntroPA)
        {
            yield return new WaitForSeconds(Random.Range(20, 30));
            int randomIndex = Random.Range(0, scriptedLines.Length);
            paSystemSource.clip = scriptedLines[randomIndex];

            if (canDoIntroPA)
                paSystemSource.Play();
        }
    }

    private IEnumerator ElectricianEvent()
    {
        electricianAnimator2.Play("Electrician01");
        yield return new WaitForSeconds(time1);
        electricianAnimator.Play("Electrician02");
        yield return new WaitForSeconds(time2);
        electricianAnimator2.SetBool("Elec1Touch", true);
        yield return new WaitForSeconds(time3);
        expodeVFX.SetActive(true);
        lightsAnimator.Play("IntroConnectorLightFlicker");
        GlobalCameraShake.Instance.ShakeCamera(0.5f, 0f, 0.3f);
        breakerExplode.Play();
        electricianAnimator2.enabled = false;
        electricianAnimator.SetBool("Elec2Look", true);
        yield return new WaitForSeconds(0.5f);
        expodeVFX.SetActive(false);
    }

    private IEnumerator FranklinWalksToDoor()
    {
        walkingFranklin.MoveAgent(scFranklinp1.transform);
        yield return new WaitForSeconds(7f);
        if (scFranklinDoor1.activeSelf)
        {
            Door door = scFranklinDoor1.GetComponent<Door>();
            door.OpenDoor();
            walkingFranklin.MoveAgent(scFranklinp2.transform);
        }
        yield return new WaitForSeconds(4f);
        if (scFranklinDoor1.activeSelf)
        {
            Door door = scFranklinDoor1.GetComponent<Door>();
            door.CloseDoor();
        }
        yield return new WaitForSeconds(1);
        walkingFranklin.gameObject.SetActive(false);
    }

    private IEnumerator FranklinUlgrinEvent()
    {
        eventDoor.OpenDoor();
        scFranklinPostBreach.Say(ufEvnt_01);
        ulgrinPostBreach.Say(ufEvnt_02);
        yield return new WaitForSeconds(0.9f);
        ulgrinPostBreach.PlayAnimationConditional("franklinUlgrinEvent");
        yield return new WaitForSeconds(0.5f);
        scFranklinPostBreach.PlayAnimationConditional("franklinWalkBack");
        yield return new WaitForSeconds(1.6f);
        ulgrinPostBreach.PlayAnimationConditional("franklinUlgrinWalkBack");
        StartCoroutine(FranklinUlgrin173Event());
        yield return new WaitForSeconds(9f);
        eventDoor.CloseDoor();
        yield return new WaitForSeconds(1f);
        ulgrinPostBreach.gameObject.SetActive(false);
        scFranklinPostBreach.gameObject.SetActive(false);
    }

    private IEnumerator FranklinUlgrin173Event()
    {
        yield return new WaitForSeconds(3.9f);
        ContLightingFlicker.SetTrigger("LightFlicker");
        lightFlickerSource.PlayOneShot(lightFlickerSounds[Random.Range(0, lightFlickerSounds.Length)]);
        yield return new WaitForSeconds(0.1f);
        scpOneSevenThreeOhThree.transform.position += new Vector3(0, 0, 8.5f);
        yield return new WaitForSeconds(5.5f);
        lightFlickerSource.PlayOneShot(lightFlickerSounds[Random.Range(0, lightFlickerSounds.Length)]);
        ContLightingFlicker.SetTrigger("LightFlicker");
        yield return new WaitForSeconds(0.1f);
        scpOneSevenThreeOhThree.SetActive(false);
    }

    IEnumerator IntroSequencePartOne()
    {
        yield return new WaitForSeconds(5f);
        beforeDoorOpenAudio.Play();
        yield return new WaitForSeconds(11f);
        cellDoor.GetComponent<Door>().OpenDoor();
        guard01.Say(exitCell);
        StartCoroutine(RefuseExitCellCheck());
        yield return new WaitUntil(() => readyForEscort);
        guard01.Say(escortBegin[Random.Range(0, escortBegin.Length)]);
        yield return new WaitForSeconds(13f);
        canDoIntroPA = true;
        yield return new WaitForSeconds(7);
        GuardSpeech();
    }

    private IEnumerator RefuseExitCellCheck()
    {
        yield return new WaitForSeconds(10f);
        if (!readyForEscort)
            guard01.Say(refuseExitCell[Random.Range(0, refuseExitCell.Length)]);
        yield return new WaitForSeconds(10f);
        if (!readyForEscort)
            guard01.Say(refuseExitGas[0]);
        yield return new WaitForSeconds(7f);
        if (!readyForEscort)
            cellDoor.GetComponent<Door>().CloseDoor();
        if (!readyForEscort)
            MusicPlayer.Instance.ChangeMusic(refuseToCooperateMusic);
        yield return new WaitForSeconds(1f);
        StartCoroutine(RefuseExitCellKill());
    }

    private IEnumerator RefuseExitCellKill()
    {
        if (!readyForEscort)
        {
            canDoIntroPA = false;
            yield return new WaitForSeconds(3f);
            cellLights[0].SetActive(false);
            cellLights[1].SetActive(true);
            autoVoxKillSource.Play();
            yield return new WaitForSeconds(18);
            gasValvlesOpen.Play();
            gas[0].SetActive(true);
            gas[1].SetActive(true);
            yield return new WaitForSeconds(5);
            GameManager.Instance.KillPlayer();
        }
    }

    IEnumerator IntroSequencePartTwo()
    {
        canDoIntroPA = false;
        yield return new WaitForSeconds(2f);
        scFranklin.PlayAnimationConditional("pushButton");
        yield return new WaitForSeconds(2f);
        dclass2.ToggleLookAtCamera(false);
        contDoor.OpenDoor();
        yield return new WaitForSeconds(3f);
        scFranklin.Say(announcementEnter);
        yield return new WaitForSeconds(5f);
        dclass1.SetAnimatorSpeed(1.3f);
        dclass1.MoveAgent(node1);
        yield return new WaitForSeconds(0.5f);
        dclass2.SetAnimatorSpeed(1f);
        dclass2.MoveAgent(node2);
        yield return new WaitForSeconds(5f);
        introTrigger02.SetActive(true);
        yield return new WaitUntil(() => readyForApproach173);
        yield return new WaitForSeconds(2f);
        scFranklin.Say(announcementApproach173);
        yield return new WaitForSeconds(3f);
        dclass2.MoveAgent(node3);
        yield return new WaitForSeconds(4f);
        otherGuy.SetActive(false);
        scFranklin.PlayAnimationConditional("BecomeNervous");
        bangSounds.clip = doorBreakSFX;
        bangSounds.Play();
        contDoor.OpenDoor();
        yield return new WaitForSeconds(0.5f);
        scFranklin.Say(announcementDoorProblem);
        yield return new WaitForSeconds(3f);
        GlobalCameraShake.Instance.ShakeCamera(0f, 0.03f, 10f);
        yield return new WaitForSeconds(2f);
        dclass1.Say(iDontLikeThis);
        yield return new WaitForSeconds(3.5f);
        dclass2.PlayAnimationConditional("walkBackScared");
        yield return new WaitForSeconds(1f);
        guardGuy.Say(wtfIsHappening[Random.Range(0, wtfIsHappening.Length)]);
        yield return new WaitForSeconds(2.5f);
        GlobalCameraShake.Instance.ShakeCamera(0.2f, 0f, 3f);
        lightingAnimator.Play("IntroLightsFlicker");
        dclass2.Say(neckSnap);
        yield return new WaitForSeconds(0.3f);
        scp173.transform.position = scpNode1.position;
        scp173.transform.rotation = scpNode1.rotation;
        yield return new WaitForSeconds(0.3f);
        dclass2.PlayAnimationConditional("death01");
        dclass1.PlayAnimationConditional("stumbleBackwards");
        yield return new WaitForSeconds(1f);
        lightingAnimator.Play("IntroLightsFlicker");
        yield return new WaitForSeconds(0.3f);
        scp173.transform.position = scpNode2.position;
        scp173.transform.rotation = scpNode2.rotation;
        dclass1.PlayAnimationConditional("death02");
        dclass1.Say(neckSnap2);
        yield return new WaitForSeconds(2f);
        bangSounds.Play();
        GlobalCameraShake.Instance.ShakeCamera(0.2f, 0f, 2f);
        lightingAnimator.Play("IntroLightsFlicker");
        yield return new WaitForSeconds(0.3f);
        guardGuy.transform.position = scpNode3.position;
        guardGuy.transform.rotation = scpNode3.rotation;
        scp173.SetActive(false);
        scp173_2.SetActive(true); // Use a separate SCP-173 for the upper part.
        yield return new WaitForSeconds(0.5f);
        lightingAnimator.gameObject.SetActive(false);
        emergencyLights.SetActive(true);
        guardGuy.Say(ohShitShitShit);
        yield return new WaitForSeconds(1.7f);
        StartCoroutine(IntroGunshots());
        yield return new WaitForSeconds(1.5f);
        bangSounds.Play();
        emergencyLights.SetActive(false);
        scp173_2.SetActive(false);
        gunShotLight.gameObject.SetActive(false);
        guardGuy.gameObject.SetActive(false);
        alarmSource.Play();
        announcementSource.clip = breachAnnouncement;
        announcementSource.Play();
        ventBreakSFX.Play();
        GlobalCameraShake.Instance.ShakeCamera(0.2f, 0f, 2f);
        cont173Intro.SetActive(false);
        cont173.SetActive(true);
        yield return new WaitForSeconds(3);
        cont173Lighting.SetActive(true);
        StartCoroutine(DelayedAnnouncement(9f));
        generatedMapParent.SetActive(true);
        //roomRenderer.SetActive(true);
        AmbienceController.Instance.ChangeZone(1);
    }

    IEnumerator IntroGunshots()
    {
        gunShotLight.gameObject.SetActive(true);

        for (int i = 0; i < 27; i++)
        {
            yield return new WaitForSeconds(0.05f);
            gunShotSource.PlayOneShot(gunshot);
        }
    }

    // Do it twice to be 100% sure, because even after the delay the player will still sometimes fall through the floor.
    IEnumerator BringTheFellow()
    {
        yield return new WaitForSeconds(0.01f);
        playerController.transform.position = skipIntroTransform.position;
        yield return new WaitForSeconds(0.01f);
        playerController.transform.position = skipIntroTransform.position;
    }

    // Unified coroutine for delayed camera shakes and announcement playback.
    private IEnumerator DelayedAnnouncement(float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);
        GlobalCameraShake.Instance.ShakeCamera(0.2f, 0f, 4f);
        yield return new WaitForSeconds(37.5f);
        GlobalCameraShake.Instance.ShakeCamera(0.2f, 0f, 4f);
        yield return new WaitForSeconds(10f);
        paSystemSource.clip = scriptedVoiceLine5;
        paSystemSource.Play();
        yield return new WaitForSeconds(39f);
        GlobalCameraShake.Instance.ShakeCamera(0.1f, 0f, 5f);
    }

    public IEnumerator SkipIntroShakes()
    {
        // Different initial delay for skip intro shakes.
        yield return new WaitForSeconds(11f);
        GlobalCameraShake.Instance.ShakeCamera(0.2f, 0f, 4f);
        yield return new WaitForSeconds(37.5f);
        GlobalCameraShake.Instance.ShakeCamera(0.2f, 0f, 4f);
        yield return new WaitForSeconds(10f);
        paSystemSource.clip = scriptedVoiceLine5;
        paSystemSource.Play();
        yield return new WaitForSeconds(39f);
        GlobalCameraShake.Instance.ShakeCamera(0.1f, 0f, 5f);
    }
}
