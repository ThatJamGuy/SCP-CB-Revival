using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Elevator_Teleport_Floor : MonoBehaviour {
    [SerializeField] private Elevator_Teleport elevatorController;
    [SerializeField] private Door thisElevatorDoor;
    [SerializeField] private LightFlicker thisLight;

    private readonly List<NavMeshAgent> npcsInside = new List<NavMeshAgent>(4);

    public Door Door => thisElevatorDoor;
    public LightFlicker LightFlicker => thisLight;
    public List<NavMeshAgent> NPCsInside => npcsInside;
    public bool PlayerInside { get; set; }

    #region Unity Lifecycle

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) PlayerInside = true;
        else if (other.CompareTag("NPC")) AddNPC(other.GetComponent<NavMeshAgent>());
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) PlayerInside = false;
        else if (other.CompareTag("NPC")) {
            NavMeshAgent npc = other.GetComponent<NavMeshAgent>();
            if (npc != null) npcsInside.Remove(npc);
        }
    }

    #endregion

    public void AddNPC(NavMeshAgent npc) {
        if (npc != null && !npcsInside.Contains(npc)) npcsInside.Add(npc);
    }

    public void OnOutsideButtonPressed() => elevatorController.OnOutsideButtonPressed(this);
    public void OnInsideButtonPressed() => elevatorController.OnInsideButtonPressed(this);
    public void OnElevatorDoorClosed() => elevatorController.OnElevatorDoorClosed(this);
}