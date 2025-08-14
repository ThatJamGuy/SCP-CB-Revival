using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace scpcbr {
    public class LoadingScreenManager : MonoBehaviour {
        [SerializeField] GameObject[] loadingScreenInfo;
        [SerializeField] Slider progressBar;
        [SerializeField] TextMeshProUGUI progressText;
        [SerializeField] TextMeshProUGUI continuePromptText;
        [SerializeField] float minDisplayTime = 2f;

        bool canContinue;
        float loadStartTime;

        void Start() {
            //if (loadingScreenInfo.Length > 0)
            //loadingScreenInfo[Random.Range(0, loadingScreenInfo.Length)].SetActive(true);
            // KEEPING THE THINGS OFF FOR NOW UNTIL I CAN GET SOMETHING BETTER

            MusicPlayer.Instance.ChangeMusic("Loading");

            if (continuePromptText) continuePromptText.gameObject.SetActive(false);
            loadStartTime = Time.time;
        }

        void Update() {
            var loader = SceneLoader.Instance;
            if (loader.CurrentLoadOp != null) {
                float progress = Mathf.Clamp01(loader.CurrentLoadOp.progress / 0.9f);
                progressBar.value = progress;
                progressText.text = $"Loading - {(progress * 100):0}%";

                bool ready = progress >= 1f &&
                             loader.CurrentLoadType != LoadType.LoadMenu &&
                             !loader.CanActivateScene &&
                             Time.time - loadStartTime >= minDisplayTime;

                if (continuePromptText) continuePromptText.gameObject.SetActive(ready);
                canContinue = ready;
            }

            if (canContinue && AnyInputPressed()) {
                SceneLoader.Instance.ConfirmContinue();
                canContinue = false;
                if (continuePromptText) continuePromptText.gameObject.SetActive(false);
            }
        }

        bool AnyInputPressed() {
            return Keyboard.current.anyKey.wasPressedThisFrame ||
                   Mouse.current.leftButton.wasPressedThisFrame ||
                   Mouse.current.rightButton.wasPressedThisFrame ||
                   Mouse.current.middleButton.wasPressedThisFrame;
        }
    }
}