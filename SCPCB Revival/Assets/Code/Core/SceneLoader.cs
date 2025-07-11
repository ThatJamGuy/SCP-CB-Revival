using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace scpcbr {
    public enum LoadType { LoadHeavy, LoadLight, LoadMenu }

    public class SceneLoader : MonoBehaviour {
        public static SceneLoader Instance;
        public AsyncOperation CurrentLoadOp;
        public LoadType CurrentLoadType;
        public bool CanActivateScene;
        public AudioSource loadCompleteStinger;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            } else Destroy(gameObject);
        }

        public void LoadScene(string loadingScene, string targetScene, LoadType loadType) {
            StartCoroutine(LoadSequence(loadingScene, targetScene, loadType));
        }

        IEnumerator LoadSequence(string loadingScene, string targetScene, LoadType loadType) {
            CurrentLoadType = loadType;
            CanActivateScene = loadType == LoadType.LoadMenu;
            yield return SceneManager.LoadSceneAsync(loadingScene, LoadSceneMode.Additive);
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(LoadTargetScene(targetScene, loadingScene));
        }

        IEnumerator LoadTargetScene(string targetScene, string loadingScene) {
            CurrentLoadOp = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
            CurrentLoadOp.allowSceneActivation = false;

            while (CurrentLoadOp.progress < 0.9f)
                yield return null;

            loadCompleteStinger?.Play();

            if (CurrentLoadType == LoadType.LoadHeavy)
                yield return StartCoroutine(HeavyBackgroundTasks());

            if (CurrentLoadType == LoadType.LoadLight)
                yield return StartCoroutine(LightBackgroundTasks());

            if (CurrentLoadType != LoadType.LoadMenu) {
                while (!CanActivateScene) yield return null;
            }

            CurrentLoadOp.allowSceneActivation = true;
            while (!CurrentLoadOp.isDone) yield return null;

            SceneManager.UnloadSceneAsync(loadingScene);
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetScene));
        }

        IEnumerator HeavyBackgroundTasks() {
            // Example: yield return StartCoroutine(MapGenerationRoutine());
            yield return new WaitForSeconds(1.5f);
        }

        IEnumerator LightBackgroundTasks() {
            yield return new WaitForSeconds(0.5f);
        }

        public void ConfirmContinue() {
            CanActivateScene = true;
        }
    }
}