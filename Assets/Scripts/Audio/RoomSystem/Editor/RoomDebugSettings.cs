using UnityEditor;

public static class RoomDebugSettings
{
    private const string ShowRoomDebugsKey = "CBR.ShowRoomDebugs";
    private const string MenuPath = "SCP:CBR/Show Room Debugs";

    public static bool ShowRoomDebugs
    {
        get => EditorPrefs.GetBool(ShowRoomDebugsKey, false);
        set
        {
            EditorPrefs.SetBool(ShowRoomDebugsKey, value);
            SceneView.RepaintAll();
        }
    }

    [MenuItem(MenuPath)]
    private static void Toggle() => ShowRoomDebugs = !ShowRoomDebugs;

    [MenuItem(MenuPath, true)]
    private static bool ToggleValidate()
    {
        Menu.SetChecked(MenuPath, ShowRoomDebugs);
        return true;
    }
}