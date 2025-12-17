using UnityEngine;
using UnityEngine.UI;

public class QualitySettingsMenu : MonoBehaviour
{
    public Dropdown qualityDropdown;

    private void Start()
    {
        // Load saved quality settings or default to Medium (index 1)
        int savedQuality = PlayerPrefs.GetInt("QualitySetting", 1);
        SetQuality(savedQuality);
        qualityDropdown.value = savedQuality;

        // Add listener to handle dropdown changes
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
    }

    private void OnQualityChanged(int qualityIndex)
    {
        SetQuality(qualityIndex);

        // Automatically save the setting
        PlayerPrefs.SetInt("QualitySetting", qualityIndex);
        PlayerPrefs.Save();

        Debug.Log($"Quality set to: {QualitySettings.names[qualityIndex]} and saved.");
    }

    private void SetQuality(int qualityIndex)
    {
        // Apply the quality settings
        QualitySettings.SetQualityLevel(qualityIndex);
    }
}
