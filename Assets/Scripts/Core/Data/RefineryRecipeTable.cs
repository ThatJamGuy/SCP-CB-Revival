using System;
using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;

[CreateAssetMenu(fileName = "RefineryRecipeData", menuName = "SCP:CBR/914 Recipe Data")]
public class RefineryRecipeTable : ScriptableObject {
    public readonly struct RefinementResult {
        public readonly ItemData Item;

        public RefinementResult(ItemData item) {
            Item = item;
        }
    }
    
    public enum RefinementMode { Rough, Coarse, OneToOne, Fine, VeryFine }
    
    [Serializable]
    public class Output {
        public ItemData outputItem; // Leave this blank to destroy the item instead of returning something

        // Setup for the chance system, item WILL return this output chanceMin out of chanceMax times
        [Min(0)] public int chanceMin;
        [Min(1)] public int chanceMax;

        public bool useOtherDiffFactors;
        
        // These values are to be used in tandem with otherDifficultyFactors reading off of GameManager to determine
        // the chances of the output based on the current difficulty factor from 0-2
        [ShowField(nameof(useOtherDiffFactors)), Min(0)] public int chanceMinEuclid;
        [ShowField(nameof(useOtherDiffFactors)), Min(1)] public int chanceMaxEuclid;
        [ShowField(nameof(useOtherDiffFactors)), Min(0)] public int chanceMinKeter;
        [ShowField(nameof(useOtherDiffFactors)), Min(1)] public int chanceMaxKeter;

        public bool useFailureItems;

        [ShowField(nameof(useFailureItems))] public ItemData[] possibleFailureItems;

        /// <summary>
        /// Returns true if this output succeeds its chance roll.
        /// </summary>
        public bool RollChance(int difficultyFactor) {
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

    [Serializable]
    public class ModeRecipe {
        public RefinementMode mode;
        public Output[] possibleOutputs;
    }

    [Serializable]
    public class Recipe {
        public ItemData inputItem;
        public ModeRecipe[] perModeRecipes;
    }

    public Recipe[] recipes;
    
    private readonly Dictionary<(ItemData, RefinementMode), ModeRecipe> recipeLookup = new();
    
    #region Unity Callbacks
    
    private void OnEnable() {
        BuildRecipeLookup();
    }

#if UNITY_EDITOR
    private void OnValidate() {
        BuildRecipeLookup();
    }
#endif
    
    #endregion
    
    #region Private Methods
    
    private void BuildRecipeLookup() {
        recipeLookup.Clear();

        if (recipes == null) return;

        foreach (var recipe in recipes) {
            if (recipe == null || recipe.inputItem == null) continue;
            if (recipe.perModeRecipes == null) continue;

            foreach (var modeRecipe in recipe.perModeRecipes) {
                if (modeRecipe == null) continue;

                var key = (recipe.inputItem, modeRecipe.mode);

                if (!recipeLookup.TryAdd(key, modeRecipe)) {
                    Debug.LogWarning(
                        $"Duplicate refinery recipe found for {recipe.inputItem.name} ({modeRecipe.mode})", this
                    );
                }
            }
        }
    }
    
    #endregion
    
    #region Public Methods
    /// <summary>
    /// Method called by the SCP-914 script to figure out the output for a given item dropped into the input area.
    /// </summary>
    /// <param name="inputItem">The ItemData SCP-914 needs to get the output for</param>
    /// <param name="mode">What mode of refinement the item is getting passed through</param>
    /// <returns></returns>
    public ItemData GetOutput(ItemData inputItem, RefinementMode mode) {
        if (!recipeLookup.TryGetValue((inputItem, mode), out var modeRecipe)) return inputItem;
        if (modeRecipe.possibleOutputs == null || modeRecipe.possibleOutputs.Length == 0) return null;
        
        var difficultyFactor = GameManager.Instance.otherDifficultyFactor;

        foreach (var output in modeRecipe.possibleOutputs) {
            if (output == null) continue;
            var result = output.ResolveItem(difficultyFactor);
            if (result) return result;
        }

        return null;
    }
    #endregion
}