using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class DecalScaler : MonoBehaviour {
    [SerializeField] private Vector3 startSize = new Vector3(1, 1, 1);
    [SerializeField] private Vector3 targetSize = new Vector3(5, 5, 5);
    [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float duration = 2f;

    private DecalProjector decProjector;
    private float elapsedTime;

    #region Unity Lifecycle

    private void Awake() {
        decProjector = GetComponent<DecalProjector>();
    }

    private void OnEnable() {
        elapsedTime = 0;
        decProjector.size = startSize;

        StartCoroutine(ScaleDecal());
    }

    #endregion

    #region Private IEnumerators

    private IEnumerator ScaleDecal() {
        while (elapsedTime < duration) {
            float toInterpolate = easing.Evaluate(elapsedTime / duration);

            decProjector.size = Vector3.LerpUnclamped(startSize, targetSize, toInterpolate);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        decProjector.size = targetSize;
    }

    #endregion
}