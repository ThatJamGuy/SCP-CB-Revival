using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSystem : MonoBehaviour
{
    [Header("Loading Screen")]
    [SerializeField] private bool isMapLoader;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image[] loadingSegments;
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] private TextMeshProUGUI loadingCompleteText;
    [SerializeField] private Slider testSlider;
    [SerializeField] private AudioSource loadingFinishedSFX;

    private bool playedSound = false;

    private void Start()
    {
        foreach (var segment in loadingSegments) segment.enabled = false;
    }

    private void Update()
    {
        UpdateLoadingBar(testSlider.value);
    }

    public void LoadScene(int sceneIDToLoad)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        StartCoroutine(LoadSceneAsync(sceneIDToLoad));
    }

    private void UpdateLoadingBar(float progress)
    {
        var filledSegmentsCount = Mathf.FloorToInt(progress * loadingSegments.Length);
        var filledSegments = loadingSegments.Take(filledSegmentsCount).ToList();
        var unfilledSegments = loadingSegments.Skip(filledSegmentsCount).ToList();

        filledSegments.ForEach(segment => segment.enabled = true);
        unfilledSegments.ForEach(segment => segment.enabled = false);

        percentageText.text = $"LOADING - {Mathf.RoundToInt(progress * 100)}%";
    }

    IEnumerator LoadSceneAsync(int sceneIdToLoad)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIdToLoad, LoadSceneMode.Single);
        operation.allowSceneActivation = true;

        if(isMapLoader)
            operation.allowSceneActivation = false;

        loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);
            testSlider.value = progressValue;

            Debug.Log(progressValue);

            if(isMapLoader) {
                if (progressValue >= 0.9f)
                {
                    percentageText.gameObject.SetActive(false);
                    loadingCompleteText.gameObject.SetActive(true);

                    if(!playedSound) {
                        loadingFinishedSFX.Play();  
                        playedSound = true;
                    }
                }
                if ((Input.GetMouseButtonDown(0) || Input.anyKeyDown) && progressValue >= 0.9f)
                    operation.allowSceneActivation = true;

            }

            yield return null;
        }
    }
}