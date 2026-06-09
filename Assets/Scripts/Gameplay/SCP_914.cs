using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SCP_914 : MonoBehaviour {
    private enum RefinementMode { Rough, Coarse, OneToOne, Fine, VeryFine }

    [SerializeField] private RefineryRecipeTable recipeTable;
    
    [Header("Refinery Settings")]
    public RefineryRecipeTable.RefinementMode currentRefinementMode;
    [SerializeField] private float refinementTime = 13.6f;
    
    [Header("References")]
    [SerializeField] private BoxCollider inputArea;
    [SerializeField] private BoxCollider outputArea;
    [SerializeField] private Door[] machineDoors;
    [SerializeField] private LightFlicker[] machineLights;

    #region Private Methods
    
    private void RefineItem(WorldItem item) {
        var outputItem = recipeTable.GetOutput(item.associatedItemData, currentRefinementMode);
        Destroy(item.gameObject);
        if (!outputItem) return;
        
        Instantiate(outputItem.itemWorldPrefab, GetRandomOutputPosition(), Quaternion.identity);
    }
    
    private IEnumerator RefinementProcess() {
        var itemsToRefine = GetItemsInInputArea();
        
        foreach (var machineLight in machineLights) machineLight.PlayPatternForDuration(1.7f);
        foreach (var door in machineDoors) door.CloseDoor();

        yield return new WaitForSeconds(refinementTime);

        foreach (var item in itemsToRefine) 
            if (item) RefineItem(item);

        foreach (var door in machineDoors)
            door.OpenDoor();
    }
    
    private List<WorldItem> GetItemsInInputArea() {
        var bounds = inputArea.bounds;
        var colliders = Physics.OverlapBox(bounds.center, bounds.extents, inputArea.transform.rotation);
        var items = new List<WorldItem>();

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
    
    public void BeginRefineProcess() {
        StartCoroutine(RefinementProcess());
        AchievementSystem.Instance.GiveAchievement("achv_914");
    }
    
    #endregion
}