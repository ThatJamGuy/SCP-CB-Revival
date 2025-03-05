using System.Collections;
using UnityEngine;

public class EVNT_Intro : MonoBehaviour
{
    [SerializeField] private bool enablePartOne;

    [Header("Intro Part One")]

    public bool readyForEscort;
    public bool reachedSector2;
    public bool reachedSector3;

    [SerializeField] private GameObject cellDoor;
    [SerializeField] private NPC_Puppet guard01;
    [SerializeField] private NPC_Puppet guard02;
    [SerializeField] private NPC_Puppet classDEscortGuard01;
    [SerializeField] private NPC_Puppet classDEscortGuard02;
    [SerializeField] private AudioClip[] vibingGuardMusic;
    [SerializeField] private AudioClip exitCell;
    [SerializeField] private AudioClip[] escortBegin;

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

    [Header("Other Stuff")]
    [SerializeField] private AudioSource alarmSource;
    [SerializeField] private AudioSource announcementSource;
    [SerializeField] private AudioClip breachAnnouncement;
    [SerializeField] private GameObject cont173Intro;
    [SerializeField] private GameObject cont173;
    [SerializeField] private GameObject cont173Lighting;
    [SerializeField] private GameObject generatedMap;

    [Header("Skip Intro")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform skipIntroTransform;
    [SerializeField] private AudioReverbZone playerReverb;
    [SerializeField] private GameObject emergenctTeleportTrigger;
    [SerializeField] private GameObject roomRenderer;

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
    }

    public void UnloadArea(GameObject area) => Destroy(area);

    public void EscortReady() => readyForEscort = true;

    public void Approach173() => readyForApproach173 = true;

    public void StartPartTwo() => StartCoroutine(IntroSequencePartTwo());

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
        StartCoroutine(BringTheFellow());
        StartCoroutine(SkipIntroShakes());
    }

    public void GuardSpeech()
    {
        // Play a random speech from the ulgrin speech array, then find the matching one in the other guard array and play that
        if (guard01.isActiveAndEnabled && guard02.isActiveAndEnabled)
        {
            int randomIndex = Random.Range(0, ulgrinSpeeches.Length);
            guard01.ToggleLookAtCamera(false);
            guard02.ToggleLookAtCamera(false);
            guard01.Say(ulgrinSpeeches[randomIndex]);
            guard02.Say(otherGuardSpeeches[randomIndex]);
        }
    }

    IEnumerator FranklinWalksToDoor()
    {
        walkingFranklin.MoveAgent(scFranklinp1.transform);
        yield return new WaitForSeconds(7f);
        scFranklinDoor1.GetComponent<Door>().OpenDoor();
        walkingFranklin.MoveAgent(scFranklinp2.transform);
        yield return new WaitForSeconds(4f);
        if (scFranklinDoor1.gameObject.activeSelf)
        {
            scFranklinDoor1.GetComponent<Door>().CloseDoor();
        }
        yield return new WaitForSeconds(1);
        walkingFranklin.gameObject.SetActive(false);
    }

    IEnumerator IntroSequencePartOne()
    {
        yield return new WaitForSeconds(5f);
        beforeDoorOpenAudio.Play();
        yield return new WaitForSeconds(11f);
        cellDoor.GetComponent<Door>().OpenDoor();
        guard01.Say(exitCell);
        yield return new WaitUntil(() => readyForEscort);
        guard01.Say(escortBegin[Random.Range(0, escortBegin.Length)]);
        yield return new WaitForSeconds(20);
        GuardSpeech();
    }

    IEnumerator IntroSequencePartTwo()
    {
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
        scp173_2.SetActive(true); // Grab a seperate SCP-173 for the upper bit cuz the normal one runs to the corner isntead of going up. :/
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
        StartCoroutine(IntroBreachAnnouncementShakes());
        generatedMap.SetActive(true);
        roomRenderer.SetActive(true);
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

    IEnumerator BringTheFellow()
    {
        yield return new WaitForSeconds(0.01f);
        playerController.transform.position = skipIntroTransform.position;
    }

    IEnumerator IntroBreachAnnouncementShakes()
    {
        yield return new WaitForSeconds(9f);
        GlobalCameraShake.Instance.ShakeCamera(0.2f, 0f, 4f);
        yield return new WaitForSeconds(37.5f);
        GlobalCameraShake.Instance.ShakeCamera(0.2f, 0f, 4f);
    }

    public IEnumerator SkipIntroShakes()
    {
        roomRenderer.SetActive(true);
        generatedMap.SetActive(true);
        cont173Lighting.SetActive(true);
        yield return new WaitForSeconds(11.5f);
        GlobalCameraShake.Instance.ShakeCamera(0.2f, 0f, 4f);
        yield return new WaitForSeconds(37.5f);
        GlobalCameraShake.Instance.ShakeCamera(0.2f, 0f, 4f);
    }
}