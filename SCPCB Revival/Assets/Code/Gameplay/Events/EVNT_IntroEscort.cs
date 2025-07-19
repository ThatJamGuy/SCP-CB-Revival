using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace scpcbr {
    public class EVNT_IntroEscort : MonoBehaviour {
        [SerializeField] private int timeUntilBeforeCellOpen = 10;
        [SerializeField] private int timeUntilDoorOpens = 7;
        [SerializeField] private Door cellDoor;
        [SerializeField] private float franklingDoorCloseTime = 10;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip stepOutCell;
        [SerializeField] private AudioClip[] followMe;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource beforeCellOpenSource;
        [SerializeField] private AudioSource ulgrinVoiceSource;
        [SerializeField] private AudioSource thomasVoiceSource;

        [Header("Escort Nodes")]
        [SerializeField] private Transform[] escortNodes;
        [SerializeField] private float nodeArrivalThreshold = 1f;

        [Header("References")]
        [SerializeField] private Door[] doorsToOpen;
        [SerializeField] private GameObject ulgrin;
        [SerializeField] private GameObject thomas;

        private NPC_RootMotionAgent ulgrinAgent;
        private NPC_RootMotionAgent thomasAgent;

        private bool isEscorting = false;
        private int currentNodeIndex = 0;

        private void Start() {
            ulgrinAgent = ulgrin.GetComponent<NPC_RootMotionAgent>();
            thomasAgent = thomas.GetComponent<NPC_RootMotionAgent>();
            StartCoroutine(OpenDoorAfterDelay());
        }

        public void TriggerEscort() {
            ulgrinVoiceSource.clip = followMe[Random.Range(0, followMe.Length)];
            ulgrinVoiceSource.Play();
            StartCoroutine(EscortPlayer());
        }

        private void Update() {
            if (!isEscorting) return;

            ulgrinAgent.WalkToPosition(escortNodes[currentNodeIndex].position);
            thomasAgent.WalkToPosition(ulgrin.transform.position);

            float dist = Vector3.Distance(ulgrin.transform.position, escortNodes[currentNodeIndex].position);
            if (dist <= nodeArrivalThreshold) {
                if (currentNodeIndex < escortNodes.Length - 1) {
                    currentNodeIndex++;
                }
                else {
                    isEscorting = false;
                }
            }
        }

        public void TriggerFranklinWalkInOffice() {
            StartCoroutine(franklinWalkInOffice());
        }

        private IEnumerator OpenDoorAfterDelay() {
            yield return new WaitForSeconds(timeUntilBeforeCellOpen);
            beforeCellOpenSource.Play();
            yield return new WaitForSeconds(timeUntilDoorOpens);
            cellDoor.OpenDoor();
            ulgrinVoiceSource.clip = stepOutCell;
            ulgrinVoiceSource.Play();
            StartCoroutine(EscortPlayer());
        }

        private IEnumerator EscortPlayer() {
            yield return new WaitForSeconds(15);
            isEscorting = true;
            yield return new WaitForSeconds(13);
            doorsToOpen[0].OpenDoor();
            yield return new WaitForSeconds(10);
            doorsToOpen[1].OpenDoor();
        }

        private IEnumerator franklinWalkInOffice() {
            yield return new WaitForSeconds(franklingDoorCloseTime);
            doorsToOpen[2].CloseDoor();
        }
    }
}