using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCP_914 : MonoBehaviour {
    private enum RefinementMode { Rough, Coarse, OneToOne, Fine, VeryFine }

    [SerializeField] private RefineryRecipeTable recipeTable;

    [Header("Refinery Settings")]
    public SCP_914_Recipe.RefinementMode currentRefinementMode;
    [SerializeField] private float refinementTime = 13.6f;

    [Header("References")]
    [SerializeField] private BoxCollider inputArea;
    [SerializeField] private BoxCollider outputArea;
    [SerializeField] private Door[] machineDoors;
    [SerializeField] private LightFlicker[] machineLights;

    #region Private Methods

    private void RefineItem(WorldItem item) {
        Debug.Log($"914 refining {item.associatedItemData.name} in mode {currentRefinementMode}");

        ItemData outputItem = recipeTable.GetOutput(item.associatedItemData, currentRefinementMode);
        Destroy(item.gameObject);
        if (!outputItem) return;

        Instantiate(outputItem.itemWorldPrefab, GetRandomOutputPosition(), Quaternion.identity);
    }

    #endregion

    #region Private IENumerators

    private IEnumerator RefinementProcess() {
        var itemsToRefine = GetItemsInInputArea();

        foreach (LightFlicker machineLight in machineLights) machineLight.PlayPatternForDuration(1.7f);
        foreach (Door door in machineDoors) door.CloseDoor();

        yield return new WaitForSeconds(refinementTime);

        foreach (WorldItem item in itemsToRefine) if (item) RefineItem(item);
        foreach (var door in machineDoors) door.OpenDoor();
    }

    #endregion

    #region Private Utilities

    private List<WorldItem> GetItemsInInputArea() {
        Bounds bounds = inputArea.bounds;
        Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, inputArea.transform.rotation);
        List<WorldItem> items = new List<WorldItem>();

        foreach (var collider in colliders)
            if (collider.TryGetComponent(out WorldItem item)) items.Add(item);

        return items;
    }

    private Vector3 GetRandomOutputPosition() {
        var bounds = outputArea.bounds;

        return new Vector3(Random.Range(bounds.min.x, bounds.max.x), bounds.center.y,
            Random.Range(bounds.min.z, bounds.max.z));
    }

    #endregion

    #region Public Methods

    public void EnterRoomEvent() {
        if (RevivalSessionEngine.currentPursuit != 0) return;
        MusicManager.Instance.SetTrack(MusicManager.MusicTrack.SCP_914);
    }

    public void BeginRefineProcess() {
        StartCoroutine(RefinementProcess());
        RevivalRuntimeEngine.Instance.GiveAchievement("achv_914");
    }

    #endregion
}