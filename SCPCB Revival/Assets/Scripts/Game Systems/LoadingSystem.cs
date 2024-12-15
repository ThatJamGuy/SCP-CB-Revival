using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSystem : MonoBehaviour
{
    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image[] loadingSegments;
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] private Slider testSlider;

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
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIdToLoad);

        loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);
            testSlider.value = progressValue;

            Debug.Log(progressValue);

            yield return null;
        }
    }
}