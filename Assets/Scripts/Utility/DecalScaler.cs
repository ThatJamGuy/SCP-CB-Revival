using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class DecalScaler : MonoBehaviour {
    public Vector3 startSize = new Vector3(1, 1, 1);
    public Vector3 targetSize = new Vector3(5, 5, 5);
    public float duration = 2f;
    public AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    DecalProjector decal;
    float timer;

    void Awake() => decal = GetComponent<DecalProjector>();

    void OnEnable() {
        timer = 0;
        decal.size = startSize;
        StartCoroutine(ScaleDecal());
    }

    System.Collections.IEnumerator ScaleDecal() {
        while (timer < duration) {
            float t = easing.Evaluate(timer / duration);
            decal.size = Vector3.LerpUnclamped(startSize, targetSize, t);
            timer += Time.deltaTime;
            yield return null;
        }
        decal.size = targetSize;
    }
}