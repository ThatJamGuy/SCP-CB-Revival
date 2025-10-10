using UnityEngine;

public class IK_PointOfInterest : MonoBehaviour {
    [SerializeField] private bool registerToAllActors = true;

    #region Default Methods
    private void Start() {
        // Register this POI to all NPC actors in the scene
        if (registerToAllActors) {
            NPC_Actor[] actors = FindObjectsByType<NPC_Actor>(FindObjectsSortMode.None);
            foreach (NPC_Actor actor in actors) {
                if (!actor.PointsOfInterest.Contains(this)) {
                    actor.PointsOfInterest.Add(this);
                }
            }
        }
    }
    #endregion

    #region Public Methods
    // If actor is unable to recieve this POI, re-register to all actors
    public void ReRegisterToAllActors() {
        if (registerToAllActors) {
            NPC_Actor[] actors = FindObjectsByType<NPC_Actor>(FindObjectsSortMode.None);
            foreach (NPC_Actor actor in actors) {
                if (!actor.PointsOfInterest.Contains(this)) {
                    actor.PointsOfInterest.Add(this);
                }
            }
        }
    }

    // Register this POI to a specific actor
    public void RegisterToSpecificActor(NPC_Actor actor) {
        if(!actor.PointsOfInterest.Contains(this))
            actor.PointsOfInterest.Add(this);
    }
    #endregion
}