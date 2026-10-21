using System.Collections.Generic;
using UnityEngine;

//TODO: Rewrite some of this later to output prefabs instead of item data prefabs because I forgot it could do that

[CreateAssetMenu(fileName = "RefineryRecipeTable", menuName = "SCP:CBR/914 Recipe Table")]
public class RefineryRecipeTable : ScriptableObject {
    public readonly struct RefinementResult {
        public readonly ItemData Item;

        public RefinementResult(ItemData item) {
            Item = item;
        }
    }

    private readonly Dictionary<ItemData, SCP_914_Recipe> recipeLookup = new();

    public SCP_914_Recipe[] recipes;

    #region Unity Lifecycle

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

        for (var i = 0; i < recipes.Length; i++) {
            SCP_914_Recipe recipe = recipes[i];
            if (recipe == null || recipe.inputItem == null) continue;

            if (!recipeLookup.TryAdd(recipe.inputItem, recipe)) {
                Debug.LogWarning($"Duplicate refinery recipe found for {recipe.inputItem.name}", this);
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
    /// <returns>The resulting item, the input item if no recipe exists, or null if the item is destroyed.</returns>
    public ItemData GetOutput(ItemData inputItem, SCP_914_Recipe.RefinementMode mode) {
        bool found = recipeLookup.TryGetValue(inputItem, out var recipe);

        Debug.Log(
            $"914 lookup: table={name}, recipes={(recipes == null ? -1 : recipes.Length)}, " +
            $"input={(inputItem ? inputItem.name : "NULL")}, " +
            $"cachedEntries={recipeLookup.Count}, found={found}, mode={mode}");

        if (!found) return inputItem;
        return recipe.GetOutput(mode, RevivalSessionEngine.otherDifficultyFactor);
    }

    #endregion
}