using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A procedural light flickering script that aims to replicate the one Valve uses in Half-Life.
/// Supports various presets also inspired by the Valve ones as well as a custom option.
/// </summary>
public class LightFlicker : MonoBehaviour {
    // Various preset names that can be chosen from in the editor
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
    
    // The actual patterns used by the aforementioned presets, using the alphabetical system
    private static readonly Dictionary<Preset, string> Presets = new()
    {
        { Preset.Normal,                    "m" },
        { Preset.FlickerA,                  "mmnmmommommnonmmonqnmmo" },
        { Preset.SlowStrongPulse,           "abcdefghijklmnopqrstuvwxyzyxwvutsrqponmlkjihgfedcba" },
        { Preset.FastStrobe,                "mammogram" },
        { Preset.GentlePulse,               "jklmnopqrstuvwxyzyxwvutsrqponmlkji" },
        { Preset.Flicker,                   "nmonqnmomnmomomno" },
        { Preset.CandleA,                   "mmamammmmammamamaaamammma" },
        { Preset.SlowStrobeA,               "mmmmmaaaaammmmmaaaaaammmmmmmmmmmaaaaaa" },
        { Preset.FluorescentFlicker,        "mmamammmmammamamaaamammma" },
        { Preset.SlowPulseNotFadeToBlack,   "abcdefghijklmnopqrrqponmlkjihgfedcba" },
        { Preset.CandleB,                   "mmmaaaabcdefgmmmmaaaammmaamm" },
        { Preset.CandleC,                   "mmmaaammmaaammmabcdefaaaammmmabcdefmmmaaaa" },
        { Preset.SlowStrobeB,               "aaaaaaaazzzzzzzz" },
        { Preset.FastPulse,                 "mmnnmmnnnmmnn" },
        { Preset.FlickerB,                  "mmnommomhamenbobaamgoamnnoaon" },
    };
    
    [Header("Pattern")]
    [SerializeField] private Preset preset = Preset.FlickerA;
    [SerializeField] private string customPattern = "mmnmmommommnonmmonqnmmo";

    [Header("Playback")] 
    [SerializeField] private float stepsPerSecond = 10f;
    [SerializeField] private bool randomOffset = true;

    [Header("Light")] 
    [SerializeField] private bool smoothTransitions;
    [SerializeField, Range(0f, 50f)] private float smoothSpeed = 20f;

    private new Light light;
    private float maxIntensity;
    private float timer;
    private int index;
    private float targetIntensity;
    private string activePattern;

    private void Awake() {
        light = GetComponent<Light>();
        maxIntensity = light.intensity;
        
        RefreshPattern();
        
        if (randomOffset) index = Random.Range(0, activePattern.Length);
    }

    private void Update() {
        if (activePattern.Length == 0) return;
        
        timer += Time.deltaTime;
        var interval = 1f / Mathf.Max(stepsPerSecond, 0.01f);

        while (timer >= interval) {
            timer -= interval;
            index = (index + 1) % activePattern.Length;
            targetIntensity = PatternCharToIntensity(activePattern[index]);
        }

        light.intensity = smoothTransitions
            ? Mathf.MoveTowards(light.intensity, targetIntensity, smoothSpeed * Time.deltaTime)
            : targetIntensity;
    }

    private float PatternCharToIntensity(char character) {
        var t = Mathf.Clamp01((character - 'a') / 25f);
        return t * maxIntensity;
    }
    
    private void RefreshPattern() {
        activePattern = preset == Preset.Custom
        ? customPattern.ToLower()
        : Presets.GetValueOrDefault(preset, "m");

        index = 0;
        timer = 0f;
        targetIntensity = PatternCharToIntensity(activePattern[0]);
    }

    public void SetPreset(Preset newPreset) {
        preset = newPreset;
        RefreshPattern();
    }
}