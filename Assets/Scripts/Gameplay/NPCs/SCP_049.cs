using System;
using UnityEngine;
using UnityEngine.AI;

public class SCP_049 : MonoBehaviour {
    [SerializeField] private Transform currentTarget;
    [SerializeField] private NPC_Locomotion locomotionSystem;

    private void Update() {
        locomotionSystem.WalkToPosition(currentTarget);
    }
}