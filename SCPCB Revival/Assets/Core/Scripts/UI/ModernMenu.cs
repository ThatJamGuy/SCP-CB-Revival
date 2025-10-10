using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace scpcbr {
    public class ModernMenu : MonoBehaviour {
        [Header("New Game Fields")]
        [SerializeField] private TMP_InputField saveNameInput;
        [SerializeField] private TMP_InputField seedInput;
        [SerializeField] private Toggle introToggle;

        [Header("Loading Screen")]
        [SerializeField] private GameObject loadingScreenPrefab;

        private void Start() {
            MusicPlayer.Instance.StartMusicByName("Menu");
        }

        public void StartGame() {
            if (saveNameInput.text == "" || seedInput.text == "") return;
            SaveDataManager.Instance.CreateSaveData(saveNameInput.text, seedInput.text, introToggle.isOn);
            SceneLoader.Instance.LoadScene("GamePreBreach", true);
        }

        public void OpenLink(string link) {
            Application.OpenURL(link);
        }

        public void QuitGame() {
            Application.Quit();
        }
    }
}