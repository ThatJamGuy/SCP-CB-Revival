using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public class Button : MonoBehaviour {

    [Space(5)]

    [InfoBox("Improved button script that does the same things as before, but with some more functionality.")]

    [Space(5)]

    [Header("Audio Clips")]
    [SerializeField] private AudioClip buttonPressSFX;
    [SerializeField] private AudioClip buttonFailSFX;

    [Header("Button Settings")]
    [Tooltip("Use this setting if this is a generic button you want un-pressable. Otherwise leave it alone, as locked parameters are defined in the link door script.")]
    [SerializeField] private bool considerLocked = false;
    [SerializeField, ShowIf("considerLocked")] private string lockMessage = "The button is locked.";

    [Header("Door Settings")]
    [Tooltip("If not a generic button, it will link to a door. Make sure you set isDoorButton to true.")]
    [SerializeField] private Door linkedDoor;
    [SerializeField] private bool isDoorButton = false;

    [Header("Custom Events")]
    [SerializeField] private UnityEvent onButtonPressed;

    private AudioSource buttonSource;
    private bool canInteract = true;

    private void Start() => buttonSource = GetComponent<AudioSource>();

    public void PressButton() {
        if (!canInteract || buttonSource == null) return;

        if (considerLocked) {
            NotifyAndCooldown(lockMessage, buttonFailSFX);
            return;
        }

        PlaySound(buttonPressSFX);
        onButtonPressed?.Invoke();

        if (isDoorButton && linkedDoor != null) HandleDoorInteraction();

        canInteract = false;
        StartCoroutine(Cooldown());
    }

    private void HandleDoorInteraction() {
        if (!linkedDoor.isLocked) {
            ToggleDoor();
        }
        else {
            var inv = InventorySystem.instance;
            if (linkedDoor.isBroken) {
                var msg = linkedDoor.requiresKeycard && inv.currentKeyLevel > 0
                    ? "The keycard was inserted into the slot but nothing happened."
                    : "You pushed the button but nothing happened.";
                NotifyAndCooldown(msg, buttonFailSFX);
            }
            else if (linkedDoor.requiresKeycard) {
                if (inv.currentKeyLevel >= linkedDoor.requiredKeyLevel) {
                    ToggleDoor();
                    inv.UnequipItem();
                }
                else {
                    var msg = inv.currentKeyLevel > 0
                        ? $"A keycard with security clearance {linkedDoor.requiredKeyLevel} or higher is required to operate this door."
                        : "A keycard is required to operate this door.";
                    NotifyAndCooldown(msg, buttonFailSFX);
                    if (inv.currentKeyLevel > 0) inv.UnequipItem();
                }
            }
            else {
                NotifyAndCooldown("The door appears to be locked.", buttonFailSFX);
            }
        }
    }

    private void ToggleDoor() {
        if (linkedDoor.isOpen) linkedDoor.CloseDoor();
        else linkedDoor.OpenDoor();
    }

    private void PlaySound(AudioClip clip) {
        if (clip == null) return;
        buttonSource.clip = clip;
        buttonSource.Play();
    }

    private void NotifyAndCooldown(string message, AudioClip clip) {
        PlaySound(clip);
        InfoTextManager.Instance.NotifyPlayer(message);
        canInteract = false;
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown() {
        yield return new WaitForSeconds(1f);
        canInteract = true;
    }
}