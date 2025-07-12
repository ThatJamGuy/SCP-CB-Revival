using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace scpcbr {
    public class EVNT_IntroEscort : MonoBehaviour {
        [SerializeField] private int timeUntilBeforeCellOpen = 10;
        [SerializeField] private int timeUntilDoorOpens = 7;
        [SerializeField] private Door cellDoor;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip stepOutCell;
        [SerializeField] private AudioClip[] followMe;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource beforeCellOpenSource;
        [SerializeField] private AudioSource ulgrinVoiceSource;
        [SerializeField] private AudioSource thomasVoiceSource;

        [Header("Escprt Nodes")]
        [SerializeField] private Transform[] escortNodes;

        [Header("References")]
        [SerializeField] private NavMeshAgent ulgrinAgent;
        [SerializeField] private Animator ulgrinAnimator;

        private int currentNodeIndex;

        private void Start() {
            ulgrinAgent.updatePosition = false;
            ulgrinAgent.updateRotation = false;

            StartCoroutine(OpenDoorAfterDelay());
        }

        public void TriggerEscort() {
            ulgrinVoiceSource.clip = followMe[Random.Range(0, followMe.Length)];
            ulgrinVoiceSource.Play();
            StartCoroutine(EscortPlayer());
        }

        private IEnumerator OpenDoorAfterDelay() {
            yield return new WaitForSeconds(timeUntilBeforeCellOpen);
            beforeCellOpenSource.Play();
            yield return new WaitForSeconds(timeUntilDoorOpens);
            cellDoor.OpenDoor();
            ulgrinVoiceSource.clip = stepOutCell;
            ulgrinVoiceSource.Play();
        }

        private IEnumerator EscortPlayer() {
            yield return new WaitForSeconds(ulgrinVoiceSource.clip.length);
            currentNodeIndex = 0;
            MoveToNextNode();
        }

        private void MoveToNextNode() {
            if (currentNodeIndex >= escortNodes.Length) {
                ulgrinAgent.ResetPath();
                ulgrinAnimator.SetBool("IsMoving", false);
                return;
            }
            ulgrinAgent.SetDestination(escortNodes[currentNodeIndex].position);
            ulgrinAnimator.SetBool("IsMoving", true);
            currentNodeIndex++;
        }

        private void OnAnimatorMove() {
            if (!ulgrinAgent.hasPath) {
                ulgrinAnimator.SetBool("IsMoving", false);
                return;
            }
            ulgrinAgent.nextPosition = ulgrinAnimator.rootPosition;
            transform.position = ulgrinAgent.nextPosition;
            transform.rotation = ulgrinAnimator.rootRotation;
        }
    }
}