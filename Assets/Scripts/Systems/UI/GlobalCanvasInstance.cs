using UnityEngine;

public class GlobalCanvasInstance : MonoBehaviour {
    public static GlobalCanvasInstance Instance { get; private set; }

    [Header("References")]
    public GameObject achievementsScreen;
    public GameObject optionsScreen;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Public method to switch the active state of the achievements menu, mostly via that one button in pause menu
    /// </summary>
    /// <param name="active">Boolean whether the achievements screen should be set to active</param>
    public static void ToggleAchievementsMenu(bool active) {
        Instance.achievementsScreen.SetActive(active);
    }

    public static void ToggleOptionsMenu(bool active) {
        Instance.optionsScreen.SetActive(active);
    }
}