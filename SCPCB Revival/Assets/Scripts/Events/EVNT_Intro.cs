using System.Collections;
using UnityEngine;

public class EVNT_Intro : MonoBehaviour
{
    public bool readyForEscort;

    [SerializeField] private GameObject cellDoor;
    [SerializeField] private NPC_Puppet guard01;
    [SerializeField] private NPC_Puppet guard02;
    [SerializeField] private NPC_Puppet guard03;
    [SerializeField] private AudioClip[] vibingGuardMusic;
    [SerializeField] private AudioClip exitCell;
    [SerializeField] private AudioClip[] escortBegin;

    [Header("Audio")]
    [SerializeField] private AudioSource beforeDoorOpenAudio;
    [SerializeField] private AudioSource vibingGuardSource;

    private void Start()
    {
        guard03.PlayAnimation("GuardLeaning01");
        vibingGuardSource.clip = vibingGuardMusic[Random.Range(0, vibingGuardMusic.Length)];
        vibingGuardSource.Play();

        StartCoroutine(IntroSequencePartOne());
    }

    public void EscortReady() => readyForEscort = true;

    IEnumerator IntroSequencePartOne()
    {
        yield return new WaitForSeconds(5f);
        beforeDoorOpenAudio.Play();
        yield return new WaitForSeconds(11f);
        cellDoor.GetComponent<Door>().OpenDoor();
        guard01.Say(exitCell);
        yield return new WaitUntil(() => readyForEscort);
        guard01.Say(escortBegin[Random.Range(0, escortBegin.Length)]);
        //yield return new WaitForSeconds(7);
        //guard01.MoveToNodes();
        //yield return new WaitForSeconds(5);
        //guard02.MoveToNodes();
    }
}