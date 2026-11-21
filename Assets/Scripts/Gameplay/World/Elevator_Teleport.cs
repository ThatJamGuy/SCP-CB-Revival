using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Elevator_Teleport : MonoBehaviour {
    private const float ELEVATOR_MOVE_TIME = 8;
    private const int MAX_CALL_INDEX = 40;

    private static readonly WaitForSeconds ElevatorMoveTime = new WaitForSeconds(ELEVATOR_MOVE_TIME);

    private enum ElevatorState {
        Idle,
        Closing, // Departure was requested, just waiting on the door to close
        Moving
    }

    [SerializeField] private Elevator_Teleport_Floor floorA;
    [SerializeField] private Elevator_Teleport_Floor floorB;
    [SerializeField] private Elevator_Teleport_Floor startingFloor;

    public UnityEvent OnPlayerUsedElevator;

    private ElevatorState state = ElevatorState.Idle;
    private Elevator_Teleport_Floor currentFloor;
    private int currentWarningIndex;

    #region Unity Lifecycle

    private void Awake() {
        currentFloor = startingFloor;
        startingFloor.Door.startOpen = true;
    }

    #endregion

    #region Private Methods

    private void RequestDeparture() {
        state = ElevatorState.Closing;

        if (currentFloor.Door.isOpen) currentFloor.Door.CloseDoor();
        else Depart();
    }

    private void Depart() {
        if (state == ElevatorState.Moving) return;

        state = ElevatorState.Moving;
        StartCoroutine(MoveElevator(currentFloor, GetOtherFloor(currentFloor)));
    }

    private void HandleButtonSpam() {
        currentWarningIndex++;

        switch (currentWarningIndex) {
            case 2:
                RevivalSessionEngine.Instance.NotifyPlayer("You already called the elevator.");
                break;
            case 10:
                RevivalSessionEngine.Instance.NotifyPlayer("Stop spamming the button.");
                break;
            case 20:
                RevivalSessionEngine.Instance.NotifyPlayer("Pressing it harder does not make the elevator come faster.");
                break;
            case 30:
                RevivalSessionEngine.Instance.NotifyPlayer("If you continue pressing this button I will generate a Memory Access Violation.");
                break;
        }

        if (currentWarningIndex >= MAX_CALL_INDEX) currentWarningIndex = 1;
    }

    private static void TeleportPlayer(Transform from, Transform to) {
        Transform player = Player.Instance.transform;

        Vector3 localPos = from.InverseTransformPoint(player.position);
        Vector3 newPos = to.transform.TransformPoint(localPos);
        Quaternion delta = to.rotation * Quaternion.Inverse(from.rotation);

        player.SetPositionAndRotation(newPos, delta * player.rotation);
        Player.Instance.ForceBlink();
    }

    private static void TeleportNpcs(Elevator_Teleport_Floor from, Elevator_Teleport_Floor to) {
        System.Collections.Generic.List<UnityEngine.AI.NavMeshAgent> npcs = from.NPCsInside;

        for (int i = 0; i < npcs.Count; i++) {
            UnityEngine.AI.NavMeshAgent npc = npcs[i];

            if (npc == null) continue;

            GetNPCDestination(from.transform, to.transform, npc.transform, out Vector3 newPos, out Quaternion newRot);

            npc.Warp(newPos);
            npc.transform.rotation = newRot;

            to.AddNPC(npc);
        }

        npcs.Clear();
    }

    private static void GetNPCDestination(Transform from, Transform to, Transform target, out Vector3 position, out Quaternion rotation) {
        position = to.TransformPoint(from.InverseTransformPoint(target.position));
        rotation = to.rotation * Quaternion.Inverse(from.rotation) * target.rotation;
    }

    #endregion

    #region Private Coroutines

    private IEnumerator MoveElevator(Elevator_Teleport_Floor from, Elevator_Teleport_Floor to) {
        if (from.PlayerInside) {
            from.LightFlicker.SetActive(true);
            AudioManager.PlayOneShot(AudioManager.Instance.globalAudioContainer.elevatorMoving);
            RevivalRuntimeEngine.Instance.ShakeCamera(0.03f, 0.06f, ELEVATOR_MOVE_TIME - 1);

            OnPlayerUsedElevator.Invoke();
        }

        yield return ElevatorMoveTime;

        if (from.PlayerInside) {
            TeleportPlayer(from.transform, to.transform);
            from.LightFlicker.SetActive(false);
            from.PlayerInside = false;
            to.PlayerInside = true;
        }

        TeleportNpcs(from, to);

        currentFloor = to;
        state = ElevatorState.Idle;
        currentWarningIndex = 0;

        to.Door.OpenDoor();
    }

    #endregion

    #region Private Extras

    private Elevator_Teleport_Floor GetOtherFloor(Elevator_Teleport_Floor floor) {
        return floor == floorA ? floorB : floorA;
    }

    #endregion

    #region Public Methods

    public void OnOutsideButtonPressed(Elevator_Teleport_Floor floor) {
        if (state != ElevatorState.Idle) {
            HandleButtonSpam();
            return;
        }

        if (floor == currentFloor) {
            if (floor.Door.isOpen) floor.Door.CloseDoor();
            else floor.Door.OpenDoor();
            return;
        }

        currentWarningIndex = 1;

        RevivalSessionEngine.Instance.NotifyPlayer("You called the elevator.");
        RequestDeparture();
    }

    public void OnInsideButtonPressed(Elevator_Teleport_Floor floor) {
        if (state != ElevatorState.Idle) {
            RevivalSessionEngine.Instance.NotifyPlayer("You cannot use the elevator while it is moving.");
            return;
        }

        if (floor == currentFloor) RequestDeparture();
    }

    public void OnElevatorDoorClosed(Elevator_Teleport_Floor floor) {
        if (state != ElevatorState.Closing || floor != currentFloor) return;

        Depart();
    }

    #endregion
}