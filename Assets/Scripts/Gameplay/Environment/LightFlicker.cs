using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Procedural light flickering replicating Valve's Half-Life implementation.
/// Supports various presets using an alphabetical intensity pattern system (a = off, z = full).
/// </summary>
public class LightFlicker : MonoBehaviour {
    public enum Preset {
        Custom,
        Normal,
        FlickerA,
        FlickerB,
        SlowStrongPulse,
        FastStrobe,
        GentlePulse,
        Flicker,
        CandleA,
        SlowStrobeA,
        FluorescentFlicker,
        SlowPulseNotFadeToBlack,
        CandleB,
        CandleC,
        SlowStrobeB,
        FastPulse
    }

    // Map each preset to its alphabetical intensity pattern string
    private static readonly Dictionary<Preset, string> Presets = new() {
        { Preset.Normal,                  "m" },
        { Preset.FlickerA,                "mmnmmommommnonmmonqnmmo" },
        { Preset.FlickerB,                "mmnommomhamenbobaamgoamnnoaon" },
        { Preset.SlowStrongPulse,         "abcdefghijklmnopqrstuvwxyzyxwvutsrqponmlkjihgfedcba" },
        { Preset.FastStrobe,              "mammogram" },
        { Preset.GentlePulse,             "jklmnopqrstuvwxyzyxwvutsrqponmlkji" },
        { Preset.Flicker,                 "nmonqnmomnmomomno" },
        { Preset.CandleA,                 "mmamammmmammamamaaamammma" },
        { Preset.CandleB,                 "mmmaaaabcdefgmmmmaaaammmaamm" },
        { Preset.CandleC,                 "mmmaaammmaaammmabcdefaaaammmmabcdefmmmaaaa" },
        { Preset.SlowStrobeA,             "mmmmmaaaaammmmmaaaaaammmmmmmmmmmaaaaaa" },
        { Preset.SlowStrobeB,             "aaaaaaaazzzzzzzz" },
        { Preset.FluorescentFlicker,      "mmamammmmammamamaaamammma" },
        { Preset.SlowPulseNotFadeToBlack, "abcdefghijklmnopqrrqponmlkjihgfedcba" },
        { Preset.FastPulse,               "mmnnmmnnnmmnn" },
    };

    [Header("Pattern")]
    [SerializeField] private Preset preset = Preset.FlickerA;
    [SerializeField] private string customPattern = "mmnmmommommnonmmonqnmmo";

    [Header("Playback")]
    [SerializeField] private float stepsPerSecond = 10f;
    [SerializeField] private bool randomOffset = true;
    [SerializeField] private bool startActive = true;

    [Header("Light")]
    [SerializeField] private bool smoothTransitions;
    [SerializeField, Range(0f, 50f)] private float smoothSpeed = 20f;

    private Coroutine oneShotCoroutine;
    private new Light light;
    private Preset oneShotSavedPreset;
    private string oneShotSavedCustomPattern;
    private bool oneShotSavedIsActive;
    private bool hasSavedOneShotState;
    
    private bool isActive;
    private float maxIntensity;
    private string activePattern;
    private float stepInterval;
    private float timer;
    private int index;
    private float targetIntensity;

    #region Unity Callbacks
    private void Awake() {
        light = GetComponent<Light>();
        maxIntensity = light.intensity;
        RefreshPattern();
        isActive = startActive;

        if (randomOffset)
            index = Random.Range(0, activePattern.Length);
    }

    private void Update() {
        if (!isActive || activePattern.Length == 0) return;

        timer += Time.deltaTime;

        // Advance the pattern index for each elapsed step interval
        while (timer >= stepInterval) {
            timer -= stepInterval;
            index = (index + 1) % activePattern.Length;
            targetIntensity = CharToIntensity(activePattern[index]);
        }

        light.intensity = smoothTransitions
            ? Mathf.MoveTowards(light.intensity, targetIntensity, smoothSpeed * Time.deltaTime)
            : targetIntensity;
    }

    // Reflect inspector changes in the editor without entering play mode
    private void OnValidate() {
        CacheStepInterval();

        // Guard against missing light reference outside play mode
        if (light == null) light = GetComponent<Light>();
        if (light != null) RefreshPattern();
    }
    #endregion

    #region Private Methods
    // Convert a pattern character (a–z) to a [0, maxIntensity] value
    private float CharToIntensity(char c) {
        var t = Mathf.Clamp01((c - 'a') / 25f);
        return t * maxIntensity;
    }

    // Precompute the interval used by the Update loop
    private void CacheStepInterval() =>
        stepInterval = 1f / Mathf.Max(stepsPerSecond, 0.01f);

    private void RefreshPattern() {
        activePattern = preset == Preset.Custom
            ? customPattern.ToLower()
            : Presets.GetValueOrDefault(preset, "m");

        CacheStepInterval();

        index = 0;
        timer = 0f;
        targetIntensity = CharToIntensity(activePattern[0]);
    }

    private void SaveOneShotState() {
        if (hasSavedOneShotState) return;

        oneShotSavedPreset = preset;
        oneShotSavedCustomPattern = customPattern;
        oneShotSavedIsActive = isActive;
        hasSavedOneShotState = true;
    }

    private void RestoreOneShotState() {
        if (!hasSavedOneShotState) return;

        preset = oneShotSavedPreset;
        customPattern = oneShotSavedCustomPattern;
        isActive = oneShotSavedIsActive;
        hasSavedOneShotState = false;
        RefreshPattern();
        light.intensity = maxIntensity;
    }
    
    private IEnumerator OneShotRoutine(float duration, string pattern) {
        SaveOneShotState();
        isActive = true;
        if (pattern != null) {
            preset = Preset.Custom;
            customPattern = pattern;
            RefreshPattern();
        }

        yield return new WaitForSeconds(duration);

        oneShotCoroutine = null;
        RestoreOneShotState();
    }
    #endregion

    #region Public Methods
    public void SetPreset(Preset newPreset) {
        preset = newPreset;
        RefreshPattern();
    }
    
    public void SetActive(bool active) => isActive = active;

    public void PlayPatternForDuration(float duration, string pattern = null) {
        SaveOneShotState();

        if (oneShotCoroutine != null) {
            StopCoroutine(oneShotCoroutine);
            oneShotCoroutine = null;
        }

        oneShotCoroutine = StartCoroutine(OneShotRoutine(duration, pattern));
    }
    #endregion
}
