using System.Collections;
using UnityEngine;

namespace scpcbr {
    public class GameManager : MonoBehaviour {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameObject titleCardPrefab;
        [SerializeField] private Transform uiParent;

        public bool testPlayerSpawn;
        public bool testTitleCard;

        [Header("Global Settings")]
        public bool menusPauseGame;

        [Header("Player References")]
        public GameObject playerPrefab;

        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start() {
            if (testPlayerSpawn) {
                StartCoroutine(SpawnPlayerAfterime(5));
            }
            if (testTitleCard) {
                StartCoroutine(TestTitleCardAfterime(5));
            }
        }

        public void PlacePlayerInWorld(Vector3 spawnPos) {
            if (playerPrefab == null) return;
            Instance.playerPrefab = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        }

        public void RemoteChangeMusic(string trackName) {
            MusicPlayer.Instance.ChangeMusic(trackName);
        }

        public void PauseGameToggle() {
            bool isPaused = Time.timeScale == 0.0f;

            AudioListener.pause = !isPaused;
            Time.timeScale = isPaused ? 1.0f : 0.0f;
        }

        public void ShowZoneTitleCard() {
            GameObject card = Instantiate(titleCardPrefab, uiParent);
            //var text = card.GetComponentInChildren<TMP_Text>();
            //text.text = areaName;
            Destroy(card, 5f);
        }

        private IEnumerator SpawnPlayerAfterime(int time) {
            yield return new WaitForSeconds(time);
            PlacePlayerInWorld(new Vector3(7.3f, 1, 0));
        }

        private IEnumerator TestTitleCardAfterime(int time) {
            yield return new WaitForSeconds(time);
        }
    }
}