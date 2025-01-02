using UnityEngine;

public class PlayerSettings : MonoBehaviour
{
    public static PlayerSettings Instance;

    [Header("Graphics Settings")]
    public bool enableFPSCounter;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
}