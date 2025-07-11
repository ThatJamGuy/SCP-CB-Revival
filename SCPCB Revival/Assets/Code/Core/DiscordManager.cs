using UnityEngine;

public class DiscordManager : MonoBehaviour {
    Discord.Discord discord;

    private void Start() {
        discord = new Discord.Discord(1357936248869748845, (ulong)Discord.CreateFlags.NoRequireDiscord);
        ChangeActivity();
    }

    private void OnDisable() {
        discord.Dispose();
    }

    private void Update() {
        discord.RunCallbacks();
    }

    public void ChangeActivity() {
        var activityManager = discord.GetActivityManager();
        var activity = new Discord.Activity {
            Details = "Doing something I think"
        };
        activityManager.UpdateActivity(activity, (res) => {
            Debug.Log("Activity updated!");
        });
    }
}