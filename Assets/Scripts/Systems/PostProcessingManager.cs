using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// handles and updates all post processing changes including those related to the room system.
/// </summary>
public class PostProcessingManager : MonoBehaviour
{
    public static PostProcessingManager Instance;

    [SerializeField] private Volume globalVolume;

    [Header("Configuration")]
    [Tooltip("Seconds to blend between room color grades. 0 = instant.")]
    [SerializeField] private float colorTransitionTime = 0.5f;

    //Private Vars
    private ColorAdjustments colorAdjustments;
    private RoomConfig currentConfig;

    private float transitionTimer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        globalVolume = FindAnyObjectByType<Volume>();
        if (globalVolume == null)
        {
            Debug.LogWarning("RoomManager: No global Volume found, room color grading will be skipped.", this);
            return;
        }

        // .profile (not .sharedProfile) so we edit a runtime instance and never the asset on disk.
        if (!globalVolume.profile.TryGet(out colorAdjustments))
            colorAdjustments = globalVolume.profile.Add<ColorAdjustments>();

        colorAdjustments.colorFilter.overrideState = true;
        colorAdjustments.saturation.overrideState = true;
    }

    public void UpdateRoom(RoomTrigger roomTrigger)
    {
        currentConfig = roomTrigger.roomConfig;
        transitionTimer = colorTransitionTime;
    }

    //using Update instead of a coroutine to prevent the cost of invoking one. Preventing a potential hitch.
    void Update()
    {
        if(transitionTimer > 0)
        {
            transitionTimer -= Time.deltaTime;
            float t = 1f - (transitionTimer / colorTransitionTime); //basic easing curve

            colorAdjustments.colorFilter.Interp(colorAdjustments.colorFilter.value, currentConfig.colorFilter, t);
            colorAdjustments.saturation.value = Mathf.Lerp(colorAdjustments.saturation.value, currentConfig.saturationValue, t);

        }
    }
}
