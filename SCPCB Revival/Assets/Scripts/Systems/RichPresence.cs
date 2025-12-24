using UnityEngine;

public class RichPresence : MonoBehaviour {
    public static RichPresence instance;

    Discord.Discord discord;

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        try {
            discord = new Discord.Discord(1450277986178830448, (ulong)Discord.CreateFlags.NoRequireDiscord);
            ChangeActivity("In the main menu");
        }
        catch (System.Exception e) {
            Debug.LogError($"Discord initialization failed: {e.Message}");
        }
    }

    private void OnDisable() {
        discord.Dispose();
    }

    public void ChangeActivity(string state) { 
        var activityManager = discord.GetActivityManager();
        var activity = new Discord.Activity {
            State = state
        };
        activityManager.UpdateActivity(activity, (res) => {
            Debug.Log("Activity updated!");
        });
    }

    private void Update() {
        discord.RunCallbacks();
    }
}