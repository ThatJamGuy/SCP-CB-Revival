using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class SCP_049 : MonoBehaviour {
    [Header("Door Manip Settings")]
    [SerializeField] private LayerMask doorlayerMask;
    [SerializeField] private float doorCheckInterval;
    [SerializeField] private float doorOpenRadius;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private NPC_Locomotion locomotionSystem;
    [SerializeField] private TwoBoneIKConstraint handIKConstraint;
    [SerializeField] private Transform ikHandTarget;

    private const float IK_DISTANCE = 5f;
    private const float IK_DISTANCE_SQR = IK_DISTANCE * IK_DISTANCE;
    private const float IK_BLEND_SPEED = 5f;
    private const int MAX_COLLIDERS = 5;

    private Transform currentTarget;
    private Collider[] hitColliders;

    private float doorCheckElapsedTime;

    #region Unity Callbacks

    private void Awake() {
        hitColliders = new Collider[MAX_COLLIDERS];
    }

    private void Update() {
        UpdateHandIK();
        CheckForDoors();
    }

    #endregion

    #region Private Methods

    #region IK

    private void UpdateHandIK() {
        if (currentTarget == null) return;

        bool isWithinRange = (currentTarget.position - transform.position).sqrMagnitude < IK_DISTANCE_SQR;

        handIKConstraint.weight = Mathf.MoveTowards(handIKConstraint.weight, isWithinRange ? 1f : 0f, IK_BLEND_SPEED * Time.deltaTime);
    }

    #endregion

    private void CheckForDoors() {
        doorCheckElapsedTime += Time.deltaTime;

        // Check for nearby doors every 3 seconds and open them if found
        if (doorCheckElapsedTime >= doorCheckInterval) {

            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, doorOpenRadius, hitColliders);

            for (int i = 0; i < numColliders; i++) {
                Collider hit = hitColliders[i];

                if (hit.TryGetComponent<Door>(out Door door)) {
                    door.OpenDoor();
                    return;
                }
            }

            doorCheckElapsedTime = 0;
        }
    }

    #endregion

    #region Public Methods

    #endregion
}