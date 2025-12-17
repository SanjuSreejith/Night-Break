using UnityEngine;
using UnityEngine.UI;

public class DifficultyMenuController : MonoBehaviour
{
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;

    private string currentDifficulty;

    void Start()
    {
        currentDifficulty = PlayerPrefs.GetString("GameDifficulty", "Medium");
        ApplyDifficulty(currentDifficulty);

        // Hook up buttons
        easyButton.onClick.AddListener(() => SetDifficulty("Easy"));
        mediumButton.onClick.AddListener(() => SetDifficulty("Medium"));
        hardButton.onClick.AddListener(() => SetDifficulty("Hard"));
    }

    void SetDifficulty(string difficulty)
    {
        if (currentDifficulty == difficulty) return;

        currentDifficulty = difficulty;
        PlayerPrefs.SetString("GameDifficulty", difficulty);
        PlayerPrefs.Save();

        ApplyDifficulty(difficulty);
    }

    void ApplyDifficulty(string difficulty)
    {
        easyButton.interactable = difficulty != "Easy";
        mediumButton.interactable = difficulty != "Medium";
        hardButton.interactable = difficulty != "Hard";
    }
}
