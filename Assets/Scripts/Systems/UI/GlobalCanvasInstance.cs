using UnityEngine;

public class GlobalCanvasInstance : MonoBehaviour {
    public static GlobalCanvasInstance Instance { get; private set; }
    
    [Header("References")] 
    public GameObject achievementsScreen;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    /// <summary>
    /// Public method to switch the active state of the achievements menu, mostly via that one button in pause menu
    /// </summary>
    /// <param name="active">Boolean whether the achievements screen should be set to active</param>
    public static void ToggleAchievementsMenu(bool active) {
        GlobalCanvasInstance.Instance.achievementsScreen.SetActive(active);
    }
}