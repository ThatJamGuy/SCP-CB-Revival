using UnityEngine;
using PrimeTween;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerBlink : MonoBehaviour {
    [SerializeField] private float blinkDrainRate = 0.07f;
    [SerializeField] private float blinkOverlayDuration = 0.2f;

    private InputAction blinkAction;
    private bool isBlinkingActive = false;
    private bool isBlinking = false;
    private float blinkTimer = 1f;

    private void OnEnable() {
        blinkAction = InputManager.Instance.blinkAction;
        blinkAction.started += OnBlinkStarted;
        blinkAction.canceled += OnBlinkCanceled;
    }

    private void OnDisable() {
        if (blinkAction != null) {
            blinkAction.started -= OnBlinkStarted;
            blinkAction.canceled -= OnBlinkCanceled;
        }
    }

    private void Update() {
        if (!isBlinkingActive || isBlinking || PlayerAccessor.instance.infiniteBlink) return;

        float drain = blinkDrainRate * (1 + PlayerAccessor.instance.blinkDepletionModifier);
        blinkTimer = Mathf.MoveTowards(blinkTimer, 0f, drain * Time.deltaTime);
        CanvasInstance.instance.blinkBar.value = blinkTimer;

        if (blinkTimer <= 0f) TriggerBlink();
    }

    public void StartBlink() {
        if (isBlinkingActive) return;
        CanvasInstance.instance.blinkBar.gameObject.SetActive(true);
        Tween.Alpha(CanvasInstance.instance.blinkBarFill, 0f, 1f, 5f);
        Tween.Alpha(CanvasInstance.instance.blinkBarBackground, 0f, 1f, 5f);
        isBlinkingActive = true;
    }

    private void OnBlinkStarted(InputAction.CallbackContext context) {
        if (!isBlinkingActive || isBlinking) return;
        TriggerBlink();
    }

    private void OnBlinkCanceled(InputAction.CallbackContext context) {
        if (!isBlinkingActive || !isBlinking) return;
        CancelInvoke(nameof(EndBlink));
        Invoke(nameof(EndBlink), blinkOverlayDuration);
    }

    private void TriggerBlink() {
        if (!isBlinkingActive || isBlinking) return;

        blinkTimer = 1f;
        CanvasInstance.instance.blinkBar.value = 0f;
        isBlinking = true;
        PlayerAccessor.instance.isBlinking = true;
        CanvasInstance.instance.blinkOverlay.SetActive(true);

        if (!InputManager.Instance.IsBlinkHeld) {
            Invoke(nameof(EndBlink), blinkOverlayDuration);
        }
    }

    private void EndBlink() {
        if (!isBlinkingActive) return;

        blinkTimer = 1f;
        CanvasInstance.instance.blinkBar.value = 1f;
        isBlinking = false;
        PlayerAccessor.instance.isBlinking = false;
        CanvasInstance.instance.blinkOverlay.SetActive(false);
    }

    public void StopBlink() {
        Tween.Alpha(CanvasInstance.instance.blinkBarFill, 1f, 0f, 1f);
        Tween.Alpha(CanvasInstance.instance.blinkBarBackground, 1f, 0f, 1f);
        StartCoroutine(DisableBlinkCoroutine());
    }

    private IEnumerator DisableBlinkCoroutine() {
        yield return new WaitForSeconds(5);
        CanvasInstance.instance.blinkBar.gameObject.SetActive(false);
        isBlinkingActive = false;
    }
}