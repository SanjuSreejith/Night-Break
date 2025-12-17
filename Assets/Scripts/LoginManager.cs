/*using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class LoginManager : MonoBehaviour
{
    public GameObject loginPanel;
    public TextMeshProUGUI statusText;
    public Button loginButton;
    public Button guestButton;
    public string mainMenuSceneName = "MainMenu";

    private const string LoginAttemptKey = "PlayGamesLoginAttempts";
    private const int MaxLoginAttempts = 6;

    void Start()
    {
        int attempts = PlayerPrefs.GetInt(LoginAttemptKey, 0);

#if UNITY_ANDROID && !UNITY_EDITOR
        PlayGamesPlatform.Activate();

        Social.localUser.Authenticate(success =>
        {
            if (success)
            {
                statusText.text = "Welcome, " + Social.localUser.userName;
                LoadCloudAndMenu();
            }
            else if (attempts >= MaxLoginAttempts)
            {
                LoadMainMenu("Login skipped after " + attempts + " failed attempts");
            }
            else
            {
                ShowLoginUI("Sign in to access features");
            }
        });
#else
        ShowLoginUI("Editor mode: Login disabled");
#endif
    }

    public void ManualLogin()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Social.localUser.Authenticate(success =>
        {
            if (success)
            {
                statusText.text = "Signed in as: " + Social.localUser.userName;
                LoadCloudAndMenu();
            }
            else
            {
                IncrementAttempt();
                statusText.text = "Login failed. Try again.";
            }
        });
#else
        statusText.text = "Login not available in editor.";
#endif
    }

    public void ContinueAsGuest()
    {
        IncrementAttempt();
        LoadMainMenu("Playing as Guest");
    }

    void ShowLoginUI(string message)
    {
        if (loginPanel != null) loginPanel.SetActive(true);
        if (statusText != null) statusText.text = message;
    }

    void LoadMainMenu(string message)
    {
        Debug.Log(message);
        if (statusText != null) statusText.text = message;
        Invoke(nameof(LoadScene), 1.5f);
    }

    void LoadScene()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void IncrementAttempt()
    {
        int attempts = PlayerPrefs.GetInt(LoginAttemptKey, 0);
        PlayerPrefs.SetInt(LoginAttemptKey, attempts + 1);
        PlayerPrefs.Save();
    }

    // ✅ New method to load cloud data before menu
    void LoadCloudAndMenu()
    {
        if (CloudSaveManager.Instance == null)
        {
            Debug.LogError("CloudSaveManager not found in scene.");
            LoadScene(); // fallback
            return;
        }

        CloudSaveManager.Instance.LoadFromCloud((loadedData) =>
        {
            if (!string.IsNullOrEmpty(loadedData))
            {
                CloudDataUtility.ApplyCloudData(loadedData);
                Debug.Log("✅ Cloud data applied.");
            }
            else
            {
                Debug.Log("ℹ️ No cloud data found or load failed.");
            }

            Invoke(nameof(LoadScene), 1.5f);
        });
    }
}
*/