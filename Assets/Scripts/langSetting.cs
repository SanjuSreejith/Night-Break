using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public enum Language { English, Hindi, Malayalam }
    public enum GameMode { Normal, Endless }

    public static Language CurrentLanguage { get; private set; } = Language.Hindi;
    public static GameMode CurrentMode { get; set; } = GameMode.Normal;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // persist between scenes
        SetLanguage(PlayerPrefs.GetString("GameLanguage", "Hindi"));
    }

    public static void SetLanguage(string langCode)
    {
        PlayerPrefs.SetString("GameLanguage", langCode);
        PlayerPrefs.Save();

        switch (langCode)
        {
            case "Hindi":
                CurrentLanguage = Language.Hindi;
                break;
            case "Malayalam":
                CurrentLanguage = Language.Malayalam;
                break;
            default:
                CurrentLanguage = Language.English;
                break;
        }
    }

    public static Language GetCurrentLanguage()
    {
        return CurrentLanguage;
    }

    public static void SetGameMode(GameMode mode)
    {
        CurrentMode = mode;
    }

    public static bool IsEndlessMode()
    {
        return CurrentMode == GameMode.Endless;
    }
}
