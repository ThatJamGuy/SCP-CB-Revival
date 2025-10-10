using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace scpcbr {
    public class PlayerBlink : MonoBehaviour {
        [Header("Input Actions")]
        public InputActionAsset playerControls;

        [Header("References")]
        public PlayerBase playerBase;
        public Image blinkOverlay;
        public Slider blinkSlider;

        [Header("Blink Settings")]
        public float blinkDrainRate = 0.05f;
        public float blinkOverlayDuration = 0.2f;

        private InputAction blinkAction;
        private float blinkTimer = 1f;
        private bool isHoldingBlink;

        #region Enable/Disable
        private void OnEnable() {
            playerControls.Enable();
            blinkAction = playerControls.FindAction("Blink", true);
            blinkAction.started += OnBlinkStarted;
            blinkAction.canceled += OnBlinkCanceled;
        }

        private void OnDisable() {
            if (blinkAction != null) {
                blinkAction.started -= OnBlinkStarted;
                blinkAction.canceled -= OnBlinkCanceled;
            }
            playerControls.Disable();
        }
        #endregion

        #region Default Methods
        private void Update() {
            if (playerBase.infiniteBlink && playerBase.isBlinking) return;
            if (isHoldingBlink) return;

            float drain = blinkDrainRate * (1 + playerBase.blinkDepletionModifier);
            blinkTimer = Mathf.MoveTowards(blinkTimer, 0f, drain * Time.deltaTime);
            blinkSlider.value = blinkTimer;

            if (blinkTimer <= 0f) TriggerBlink();
        }
        #endregion

        #region Private Methods
        private void OnBlinkStarted(InputAction.CallbackContext context) {
            isHoldingBlink = true;
            TriggerBlink();
        }

        private void OnBlinkCanceled(InputAction.CallbackContext context) {
            isHoldingBlink = false;
            if (playerBase.isBlinking) {
                CancelInvoke(nameof(EndBlink));
                Invoke(nameof(EndBlink), blinkOverlayDuration);
            }
        }

        private void TriggerBlink() {
            blinkTimer = 1f;
            blinkSlider.value = 1f;
            playerBase.isBlinking = true;
            blinkOverlay.enabled = true;
            if (!isHoldingBlink) Invoke(nameof(EndBlink), blinkOverlayDuration);
        }

        private void EndBlink() {
            blinkTimer = 1f;
            blinkSlider.value = 1f;
            playerBase.isBlinking = false;
            blinkOverlay.enabled = false;
        }
        #endregion
    }
}