using scpcbr;
using System.Collections;
using UnityEngine;

namespace scpcbr {
    public class EVNT_PlayerWakeup : MonoBehaviour {
        [SerializeField] private GameObject evntObjs;
        [SerializeField] private GameObject player;
        [SerializeField] private float wakeUpDelay;

        private void Start() {
            MusicPlayer.Instance.StartMusicByName("Intro");
            StartCoroutine(WakeUp());
        }

        private IEnumerator WakeUp() {
            yield return new WaitForSeconds(wakeUpDelay);
            player.SetActive(true);
            evntObjs.SetActive(false);
        }
    }
}