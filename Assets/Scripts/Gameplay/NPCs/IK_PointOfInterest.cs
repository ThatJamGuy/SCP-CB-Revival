using UnityEngine;

public class IK_PointOfInterest : MonoBehaviour {
    [Header("POI Settings")]
    public bool poiIsActive = true;
    [SerializeField] private bool registerOnEnable;

    [Header("Register to...")]
    [SerializeField] private bool allActors;
    [SerializeField] private bool specificActor;

    #region Unity Callbacks

    private void OnEnable() {
        if (registerOnEnable && allActors) RegisterPOIToAllActors();
    }

    #endregion

    #region Public Methods

    // In most cases this will be for the players POI as he enters all kinds of rooms full of IK Masters yet to be activated
    public void RegisterPOIToAllActors() {
        IK_MasterComponent[] ikSystems = FindObjectsByType<IK_MasterComponent>();

        foreach (IK_MasterComponent ikSystem in ikSystems) {
            if (!ikSystem.pointsOfInterest.Contains(this))
                ikSystem.pointsOfInterest.Add(this);
        }
    }

    #endregion
}
