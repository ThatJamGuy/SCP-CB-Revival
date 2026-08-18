using Discord.Sdk;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevivalRuntimeEngine : MonoBehaviour {
    public static RevivalRuntimeEngine Instance { get; private set; }
    public static SettingsData SettingsData { get; set; }
    private static readonly HashSet<string> obtainedAchievementNames = new HashSet<string>();

    public static int TotalAchievements { get; private set; }
    public static int ObtainedAchievementsCount => obtainedAchievementNames.Count;

    [Header("Default RPC Settings")]
    [SerializeField] private string details = "";
    [SerializeField] private string state = "";

    [Header("Achievement Settings")]
    [SerializeField] private AchievementData[] achievements;
    [SerializeField] private EventReference achievementUnlockStinger;
    [SerializeField] private GameObject achievementItemPrefab;
    [SerializeField] private Transform achievementContainer;

    [Header("Canvas References")]
    public GameObject achievementsScreen;
    public GameObject optionsScreen;

    private ulong startTimestamp;

    private const ulong APPLICATION_ID = 1450277986178830448;
    private const string SETTINGS_FILE_NAME = "settings.json";
    private const string ACHIEVMENTS_FILE_NAME = "achievements.json";

    private List<Transform> camTransforms = new List<Transform>();
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Client client;
    private Coroutine shakeCoroutine;
    private AchievementFile achievementFileData;
    private GameObject currentAchievementItem = null;

    private float shakeIntensity;

    #region Unity Callbacks

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        SettingsData = DataSaver.Load<SettingsData>(SETTINGS_FILE_NAME);
        client = new Client();

        // Set the log utility to the OnLog method and set the application ID to applicationID. Then update RPC. 
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

        achievementFileData = DataSaver.Load<AchievementFile>(ACHIEVMENTS_FILE_NAME);
        TotalAchievements = achievements.Length;

        // Fill the obtainedAchievementNames hashset with achievement names from the achievements file
        if (achievementFileData?.obtainedAchievementIDs != null) {
            foreach (string achievementID in achievementFileData.obtainedAchievementIDs) {
                if (!string.IsNullOrEmpty(achievementID)) obtainedAchievementNames.Add(achievementID);
            }
        }

        // Create the visual representations of each achievement and assign it's data accordingly
        foreach (AchievementData achievementData in achievements) {
            currentAchievementItem = Instantiate(achievementItemPrefab, achievementContainer);
            currentAchievementItem.GetComponent<AchivementDisplay>().DisplayAchievementValues(achievementData);
        }
    }

    #endregion

    #region Private Methods

    private static void OnUpdateRichPresence(ClientResult result) {
        return;
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

    private void SaveAchievementsToFile() {
        string[] names = new string[obtainedAchievementNames.Count];
        obtainedAchievementNames.CopyTo(names);

        achievementFileData = new AchievementFile { obtainedAchievementIDs = names };
        DataSaver.Save(achievementFileData, ACHIEVMENTS_FILE_NAME);
    }

    private void ApplyShake() {
        foreach (var cam in camTransforms) {
            if (cam == null || !originalPositions.ContainsKey(cam)) continue;
            cam.localPosition = originalPositions[cam] + (Vector3)Random.insideUnitCircle * shakeIntensity;
        }
    }

    #endregion

    #region Private Coroutines

    private IEnumerator ShakeRoutine(float startIntensity, float endIntensity, float duration) {
        foreach (var cam in camTransforms) {
            if (cam != null)
                originalPositions[cam] = cam.localPosition;
        }
        float elapsed = 0f;

        while (elapsed < duration) {
            shakeIntensity = Mathf.Lerp(startIntensity, endIntensity, elapsed / duration);
            ApplyShake();
            elapsed += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(FadeOutShake());
    }

    private IEnumerator FadeOutShake() {
        float fadeDuration = 0.5f;
        float startShake = shakeIntensity;
        float elapsed = 0f;

        while (elapsed < fadeDuration) {
            shakeIntensity = Mathf.Lerp(startShake, 0f, elapsed / fadeDuration);
            ApplyShake();
            elapsed += Time.deltaTime;
            yield return null;
        }
        shakeIntensity = 0f;
        foreach (var cam in camTransforms) {
            if (cam != null && originalPositions.ContainsKey(cam))
                cam.localPosition = originalPositions[cam];
        }
    }

    #endregion

    #region Public Methods

    public static void SaveSettingsData() => DataSaver.Save<SettingsData>(SettingsData, SETTINGS_FILE_NAME);

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

    /// <summary>
    /// Returns a true or false value based on if the defined achievement is unlocked.
    /// </summary>
    /// <param name="achievementIdentifier">ID of the achievement to check</param>
    /// <returns></returns>
    public bool AchievementUnlocked(string achievementIdentifier) {
        if (obtainedAchievementNames.Contains(achievementIdentifier)) return true;
        else return false;
    }

    /// <summary>
    /// Gives the player an achievement via that achievements identifier
    /// </summary>
    /// <param name="achievementIdentifier">Identifier for this achievement. (IE. "achv_914")</param>
    public void GiveAchievement(string achievementIdentifier) {
        foreach (var achievement in achievements) {
            if (SettingsData.consoleEnabled) {
                Debug.Log("<color=#ff0000>Tried to give you an achievement, but it looks like you have the console enabled!");
                return;
            }

            if (achievement.achievementIdentifier != achievementIdentifier) continue;
            if (obtainedAchievementNames.Contains(achievement.achievementIdentifier)) return;

            obtainedAchievementNames.Add(achievement.achievementIdentifier);
            SaveAchievementsToFile();

            CanvasInstance.Instance.achievementName.text = achievement.achievementName;
            CanvasInstance.Instance.achievementDesc.text = achievement.achievementDescription;
            CanvasInstance.Instance.achievementIcon.sprite = achievement.achievementIcon;
            CanvasInstance.Instance.HUD_AchievementPopup.Play("HUD_AchievementPopup");
            AudioManager.PlayOneShot(achievementUnlockStinger, transform.position);

            return;
        }

        Debug.LogWarning($"No achievement found with name: " + achievementIdentifier);
    }

    public void RegisterCamera(Transform cam) {
        if (!camTransforms.Contains(cam)) {
            camTransforms.Add(cam);
            originalPositions[cam] = cam.localPosition;
        }
    }

    public void UnregisterCamera(Transform cam) {
        if (camTransforms.Contains(cam)) {
            camTransforms.Remove(cam);
            originalPositions.Remove(cam);
        }
    }

    public void ShakeCamera(float startIntensity, float endIntensity, float duration) {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine(startIntensity, endIntensity, duration));
    }

    public static void ToggleAchievementsMenu(bool active) {
        Instance.achievementsScreen.SetActive(active);
    }

    public static void ToggleOptionsMenu(bool active) {
        Instance.optionsScreen.SetActive(active);
    }

    #endregion
}

[System.Serializable]
public class AchievementFile {
    public string[] obtainedAchievementIDs;
}