using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class GlobalLightFlicker : MonoBehaviour
{
    public static GlobalLightFlicker Instance;
    private List<Light> sceneLights;
    private Coroutine flickerCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        sceneLights = new List<Light>(FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None));
    }

    public void FlickerLights(float startIntensity, float endIntensity, float duration)
    {
        if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
        flickerCoroutine = StartCoroutine(FlickerRoutine(startIntensity, endIntensity, duration));
    }

    private IEnumerator FlickerRoutine(float startIntensity, float endIntensity, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float flickerIntensity = Mathf.Lerp(startIntensity, endIntensity, Random.value);
            foreach (Light light in sceneLights)
            {
                if (light != null)
                    light.intensity = flickerIntensity;
            }
            elapsed += Time.deltaTime;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }

        ResetLights();
    }

    private void ResetLights()
    {
        foreach (Light light in sceneLights)
        {
            if (light != null)
                light.intensity = 1f;
        }
    }
}
