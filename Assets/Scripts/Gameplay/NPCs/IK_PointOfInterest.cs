using System.Collections.Generic;
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
        if (registerOnEnable) {
            if (allActors) {
                IK_MasterComponent[] ikSystems = FindObjectsByType<IK_MasterComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                List<IK_MasterComponent> ikSystemsList = new List<IK_MasterComponent>(ikSystems);

                foreach (IK_MasterComponent ikSystemss in ikSystemsList) {
                    ikSystemss.pointsOfInterest.Add(this);
                }
            }
        }
    }

    #endregion

    #region Public Methods

    // In most cases this will be for the players POI as he enters all kinds of rooms full of IK Masters yet to be activated
    public void RegisterPOIToAllActors() {
        IK_MasterComponent[] ikSystems = FindObjectsByType<IK_MasterComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<IK_MasterComponent> ikSystemsList = new List<IK_MasterComponent>(ikSystems);

        foreach (IK_MasterComponent ikSystemss in ikSystemsList) {
            ikSystemss.pointsOfInterest.Add(this);
        }
    }

    #endregion
}