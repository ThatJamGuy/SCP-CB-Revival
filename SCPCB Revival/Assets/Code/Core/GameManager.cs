using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool testPlayerSpawn;

    [Header("Global Settings")]
    public bool menusPauseGame;

    [Header("Player References")]
    public GameObject playerPrefab;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() {
        if(testPlayerSpawn) {
            StartCoroutine(SpawnPlayerAfterime(5));
        }
    }

    public void PlacePlayerInWorld(Vector3 spawnPos) {
        if (playerPrefab == null) return;
        Instance.playerPrefab = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }

    public void PauseGameToggle() {
        bool isPaused = Time.timeScale == 0.0f;

        AudioListener.pause = !isPaused;
        Time.timeScale = isPaused ? 1.0f : 0.0f;
    }

    private IEnumerator SpawnPlayerAfterime(int time) {
        yield return new WaitForSeconds(time);
        PlacePlayerInWorld(new Vector3(7.3f, 1, 0));
    }
}