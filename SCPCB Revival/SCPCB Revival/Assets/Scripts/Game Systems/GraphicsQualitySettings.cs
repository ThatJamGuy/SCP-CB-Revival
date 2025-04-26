using TMPro;
using UnityEngine;

public class GraphicsQualitySettings : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown qualityLevelDropdown;

    void Start()
    {
        if (qualityLevelDropdown != null)
        {
            qualityLevelDropdown.ClearOptions();
            qualityLevelDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
            int savedQualityLevel = PlayerPrefs.GetInt("QualityLevel", 2);
            QualitySettings.SetQualityLevel(savedQualityLevel, false);
            qualityLevelDropdown.value = savedQualityLevel;
            qualityLevelDropdown.onValueChanged.AddListener(OnQualityLevelChanged);
        }
    }

    public void OnQualityLevelChanged(int index)
    {
        QualitySettings.SetQualityLevel(index, false);
        PlayerPrefs.SetInt("QualityLevel", index);
        PlayerPrefs.Save();
    }
}