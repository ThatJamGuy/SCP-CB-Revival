using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCP_914 : MonoBehaviour {
    [Header("Refinery Settings")]
    public SCP_914_Recipe.RefinementMode currentRefinementMode;
    public SCP_914_Recipe[] recipes;

    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private bool verboseLogging;

    [Header("References")]
    [SerializeField] private BoxCollider inputArea;
    [SerializeField] private BoxCollider outputArea;
    [SerializeField] private Door[] machineDoors;
    [SerializeField] private LightFlicker[] machineLights;

    private readonly Dictionary<ItemData, SCP_914_Recipe> recipeDictionary = new();
    private readonly Collider[] overlapBuffer = new Collider[MAX_COLLIDERS];
    private readonly List<WorldItem> itemsToRefine = new(MAX_COLLIDERS);
    private readonly HashSet<WorldItem> seenItems = new();

    private const int MAX_COLLIDERS = 10;
    private const float LIGHT_FLICKER_DURATION = 1.7f;
    private const float REFINEMENT_TIME = 13.6f;
    private const string ACHV_TO_GIVE = "achv_914";

    private bool isRefining;

    #region Unity Lifecycle

    private void Awake() {
        FillRecipeDictionary();
    }

    #endregion

    #region Public Methods

    public void EnterRoomEvent() {
        if (RevivalSessionEngine.currentPursuit != 0) return;
        MusicManager.Instance.SetTrack(MusicManager.MusicTrack.SCP_914);
    }

    public void BeginRefineProcess() {
        if (isRefining) return;

        StartCoroutine(RefinementProcess());

        if (RevivalRuntimeEngine.Instance != null)
            RevivalRuntimeEngine.Instance.GiveAchievement(ACHV_TO_GIVE);
    }

    #endregion

    #region Recipes

    private void FillRecipeDictionary() {
        recipeDictionary.Clear();

        if (recipes == null) return;

        for (int i = 0; i < recipes.Length; i++) {
            SCP_914_Recipe recipe = recipes[i];
            if (recipe == null || recipe.inputItem == null) continue;

            if (!recipeDictionary.TryAdd(recipe.inputItem, recipe))
                Debug.LogWarning($"{name}: duplicate 914 recipe for '{recipe.inputItem.name}' ignored.", this);
        }
    }

    private ItemData GetOutput(ItemData inputItem, SCP_914_Recipe.RefinementMode mode) {
        if (!recipeDictionary.TryGetValue(inputItem, out SCP_914_Recipe recipe)) {
            if (verboseLogging) Debug.LogWarning($"914 lookup: no recipe for '{inputItem.name}', passing through unchanged.", this);
            return inputItem;
        }

        return recipe.GetOutput(mode, RevivalSessionEngine.otherDifficultyFactor);
    }

    #endregion

    #region Refinement

    private IEnumerator RefinementProcess() {
        isRefining = true;

        CollectItemsInInputArea();

        for (int i = 0; i < machineLights.Length; i++)
            machineLights[i].PlayPatternForDuration(LIGHT_FLICKER_DURATION);

        for (int i = 0; i < machineDoors.Length; i++)
            machineDoors[i].CloseDoor();

        yield return new WaitForSeconds(REFINEMENT_TIME);

        for (int i = 0; i < itemsToRefine.Count; i++) {
            WorldItem item = itemsToRefine[i];
            if (item) RefineItem(item);
        }

        itemsToRefine.Clear();

        for (int i = 0; i < machineDoors.Length; i++)
            machineDoors[i].OpenDoor();

        isRefining = false;
    }

    private void RefineItem(WorldItem item) {
        ItemData inputData = item.associatedItemData;
        if (inputData == null) return;

        if (verboseLogging)
            Debug.Log($"914 refining {inputData.name} in mode {currentRefinementMode}", this);

        ItemData outputItem = GetOutput(inputData, currentRefinementMode);
        Destroy(item.gameObject);

        if (outputItem == null) return;

        Instantiate(outputItem.itemWorldPrefab, GetRandomOutputPosition(), Quaternion.identity);
    }

    #endregion

    #region Utilities

    private void CollectItemsInInputArea() {
        itemsToRefine.Clear();
        seenItems.Clear();

        Transform t = inputArea.transform;
        Vector3 size = inputArea.size;
        Vector3 scale = t.lossyScale;
        Vector3 halfExtents = new Vector3(
            Mathf.Abs(size.x * scale.x) * 0.5f,
            Mathf.Abs(size.y * scale.y) * 0.5f,
            Mathf.Abs(size.z * scale.z) * 0.5f);

        int count = Physics.OverlapBoxNonAlloc(
            t.TransformPoint(inputArea.center), halfExtents, overlapBuffer, t.rotation, itemLayer);

        if (count == MAX_COLLIDERS)
            Debug.LogWarning($"{name}: max colldiers full, some items may be skipped.", this);

        for (int i = 0; i < count; i++) {
            if (overlapBuffer[i].TryGetComponent(out WorldItem item) && seenItems.Add(item))
                itemsToRefine.Add(item);
        }
    }

    private Vector3 GetRandomOutputPosition() {
        Bounds bounds = outputArea.bounds;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        return new Vector3(Random.Range(min.x, max.x), bounds.center.y, Random.Range(min.z, max.z));
    }

    #endregion
}