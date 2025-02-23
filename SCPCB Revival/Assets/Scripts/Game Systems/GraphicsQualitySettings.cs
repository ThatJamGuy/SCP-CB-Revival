using TMPro;
using UnityEngine;

public class GraphicsQualitySettings : MonoBehaviour
{
    [SerializeField] TMP_Dropdown qualityLevelDropdown;

    void Start()
    {
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("QualityLevel", 2), false);
        SetQualityLevelDropdown(QualitySettings.GetQualityLevel());

        if (qualityLevelDropdown != null)
        {
            qualityLevelDropdown.ClearOptions();
            qualityLevelDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
            qualityLevelDropdown.value = QualitySettings.GetQualityLevel();
            qualityLevelDropdown.onValueChanged.AddListener(SetQualityLevelDropdown);
        }
    }

    public void SetQualityLevelDropdown(int index)
    {
        QualitySettings.SetQualityLevel(index, false);
    }
}
