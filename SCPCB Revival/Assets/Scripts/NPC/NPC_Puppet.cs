using NaughtyAttributes;
using UnityEngine;

public class NPC_Puppet : MonoBehaviour
{
    [Header("Speech")]
    [SerializeField] private bool doesSpeech;
    [SerializeField, ShowIf(nameof(doesSpeech))] private AudioSource speechSource;

    [Header("Head IK")]
    public bool useHeadIK;
    [SerializeField, ShowIf(nameof(useHeadIK))] private IK_HeadTracking headTracking;
    [SerializeField, ShowIf(nameof(useHeadIK))] private bool targetMainCamera;

    [Header("Animation")]
    [SerializeField] private bool useAnimations;
    [SerializeField, ShowIf(nameof(useAnimations))] private Animator animator;
    [SerializeField, ShowIf(nameof(useAnimations))] private bool playAnimOnStart;
    [SerializeField, ShowIf(nameof(playAnimOnStart))] private AnimationClip startingAinim;
    [SerializeField, ShowIf(nameof(useAnimations))] private bool uniqueAnimSpeeds;
    [SerializeField, ShowIf(nameof(uniqueAnimSpeeds))] private float minAnimSpeed = 0.5f, maxAnimSpeed = 1;

    [Header("Movement")]
    [SerializeField] private bool enableMovement;

    private IK_PointOfInterest mainCameraPointOfInterest;
    private NPC_Locomotion locomotion;

    private void Start()
    {
        if (useAnimations && playAnimOnStart && startingAinim != null) animator.Play(startingAinim.name);

        if (useHeadIK && targetMainCamera)
        {
            mainCameraPointOfInterest = Camera.main.GetComponent<IK_PointOfInterest>();
            if (mainCameraPointOfInterest != null)
            {
                headTracking.POIs.Add(mainCameraPointOfInterest);
            }
        }

        if (useAnimations) animator.speed = uniqueAnimSpeeds ? Random.Range(minAnimSpeed, maxAnimSpeed) : 1f;

        if (enableMovement && GetComponent<NPC_Locomotion>())
            locomotion = GetComponent<NPC_Locomotion>();
        else if (enableMovement && !GetComponent<NPC_Locomotion>())
            Debug.LogWarning($"{gameObject.name} is missing a NPC_Locomotion component.");
    }

    private void Update()
    {
        if (useHeadIK && headTracking)
            headTracking.enabled = useHeadIK;

        if (!targetMainCamera && mainCameraPointOfInterest != null && headTracking.POIs.Contains(mainCameraPointOfInterest))
        {
            headTracking.POIs.Remove(mainCameraPointOfInterest);
        }
        if (targetMainCamera && mainCameraPointOfInterest != null && !headTracking.POIs.Contains(mainCameraPointOfInterest))
        {
            headTracking.POIs.Insert(0, mainCameraPointOfInterest);
        }
    }

    public void ToggleLookAtCamera(bool lookAtCamera)
    {
        targetMainCamera = lookAtCamera;

        if (useHeadIK && targetMainCamera)
        {
            mainCameraPointOfInterest = Camera.main.GetComponent<IK_PointOfInterest>();
        }
        else if (!targetMainCamera && mainCameraPointOfInterest != null && headTracking.POIs.Contains(mainCameraPointOfInterest))
        {
            headTracking.POIs.Remove(mainCameraPointOfInterest);
        }
    }

    public void SetAnimatorSpeed(float speed) => animator.speed = speed;

    public void PlayAnimationConditional(string animTrigger, float speedToPlay = 1f)
    {
        if (!useAnimations || !animator) return;

        animator.speed = speedToPlay;
        animator.applyRootMotion = true;
        animator.SetTrigger(animTrigger);
    }

    public void MoveAgent(Transform destination)
    {
        if (!enableMovement || !locomotion) return;
        locomotion.WalkToPosition(destination);
    }

    public void Say(AudioClip clip)
    {
        if (!doesSpeech || !speechSource)
        {
            Debug.LogWarning($"{gameObject.name} is not set to use speech or missing a speech source.");
            return;
        }

        speechSource.clip = clip;
        speechSource.Play();
    }
}