using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class SCP_049 : MonoBehaviour, IRoamingSCP {
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

    private Transform currentTarget;

    private float doorCheckElapsedTime = 0;
    

    #region Unity Callbacks

    private void Start() {
        EntitySystem.Instance.RegisterEntity(this);
    }

    private void Update() {
        UpdateHandIK();
        CheckForDoors();
    }

    #endregion

    #region Private Methods
    
    private void UpdateHandIK() {
        if (currentTarget == null) return;

        bool isWithinRange = (currentTarget.position - transform.position).sqrMagnitude < IK_DISTANCE_SQR;

        handIKConstraint.weight = Mathf.MoveTowards(
            handIKConstraint.weight,
            isWithinRange ? 1f : 0f,
            IK_BLEND_SPEED * Time.deltaTime
        );
    }

    private void CheckForDoors() {
        doorCheckElapsedTime += Time.deltaTime;

        // Check for nearby doors every 3 seconds and open them if found
        if (doorCheckElapsedTime >= 3f) {

            Collider[] hits = Physics.OverlapSphere(transform.position, doorOpenRadius);

            foreach (Collider hit in hits) {
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

    public void WalkTo(Vector3 position) {
        locomotionSystem.WalkToPosition(position);
    }

    public void Teleport(Vector3 position) {
        locomotionSystem.Warp(position);
    }

    #endregion
}