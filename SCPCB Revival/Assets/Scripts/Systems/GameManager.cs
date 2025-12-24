using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance { get; private set; }

    [SerializeField] private ItemDatabaseSO itemDatabase;

    public bool isGamePaused = false;
    public bool scp106Active = false;
    public bool scp173ChasingPlayer = false;
    public bool scp173currentVisibleToPlayer = false;



    public void Awake() {
        instance = this;

        itemDatabase.Init();
    }

    private void Start() {
        DevConsole.Instance.Add<string>("spawnitem", id => {
            if (!ItemDatabase.TryGet(id, out var item)) {
                DevConsole.Instance.Log($"Unknown item: {id}", LogType.Error);
                return;
            }
            SpawnItem(item);
        });
    }

    public void PauseGame() {
        isGamePaused = true;
        Time.timeScale = 0f;
        AudioManager.instance.PauseGameAudio();
    }

    public void UnpauseGame() {
        isGamePaused = false;
        Time.timeScale = 1f;
        AudioManager.instance.UnpauseGameAudio();
    }

    public void ShowDeathScreen(string causeOfDeath) {
        PlayerAccessor.instance.DisablePlayerInputs(true);
        PlayerAccessor.instance.isDead = true;
        PlayerAccessor.instance.isMoving = false;
        //AudioManager.instance.StopMusic();
        MusicManager.instance.SetMusicState(MusicState.LCZ);
        CanvasInstance.instance.deathMenu.SetActive(true);
        CanvasInstance.instance.deathMenuDeathCauseText.text = causeOfDeath;
    }

    public void SpawnItem(ItemData item) {
        Instantiate(item.worldPrefab, PlayerAccessor.instance.transform.position + PlayerAccessor.instance.transform.forward * 2f, Quaternion.identity);
    }
}