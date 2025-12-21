using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EntitySystem : MonoBehaviour {
    public static EntitySystem instance;
    public enum EntityType { SCP173, SCP106 };

    private void Awake() {
        if (instance == null) { 
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void MoveEntity(EntityType entityType, Transform newPos) {
        switch (entityType) { 
            case EntityType.SCP173:
                SCP_173 active173 = FindFirstObjectByType<SCP_173>();

                active173.transform.position = newPos.transform.position;
                active173.GetComponent<NavMeshAgent>().Warp(newPos.transform.position);
                StartCoroutine(SleepAfterTeleport(active173.GetComponent<NavMeshAgent>()));
                break;
            case EntityType.SCP106:
                break;
            default:
                break;
        }
    }

    private IEnumerator SleepAfterTeleport(NavMeshAgent agentToStop) {
        agentToStop.isStopped = true;
        yield return new WaitForSeconds(0.7f);
        agentToStop.isStopped = false;

    }
}