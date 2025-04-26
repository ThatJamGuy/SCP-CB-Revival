using System.Collections;
using UnityEngine;

/// <summary>
/// A script created mainly for the intro sequence to help with loading new areas of the intro without lag spikes.
/// </summary>
public class BatchProcessing : MonoBehaviour
{
    [SerializeField] private GameObject parentObject;
    [SerializeField] private float operationDelay = 0.5f;

    private Coroutine activationCoroutine;

    public void ActivateObjects(bool activate)
    {
        StartCoroutine(ActivateObjectsCoroutine(activate));
    }

    private IEnumerator ActivateObjectsCoroutine(bool activate)
    {
        Transform parentTransform = parentObject.transform;

        if (parentTransform.childCount == 0)
        {
            Debug.LogWarning("The parent object has no children to manage.");
            yield break;
        }

        foreach (Transform child in parentTransform)
        {
            if (child.gameObject != null)
            {
                child.gameObject.SetActive(activate);
            }

            yield return new WaitForSeconds(operationDelay);
        }
    }
}