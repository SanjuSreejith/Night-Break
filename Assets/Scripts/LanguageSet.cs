using UnityEngine;
using UnityEngine.UI;

public class LanguageSelector : MonoBehaviour
{
    public Button englishButton;
    public Button hindiButton;
    public Button malayalamButton;

    private string currentLanguage;

    void Start()
    {
        currentLanguage = PlayerPrefs.GetString("GameLanguage", "Hindi");
        ApplyLanguage(currentLanguage);

        // Hook up buttons
        englishButton.onClick.AddListener(() => SetLanguage("English"));
        hindiButton.onClick.AddListener(() => SetLanguage("Hindi"));
        malayalamButton.onClick.AddListener(() => SetLanguage("Malayalam"));
    }

    void SetLanguage(string lang)
    {
        if (currentLanguage == lang) return;

        currentLanguage = lang;
        PlayerPrefs.SetString("GameLanguage", lang);
        PlayerPrefs.Save();

        ApplyLanguage(lang);
        Debug.Log("Language set to: " + lang);
    }

    void ApplyLanguage(string lang)
    {
        englishButton.interactable = lang != "English";
        hindiButton.interactable = lang != "Hindi";
        malayalamButton.interactable = lang != "Malayalam";
    }
}
