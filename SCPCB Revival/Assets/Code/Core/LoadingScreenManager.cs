using UnityEngine;

public class LoadingScreenManager : MonoBehaviour {
    [SerializeField] private GameObject[] loadingScreenInfo;

    private void Start() {
        loadingScreenInfo[Random.Range(0, loadingScreenInfo.Length)].SetActive(true);
    }
}