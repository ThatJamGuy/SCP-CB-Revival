using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class SceneLoader : MonoBehaviour {
    public static SceneLoader Instance;
    [SerializeField] string loadingScene = "Loader";
    [SerializeField] float minLoadTime = 3f;
    bool isLoading;

    void Awake() {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void LoadScene(string sceneName, bool requireUserInput = false) {
        if (isLoading) return;
        StartCoroutine(LoadRoutine(sceneName, requireUserInput));
    }

    IEnumerator LoadRoutine(string sceneName, bool requireUserInput) {
        isLoading = true;
        SceneManager.LoadScene(loadingScene);
        yield return null;
        yield return null;

        LoadingUI ui = null;
        float wait = 0f;
        while (ui == null && wait < 2f) {
            ui = FindFirstObjectByType<LoadingUI>();
            wait += Time.unscaledDeltaTime;
            yield return null;
        }

        float elapsed = 0f;
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        while (!op.isDone) {
            elapsed += Time.unscaledDeltaTime;
            float target = Mathf.Clamp01(op.progress / 0.9f);
            if (ui) ui.SetTargetProgress(target);
            if (op.progress >= 0.9f && elapsed >= minLoadTime) {
                if (requireUserInput) {
                    if (ui) ui.ShowContinuePrompt(true);

                    bool inputReceived = false;
                    while (!inputReceived) {
                        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) {
                            inputReceived = true;
                        } else if (Gamepad.current != null) {
                            foreach (var control in Gamepad.current.allControls) {
                                if (control is ButtonControl btn && btn.wasPressedThisFrame) {
                                    inputReceived = true;
                                    break;
                                }
                            }
                        }
                        yield return null;
                    }
                    if (ui) ui.ShowContinuePrompt(false);
                }
                op.allowSceneActivation = true;
            }
            yield return null;
        }

        isLoading = false;
    }
}