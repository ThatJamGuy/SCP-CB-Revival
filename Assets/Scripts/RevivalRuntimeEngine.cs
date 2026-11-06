using Discord.Sdk;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// PURPOSE: Main runtime script for the game. It persists throughout the ENTIRE lifecycle of the game and is responsible
// for various reusable functions across scenes. This includes high priority functions like Discord RPC, achievement handling, etc.

// I'm writing code comments at 3:22 in the morning and I have class at 11 what am I doing with my life

public class RevivalRuntimeEngine : MonoBehaviour {
    public static RevivalRuntimeEngine Instance { get; private set; }
    public static SettingsData SettingsData { get; set; }
    public static Vector3 ShakeOffset { get; private set; }
    public static Vector3 ShakeRotation { get; private set; }
    private static readonly HashSet<string> obtainedAchievementNames = new HashSet<string>();

    public static int TotalAchievements { get; private set; }
    public static int ObtainedAchievementsCount => obtainedAchievementNames.Count;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Default RPC Settings")]
    public string details = "";
    public string state = "";

    [Header("Achievement Settings")]
    [SerializeField] private AchievementData[] achievements;
    [SerializeField] private EventReference achievementUnlockStinger;
    [SerializeField] private GameObject achievementItemPrefab;
    [SerializeField] private Transform achievementContainer;

    [Header("Camera Shake Settings")]
    [SerializeField] private float shakeNoiseFrequency = 11f;
    [SerializeField] private float shakeRotationStrength = 9f;
    [SerializeField] private float shakeImpactKick = 25f;
    [SerializeField] private float shakeSpringFrequency = 7f;
    [SerializeField, Range(0.05f, 1f)] private float shakeSpringDamping = 0.3f;
    [SerializeField] private float shakeFadeDuration = 1.4f;

    [Header("Canvas References")]
    public GameObject achievementsScreen;
    public GameObject optionsScreen;

    public bool anyMenuOpen { get; private set; }

    private const ulong APPLICATION_ID = 1450277986178830448; // Discord app token. Hopefully leaving this here isn't a security vulernability :)
    private const string SETTINGS_FILE_NAME = "settings.json"; // File name for the JSON settings file. This is where the settings are saved and loaded from.
    private const string ACHIEVMENTS_FILE_NAME = "achievements.json"; // Similar to the settings, but housng a list of achievement identifiers the player has unlocked.
    private const float SHAKE_BLEND_IN = 0.15f;
    private const float SHAKE_SPRING_STEP = 0.005f;
    private const float SHAKE_REST_THRESHOLD = 0.00001f;

    private ulong startTimestamp;

    private Client client;
    private Coroutine shakeCoroutine;
    private AchievementFile achievementFileData;
    private GameObject currentAchievementItem = null;

    private bool developerMode;
    private float shakeIntensity;
    private float shakeTime;
    private float shakeTimeScale = 1f;
    private Vector4 shakeSeed;
    private Vector3 shakePosVelocity;
    private Vector3 shakeRotVelocity;

    #region Unity Lifecycle

