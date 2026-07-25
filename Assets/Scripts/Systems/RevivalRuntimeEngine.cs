using Discord.Sdk;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Master script to sit on the Global Things object.
/// Basically the functionality of a bunch of other scripts merged into one to clean
/// things up a bit.
/// </summary>
public class RevivalRuntimeEngine : MonoBehaviour {
    public static RevivalRuntimeEngine Instance { get; private set; }
    public static SettingsData SettingsData { get; private set; }
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

    [Header("Camera Shake Settings")]
    [SerializeField] private float noiseFrequency = 15f;
    [SerializeField] private float maxRotationDegrees = 3f;

    [Header("Canvas References")]
    public GameObject achievementsScreen;
    public GameObject optionsScreen;

    private ulong startTimestamp;

    private const ulong APPLICATION_ID = 1450277986178830448;
    private const string SETTINGS_FILE_NAME = "settings.json";
    private const string ACHIEVMENTS_FILE_NAME = "achievements.json";

    private static readonly HashSet<string> obtainedAchievementNames = new HashSet<string>();
    private readonly List<Transform> camTransforms = new List<Transform>();
    private readonly Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private readonly Dictionary<Transform, Quaternion> originalRotations = new Dictionary<Transform, Quaternion>();

    private Client client;
    private Coroutine activeShakeRoutine;
    private AchievementFile achievementFileData;
    private GameObject currentAchievementItem = null;

    private float shakeIntensity;
    private float shakeNoiseSeed;

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
        float t = Time.time * noiseFrequency;
        float offsetX = (Mathf.PerlinNoise(shakeNoiseSeed, t) - 0.5f) * 2f;
        float offsetY = (Mathf.PerlinNoise(shakeNoiseSeed + 1f, t) - 0.5f) * 2f;
        float offsetZ = (Mathf.PerlinNoise(shakeNoiseSeed + 2f, t) - 0.5f) * 2f;

        foreach (var cam in camTransforms) {
            if (cam == null || !originalPositions.ContainsKey(cam)) continue;

            Vector3 posOffset = new Vector3(offsetX, offsetY, 0f) * shakeIntensity;
            cam.localPosition = originalPositions[cam] + posOffset;

            float rotOffset = offsetZ * shakeIntensity * maxRotationDegrees;
            cam.localRotation = originalRotations[cam] * Quaternion.Euler(0f, 0f, rotOffset);
        }
    }

    #endregion

    #region Private Coroutines

    private IEnumerator ShakeRoutine(float startIntensity, float endIntensity, float duration) {
        float elapsed = 0f;

        while (elapsed < duration) {
            shakeIntensity = Mathf.Lerp(startIntensity, endIntensity, elapsed / duration);
            ApplyShake();
            elapsed += Time.deltaTime;
            yield return null;
        }

        activeShakeRoutine = StartCoroutine(FadeOutShake());
    }

    private IEnumerator FadeOutShake() {
        const float fadeDuration = 0.5f;
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
            if (cam == null) continue;
            if (originalPositions.ContainsKey(cam)) cam.localPosition = originalPositions[cam];
            if (originalRotations.ContainsKey(cam)) cam.localRotation = originalRotations[cam];
        }
        activeShakeRoutine = null;
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

        Debug.LogWarning($"No achievement found with name: '{achievementIdentifier}'");
    }

    public void RegisterCamera(Transform cam) {
        if (camTransforms.Contains(cam)) return;
        camTransforms.Add(cam);
        originalPositions[cam] = cam.localPosition;
        originalRotations[cam] = cam.localRotation;
    }

    public void UnregisterCamera(Transform cam) {
        camTransforms.Remove(cam);
        originalPositions.Remove(cam);
        originalRotations.Remove(cam);
    }

    public void ShakeCamera(float startIntensity, float endIntensity, float duration) {
        if (activeShakeRoutine != null) StopCoroutine(activeShakeRoutine);

        foreach (var cam in camTransforms) {
            if (cam == null) continue;
            originalPositions[cam] = cam.localPosition;
            originalRotations[cam] = cam.localRotation;
        }

        shakeNoiseSeed = Random.value * 1000f;
        activeShakeRoutine = StartCoroutine(ShakeRoutine(startIntensity, endIntensity, duration));
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