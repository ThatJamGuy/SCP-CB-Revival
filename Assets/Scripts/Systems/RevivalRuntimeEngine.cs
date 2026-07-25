using Discord.Sdk;
using UnityEngine;

/// <summary>
/// Master script to sit on the Global Things object.
/// Basically the functionality of a bunch of other scripts merged into one to clean
/// things up a bit.
/// </summary>
public class RevivalRuntimeEngine : MonoBehaviour {
    public static RevivalRuntimeEngine Instance { get; private set; }

    [Header("Default RPC Settings")]
    [SerializeField] private string details = "";
    [SerializeField] private string state = "";

    [Header("Canvas References")]
    public GameObject achievementsScreen;
    public GameObject optionsScreen;

    private Client client;

    private ulong startTimestamp;

    private const ulong APPLICATION_ID = 1450277986178830448;

    #region Unity Callbacks

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        client = new Client();

        // Set the log utility to the OnLog method and set the application ID to applicationID. Then update RPC.
        client.AddLogCallback(OnLog, LoggingSeverity.Error);
        client.SetApplicationId(APPLICATION_ID);
        UpdateRichPresence();
    }

    private void OnDestroy() {
        // Cleanup
        client.ClearRichPresence();
        client.Disconnect();
    }

    private void Start() {
        // Do some black magic shit to set the timestamp to the 0:00 mark
        startTimestamp = (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    #endregion

    #region Private Methods

    private static void OnLog(string message, LoggingSeverity severity) {
        Debug.Log($"Log: {severity} -  {message}");
    }

    private static void OnUpdateRichPresence(ClientResult result) {
        return;
        // If the RPC was properly updated, let the world know. Otherwise, I assume that user doesn't have discord.
        /*Debug.Log(result.Successful()
            ? "<color=purple>[Discord Social SDK]</color> Rich presence updated!"
            : $"<color=red>Failed to update rich presence {result.Error()}</color> Likely no Discord open.");*/
    }

    private void UpdateRichPresence() {
        var activity = new Activity();

        // Set the state to something like "Playing SCP - CB Revival, details, state"
        activity.SetType(ActivityTypes.Playing);
        activity.SetDetails(details);
        activity.SetState(state);

        // Set the timestamp back to 0:00 since we changed stuff
        var activityTimestamp = new ActivityTimestamps();
        activityTimestamp.SetStart(startTimestamp);
        activity.SetTimestamps(activityTimestamp);

        // Actually update the rich presence and run the method that prints the output to the console
        client.UpdateRichPresence(activity, OnUpdateRichPresence);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Publicly available method to change the discord RPC to whatever (USE WISELY, OTHER SCRIPTS KNOW THIS GUY)
    /// </summary>
    /// <param name="newDetails">New details the RPC will use (Subtext 1)</param>
    /// <param name="newState">New state of that detail the RPC will use (Subtext 2)</param>
    public void ChangeDiscordStatus(string newDetails, string newState = "") {
        // Set the details and state to the new ones given through this method
        details = newDetails;
        state = newState;

        // Update the rich presence so it shows properly
        UpdateRichPresence();
    }

    public static void ToggleAchievementsMenu(bool active) {
        Instance.achievementsScreen.SetActive(active);
    }

    public static void ToggleOptionsMenu(bool active) {
        Instance.optionsScreen.SetActive(active);
    }

    #endregion
}