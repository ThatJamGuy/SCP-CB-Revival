using UnityEngine;

namespace scpcbr {
    public class DiscordManager : MonoBehaviour {
        public static DiscordManager Instance;
        
        Discord.Discord discord;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Instance = this;
                Destroy(this.gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

        private void Start() {
            discord = new Discord.Discord(1357936248869748845, (ulong)Discord.CreateFlags.NoRequireDiscord);
            ChangeActivity("In the main menu");
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
                Debug.Log("Activity updated!");
            });
        }
    }
}