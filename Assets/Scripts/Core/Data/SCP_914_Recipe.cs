using EditorAttributes;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "SCP:CBR/914 Recipe")]
public class SCP_914_Recipe : ScriptableObject {
    public enum RefinementMode { Rough, Coarse, OneToOne, Fine, VeryFine }

    #region Output Class

    [Serializable]
    public class Output {
        public ItemData outputItem; // Leave this blank to destroy the item instead of returning something

        // Setup for the chance system, item WILL return this output chanceMin out of chanceMax times
        [Min(0)] public int chanceMin;
        [Min(1)] public int chanceMax;

        // These values are to be used in tandem with otherDifficultyFactors reading off of GameManager to determine
        // the chances of the output based on the current difficulty factor from 0-2
        public bool useOtherDiffFactors;
        [ShowField(nameof(useOtherDiffFactors)), Min(0)] public int chanceMinEuclid;
        [ShowField(nameof(useOtherDiffFactors)), Min(1)] public int chanceMaxEuclid;
        [ShowField(nameof(useOtherDiffFactors)), Min(0)] public int chanceMinKeter;
        [ShowField(nameof(useOtherDiffFactors)), Min(1)] public int chanceMaxKeter;

        // If any of the chances given don't meet the given range then spit out a random item listed below
        public bool useFailureItems;
        [ShowField(nameof(useFailureItems))] public ItemData[] possibleFailureItems;

        // Do some math stuff that took me and Buck 10 minutes to calculate to change odds based on achievements too
        public bool useAchievementStats;
        private const int AchievementOffsetMultiplier = 3;
        private static readonly int[] DifficultyBaseMultipliers = { 3, 4, 5 }; // HARDCODED PROBABLY FOREVER

        private static int ComputeAchievementRange(int difficultyFactor, int maxAchievements, int currentAchievements) {
            var clampedFactor = Mathf.Clamp(difficultyFactor, 0, DifficultyBaseMultipliers.Length - 1);
            var baseRange = (maxAchievements - 1) * DifficultyBaseMultipliers[clampedFactor];
            var achievementOffset = (currentAchievements - 1) * AchievementOffsetMultiplier;

            return Mathf.Max(1, baseRange - achievementOffset);
        }

        // Returns true if this output succeeds its chance roll.
        public bool RollChance(int difficultyFactor) {
            if (useAchievementStats) {
                var maxAchievements = RevivalRuntimeEngine.TotalAchievements;
                var currAchievements = RevivalRuntimeEngine.ObtainedAchievementsCount;

                var range = ComputeAchievementRange(difficultyFactor, maxAchievements, currAchievements);
                return UnityEngine.Random.Range(0, range) == 0;
            }

            var min = chanceMin;
            var max = chanceMax;

            if (useOtherDiffFactors) {
                switch (difficultyFactor) {
                    case 1:
                        min = chanceMinEuclid;
                        max = chanceMaxEuclid;
                        break;

                    case 2:
                        min = chanceMinKeter;
                        max = chanceMaxKeter;
                        break;
                }
            }

            min = Mathf.Max(0, min);
            max = Mathf.Max(1, max);

            if (min >= max) return true;

            return UnityEngine.Random.Range(0, max) < min;
        }

        public ItemData ResolveItem(int difficultyFactor) {
            if (RollChance(difficultyFactor)) return outputItem;

            if (!useFailureItems ||
                possibleFailureItems == null ||
                possibleFailureItems.Length == 0)
                return null;

            return possibleFailureItems[UnityEngine.Random.Range(0, possibleFailureItems.Length)];
        }
    }

    #endregion

    #region PerModeRecipe Class

    [Serializable]
    public class ModeRecipe {
        public RefinementMode mode;
        public Output[] possibleOutputs;
    }

    #endregion

    private static readonly int ModeCount = Enum.GetValues(typeof(RefinementMode)).Length;

    public ItemData inputItem;
    public ModeRecipe[] perModeRecipes;

    private ModeRecipe[] modeLookup;

    #region Unity Lifecycle

    private void OnEnable() {
        BuildModeLookup();
    }

#if UNITY_EDITOR
    private void OnValidate() {
        BuildModeLookup();
    }
#endif

    #endregion

    #region Private Methods

    private void BuildModeLookup() {
        if (modeLookup == null || modeLookup.Length != ModeCount) {
            modeLookup = new ModeRecipe[ModeCount];
        } else {
            Array.Clear(modeLookup, 0, modeLookup.Length);
        }

        if (perModeRecipes == null) return;

        for (var i = 0; i < perModeRecipes.Length; i++) {
            ModeRecipe modeRecipe = perModeRecipes[i];
            if (modeRecipe == null) continue;

            int index = (int)modeRecipe.mode;
            if (index < 0 || index >= ModeCount) continue;

            if (modeLookup[index] != null) {
                Debug.LogWarning($"Duplicate mode {modeRecipe.mode} found in recipe {name}", this);
                continue;
            }

            modeLookup[index] = modeRecipe;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Resolves the output of this recipe for the given refinement mode.
    /// Returns the input item unchanged if this recipe has no entry for the mode,
    /// or null if the item should be destroyed.
    /// </summary>
    /// <param name="mode">What mode of refinement the item is getting passed through</param>
    /// <param name="difficultyFactor">The current otherDifficultyFactor (0-2)</param>
    public ItemData GetOutput(RefinementMode mode, int difficultyFactor) {
        if (modeLookup == null) BuildModeLookup();

        int index = (int)mode;
        if (index < 0 || index >= ModeCount) return inputItem;

        ModeRecipe modeRecipe = modeLookup[index];
        if (modeRecipe == null) return inputItem;

        Output[] outputs = modeRecipe.possibleOutputs;
        if (outputs == null || outputs.Length == 0) return null;

        for (var i = 0; i < outputs.Length; i++) {
            Output output = outputs[i];
            if (output == null) continue;

            ItemData result = output.ResolveItem(difficultyFactor);
            if (result) return result;
        }

        return null;
    }

    #endregion
}