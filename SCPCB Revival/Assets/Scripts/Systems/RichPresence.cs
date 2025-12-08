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
        discord = new Discord.Discord(1357936248869748845, (ulong)Discord.CreateFlags.NoRequireDiscord);
        ChangeActivity("Doing something");
    }

    private void OnDisable() {
        discord.Dispose();
    }

    private void Update() {
        discord.RunCallbacks();
    }

    public void ChangeActivity(string details) {
        var activityManager = discord.GetActivityManager();
        var activity = new Discord.Activity {
            Details = details
        };
        activityManager.UpdateActivity(activity, (res) => {
            Debug.Log("Activity updated");
        });
    }
}