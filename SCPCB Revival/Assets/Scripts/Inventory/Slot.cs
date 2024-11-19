using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    [SerializeField] private GameObject outline;
    [SerializeField] private RectTransform rectTransform;

    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f;

    private void Awake()
    {
        if (outline != null)
            outline.SetActive(false);
    }

    private void Update()
    {
        if (IsMouseOverSlot())
        {
            ShowOutline();
        }
        else
        {
            HideOutline();
        }
    }

    private bool IsMouseOverSlot()
    {
        Vector2 mousePosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out mousePosition))
        {
            return rectTransform.rect.Contains(mousePosition);
        }
        return false;
    }

    private void ShowOutline()
    {
        if (outline != null && !outline.activeSelf)
            outline.SetActive(true);
    }

    private void HideOutline()
    {
        if (outline != null && outline.activeSelf)
            outline.SetActive(false);
    }

    public bool IsDoubleClicked()
    {
        if (Time.timeScale == 0)
        {
            float clickTime = Time.realtimeSinceStartup;
            if (clickTime - lastClickTime < doubleClickThreshold)
            {
                lastClickTime = 0;
                return true;
            }
            lastClickTime = clickTime;
        }
        return false;
    }
}