    private void Awake() {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        SettingsData = DataSaver.Load<SettingsData>(SETTINGS_FILE_NAME);
        client = new Client();

        // Do some black magic shit to set the RPC timestamp to the 0:00 mark
        startTimestamp = (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Set the log utility to the OnLog method and set the application ID to applicationID. Then update RPC. 
        client.SetApplicationId(APPLICATION_ID);
        UpdateRichPresence();
    }

    private void OnEnable() {
        inputActions.Enable();
    }

    private void OnDisable() {
        inputActions.Disable();
    }

    private void OnDestroy() {
        if (Instance != this || client == null) return;

        // Cleanup
        client.ClearRichPresence();
        client.Disconnect();
    }

    private void Start() {
        // Perform CORE setup (Scene loading)

        // Safeguard because I keep forgetting to change this manually
        if (SceneController.instance == null) developerMode = true;

        if (!developerMode) {
            SceneController.instance
                .NewTransition()
                .Load(SceneDatabase.Slots.MENU, SceneDatabase.Scenes.MENU)
                .WithOverlay()
                .Perform();
        }

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

    private void Update() {
        UpdateCameraShake(Time.deltaTime);
    }

    #endregion

    #region Private Methods

    // Purely for result logging. Recently added stuff back in here because RPC doesn't want to work anymore.
    private static void OnUpdateRichPresence(ClientResult result) {
        if (!result.Successful()) Debug.LogError("Rich presence failed: " + result.Error());
    }

    private void UpdateRichPresence() {
        var activity = new Activity();

        // Set the state to something like "Playing SCP - CB Revival, details, state"
        activity.SetType(ActivityTypes.Playing);
        if (!string.IsNullOrEmpty(details)) activity.SetDetails(details);
        if (!string.IsNullOrEmpty(state)) activity.SetState(state);

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

    // Generate random shake offset for the camera shake
    private void UpdateCameraShake(float deltaTime) {
        if (deltaTime <= 0f) return;

        Vector3 targetOffset = Vector3.zero;
        Vector3 targetRotation = Vector3.zero;

        if (shakeIntensity > 0f) {
            shakeTime += deltaTime * shakeTimeScale;
            float t = shakeTime * shakeNoiseFrequency;

            targetOffset = new Vector3(
                ShakeNoise(shakeSeed.x, t),
                ShakeNoise(shakeSeed.y, t),
                ShakeNoise(shakeSeed.z + shakeSeed.y, t) * 0.5f
            ) * shakeIntensity;

            targetRotation = new Vector3(
                ShakeNoise(shakeSeed.z, t),
                ShakeNoise(shakeSeed.w, t),
                ShakeNoise(shakeSeed.x + shakeSeed.w, t) * 0.6f
            ) * (shakeIntensity * shakeRotationStrength);
        } else if (ShakeOffset.sqrMagnitude < SHAKE_REST_THRESHOLD && shakePosVelocity.sqrMagnitude < SHAKE_REST_THRESHOLD
                && ShakeRotation.sqrMagnitude < SHAKE_REST_THRESHOLD && shakeRotVelocity.sqrMagnitude < SHAKE_REST_THRESHOLD) {
            ShakeOffset = Vector3.zero;
            ShakeRotation = Vector3.zero;
            shakePosVelocity = Vector3.zero;
            shakeRotVelocity = Vector3.zero;
            return;
        }

        float omega = shakeSpringFrequency * 2f * Mathf.PI;
        float stiffness = omega * omega;
        float damping = 2f * shakeSpringDamping * omega;
        int steps = Mathf.CeilToInt(deltaTime / SHAKE_SPRING_STEP);
        float step = deltaTime / steps;

        Vector3 offset = ShakeOffset;
        Vector3 rotation = ShakeRotation;

        for (int i = 0; i < steps; i++) {
            shakePosVelocity += (stiffness * (targetOffset - offset) - damping * shakePosVelocity) * step;
            offset += shakePosVelocity * step;

            shakeRotVelocity += (stiffness * (targetRotation - rotation) - damping * shakeRotVelocity) * step;
            rotation += shakeRotVelocity * step;
        }

        ShakeOffset = offset;
        ShakeRotation = rotation;
    }

    private void ApplyShakeImpact(float intensity) {
        if (intensity <= 0f) return;

        Vector3 direction = Random.onUnitSphere;
        shakePosVelocity += new Vector3(direction.x, direction.y, direction.z * 0.5f) * (intensity * shakeImpactKick);

        Vector3 rotDirection = Random.onUnitSphere;
        shakeRotVelocity += new Vector3(rotDirection.x, rotDirection.y, rotDirection.z * 0.6f) * (intensity * shakeImpactKick * shakeRotationStrength);
    }

    private static float ShakeNoise(float seed, float t) {
        float sway = Mathf.PerlinNoise(seed, t * 0.35f) * 2f - 1f;
        float shake = Mathf.PerlinNoise(seed + 31.7f, t) * 2f - 1f;
        float rumble = Mathf.PerlinNoise(seed + 77.3f, t * 2.7f) * 2f - 1f;
        return sway * 0.6f + shake + rumble * 0.45f;
    }

    #endregion

    #region Private Coroutines

    private IEnumerator CameraShakeRoutine(float startIntensity, float endIntensity, float duration) {
        float elapsed = 0f;
        float invDuration = duration > 0f ? 1f / duration : 0f;
        float previousIntensity = shakeIntensity;

        if (shakeIntensity <= 0f) {
            shakeTime = 0f;
            shakeSeed = new Vector4(Random.value, Random.value, Random.value, Random.value) * 1000f;
        }

        shakeTimeScale = 1f;
        ApplyShakeImpact(startIntensity);

        // While elapsed time is less than the duration,
        // lerp the shake intensity from start to end and update the camera shake offset
        while (elapsed < duration) {
            float target = Mathf.Lerp(startIntensity, endIntensity, elapsed * invDuration);
            shakeIntensity = Mathf.Lerp(previousIntensity, target, Mathf.SmoothStep(0f, 1f, elapsed / SHAKE_BLEND_IN));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Fade out
        float invFade = shakeFadeDuration > 0f ? 1f / shakeFadeDuration : float.MaxValue;
        float startShake = shakeIntensity;
        elapsed = 0f;

        // While elapsed time is less than the fade duration,
        // lerp the shake intensity from the current shake intensity to 0 and update the camera shake offset
        while (elapsed < shakeFadeDuration) {
            float remaining = 1f - elapsed * invFade;
            shakeIntensity = startShake * remaining * remaining;
            shakeTimeScale = Mathf.Lerp(0.4f, 1f, remaining);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset these Johns
        shakeIntensity = 0f;
        shakeTimeScale = 1f;
        shakeCoroutine = null;
    }

    #endregion

    // Gets an action for a given map. Called from external scripts
    public InputAction GetAction(string mapName, string actionName) {
        var action = inputActions.FindActionMap(mapName)?.FindAction(actionName);

        if (action == null) Debug.LogWarning($"Action {actionName} not found in Map {mapName}.");
        return action;
    }

    public static void SaveSettingsData() => DataSaver.Save<SettingsData>(SettingsData, SETTINGS_FILE_NAME);

    /// <summary>
    /// Publicly available method to change the discord RPC to whatever (USE WISELY, OTHER SCRIPTS KNOW THIS GUY)
    /// </summary>
    /// <param name="newDetails">New details the RPC will use (Subtext 1)</param>
    /// <param name="newState">New state of that detail the RPC will use (Subtext 2)</param>
    public void ChangeDiscordStatus(string newDetails, string newState = "") {
        details = newDetails;
        state = newState;

        UpdateRichPresence();
    }

    /// <summary>
    /// Is the achievement associated with the provided identifier unlocked? Returns true or false based on this question.
    /// </summary>
    /// <param name="achievementIdentifier">The identifier of the achievement to check (IE. achv_173).</param>
    /// <returns>True if the achievement is unlocked, false otherwise.</returns>
    public bool AchievementUnlocked(string achievementIdentifier) {
        if (obtainedAchievementNames.Contains(achievementIdentifier)) return true;
        else return false;
    }

    public void GiveAchievement(string achievementIdentifier) {
        if (achievements.Length > 0 && SettingsData.consoleEnabled) {
            Debug.Log("<color=#ff0000>Tried to give you an achievement, but it looks like you have the console enabled!");
            return;
        }

        if (obtainedAchievementNames.Contains(achievementIdentifier)) return;

        foreach (var achievement in achievements) {
            if (achievement.achievementIdentifier != achievementIdentifier) continue;

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

    public void ShakeCamera(float startIntensity, float endIntensity, float duration) {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(CameraShakeRoutine(startIntensity, endIntensity, duration));
    }

    //TODO: Maybe change the name of these laters since it's not actually toggling anything
    public static void ToggleAchievementsMenu(bool active) {
        Instance.achievementsScreen.SetActive(active);
        Instance.anyMenuOpen = active;
    }

    public static void ToggleOptionsMenu(bool active) {
        Instance.optionsScreen.SetActive(active);
        Instance.anyMenuOpen = active;
    }
}

// If only this outcast knew how important he is right now 
[System.Serializable]
public class AchievementFile {
    public string[] obtainedAchievementIDs;
}