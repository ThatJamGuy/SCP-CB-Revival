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
    [SerializeField] private NPC_Puppet classDEscortGuard01;
    [SerializeField] private NPC_Puppet classDEscortGuard02;
    [SerializeField] private AudioClip[] vibingGuardMusic;
    [SerializeField] private AudioClip exitCell;
    [SerializeField] private AudioClip[] escortBegin;

    [Header("Audio")]
    [SerializeField] private AudioSource beforeDoorOpenAudio;
    [SerializeField] private AudioSource vibingGuardSource;

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

    private bool readyForTwoPointFive;

    private void Start()
    {
        if(!enablePartOne) return;

        vibingGuardSource.clip = vibingGuardMusic[Random.Range(0, vibingGuardMusic.Length)];
        vibingGuardSource.Play();

        StartCoroutine(IntroSequencePartOne());
    }

    public void UnloadArea(GameObject area) => Destroy(area);

    public void EscortReady() => readyForEscort = true;

    public void Approach173() => readyForApproach173 = true;

    public void StartPartTwo() => StartCoroutine(IntroSequencePartTwo());

    public void StartPartTwoPointFive() => readyForTwoPointFive = true;

    IEnumerator IntroSequencePartOne()
    {
        yield return new WaitForSeconds(5f);
        beforeDoorOpenAudio.Play();
        yield return new WaitForSeconds(11f);
        cellDoor.GetComponent<Door>().OpenDoor();
        guard01.Say(exitCell);
        yield return new WaitUntil(() => readyForEscort);
        guard01.Say(escortBegin[Random.Range(0, escortBegin.Length)]);
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
        yield return new WaitForSeconds(5f);
        dclass1.Say(iDontLikeThis);
        yield return new WaitForSeconds(3.5f);
        dclass2.PlayAnimationConditional("walkBackScared");
        yield return new WaitForSeconds(3.5f);
        lightingAnimator.Play("IntroLightsFlicker");
        guardGuy.Say(wtfIsHappening[Random.Range(0, wtfIsHappening.Length)]);
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
        yield return new WaitForSeconds(2f);
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
        yield return new WaitForSeconds(4);
        emergencyLights.SetActive(true);
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
}