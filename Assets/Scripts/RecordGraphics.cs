using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GraphicsSelectionUI : MonoBehaviour
{
    public Dropdown qualityDropdown;

    void Start()
    {
        if (qualityDropdown == null) return;

        // Get all quality levels from Project Settings
        string[] allNames = QualitySettings.names;
        int maxDetected = PlayerPrefs.GetInt("MaxSupportedQuality", allNames.Length - 1);

        // Force full range: ensure user can choose up to real max
        List<string> options = new List<string>(allNames);

        // Populate dropdown
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(options);

        // Load saved selection, clamp to real maximum
        int saved = PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
        saved = Mathf.Clamp(saved, 0, allNames.Length - 1);
        qualityDropdown.value = saved;

        qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    public void SetQuality(int idx)
    {
        // Apply user selection
        QualitySettings.SetQualityLevel(idx, true);
        PlayerPrefs.SetInt("GraphicsQuality", idx);
        PlayerPrefs.Save();
        Debug.Log($"Graphics set to: {QualitySettings.names[idx]}");
    }
}
