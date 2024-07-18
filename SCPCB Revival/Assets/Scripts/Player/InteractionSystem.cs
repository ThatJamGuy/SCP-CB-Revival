using UnityEngine;
using UnityEngine.UI;

public class InteractionSystem : MonoBehaviour
{
    [Header("Target Settings")]
    public string targetTag = "I_Interactable";
    public int interactRadius;

    [Header("UI Settings")]
    public Canvas canvas;
    public Image interactDisplay;

    [Header("Interactable Object")]
    public GameObject currentInteractible;

    private void Update()
    {
        FindClosestInteractable();
    }

    private void FindClosestInteractable()
    {
        GameObject[] targetObjects = GameObject.FindGameObjectsWithTag(targetTag);

        float closestDistance = float.MaxValue;
        GameObject closestInteractable = null;

        foreach (GameObject targetObject in targetObjects)
        {
            float distance = Vector3.Distance(transform.position, targetObject.transform.position);

            if (distance <= interactRadius && distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = targetObject;
            }
        }

        UpdateInteractableDisplay(closestInteractable);
    }

    private void UpdateInteractableDisplay(GameObject interactable)
    {
        if (interactable != null)
        {
            currentInteractible = interactable;

            Vector2 screenPosition = Camera.main.WorldToScreenPoint(currentInteractible.transform.position);
            Vector2 canvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), screenPosition, null, out canvasPosition);

            interactDisplay.rectTransform.anchoredPosition = canvasPosition;
            interactDisplay.gameObject.SetActive(true);

            if (Input.GetMouseButton(0))
            {
                if (currentInteractible.GetComponent<DoorButton>())
                {
                    currentInteractible.GetComponent<DoorButton>().UseButton();
                    Debug.Log("[!] " + currentInteractible + " was detected as a Button!");
                }
                else
                {
                    Debug.Log("[!] " + currentInteractible + " doesn't have any function! Ignoring...");
                }
            }
        }
        else
        {
            currentInteractible = null;
            interactDisplay.gameObject.SetActive(false);
        }
    }
}