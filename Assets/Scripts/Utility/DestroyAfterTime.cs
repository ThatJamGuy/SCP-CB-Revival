using System.Collections;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour {
    [SerializeField] private float timeUntilDestroy;

    private void Start() {
        StartCoroutine(DestroyAfterTimeRoutine());
    }

    private IEnumerator DestroyAfterTimeRoutine() {
        yield return new WaitForSeconds(timeUntilDestroy);
        Destroy(gameObject);
    }
}