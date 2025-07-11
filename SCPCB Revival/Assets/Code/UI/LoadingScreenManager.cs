using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace scpcbr {
    public class LoadingScreenManager : MonoBehaviour {
        [SerializeField] private GameObject[] loadingScreenInfo;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI continuePromptText;

        private bool canContinue = false;

        private void Start() {
            //loadingScreenInfo[Random.Range(0, loadingScreenInfo.Length)].SetActive(true);

            //MusicPlayer.Instance.ChangeMusic("Loading");

            if (continuePromptText != null)
                continuePromptText.gameObject.SetActive(false);
        }

        private void Update() {
            if (SceneLoader.Instance.CurrentLoadOp != null) {
                float normalizedProgress = Mathf.Clamp01(SceneLoader.Instance.CurrentLoadOp.progress / 0.9f);
                progressBar.value = normalizedProgress;
                progressText.text = $"Loading - {(normalizedProgress * 100):0}%";

                bool ready = SceneLoader.Instance.CurrentLoadOp.progress >= 0.9f &&
                    SceneLoader.Instance.CurrentLoadType != LoadType.LoadMenu &&
                    !SceneLoader.Instance.CanActivateScene;

                if (continuePromptText != null)
                    continuePromptText.gameObject.SetActive(ready);
                canContinue = ready;
            }
            if (canContinue && (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))) {
                SceneLoader.Instance.ConfirmContinue();
                canContinue = false;
                if (continuePromptText != null)
                    continuePromptText.gameObject.SetActive(false);
            }
        }
    }
}