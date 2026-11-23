using UnityEngine;


/// <summary>
/// Handles all updates related to the room system. The main draw of this system is complete customization of every single room. 
/// However it also requires a bit of extra authoring work on behalf of the user to manually set up the room trigger to be the exact size of the room its representing. 
/// see https://github.com/ThatJamGuy/SCP-CB-Revival/pull/32
/// </summary>
public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;
    public RoomConfig currentRoomConfig;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Sets the active room config. For multiplayer, make sure each player can manage their own changes to the appropreate systems.
    /// </summary>
    public void UpdateRoomConfig(RoomTrigger roomTrigger)
    {
        currentRoomConfig = roomTrigger.roomConfig;
        AudioManager.Instance.UpdateRoom(roomTrigger);
        PostProcessingManager.Instance.UpdateRoom(roomTrigger);
    }

    /// <summary>
    /// Subscribes the roommanager to the players listener event.
    /// </summary>
    [ContextMenu("Force Update Rooms")]
    public void UpdateRegistry()
    {
        Player.Instance.roomListener.OnRoomChanged += UpdateRoomConfig;
        Player.Instance.roomListener.ManualUpdate();
    }


    //Editor bullshitery, this doesnt need to be compiled as its purely designer stuff. 
#if UNITY_EDITOR
    private const string ShowRoomDebugsKey = "CBR.ShowRoomDebugs";
    private const string RoomDebugsMenuPath = "SCP:CBR/Show Room Debugs";

    public static bool showRoomDebugs
    {
        get => UnityEditor.EditorPrefs.GetBool(ShowRoomDebugsKey, false);
        set
        {
            UnityEditor.EditorPrefs.SetBool(ShowRoomDebugsKey, value);
            UnityEditor.SceneView.RepaintAll();
        }
    }

    [UnityEditor.MenuItem(RoomDebugsMenuPath)]
    public static void ToggleRoomDebugs()
    {
        showRoomDebugs = !showRoomDebugs;
    }

    // Validate function: runs when the menu is drawn, used to show the tick
    [UnityEditor.MenuItem(RoomDebugsMenuPath, true)]
    private static bool ToggleRoomDebugsValidate()
    {
        UnityEditor.Menu.SetChecked(RoomDebugsMenuPath, showRoomDebugs);
        return true;
    }
#endif
}