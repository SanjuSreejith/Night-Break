using Firebase.Extensions;
using Firebase.RemoteConfig;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RemoteConfigScript : MonoBehaviour
{
    [Header("Update UI")]
    public GameObject updatePanel;                // UI Panel for update notice
    public TextMeshProUGUI updateMessageText;     // Message inside the update panel

    [Header("Current App Version")]
    public string currentAppVersion = "0.4.0";

    private void Awake()
    {
        Debug.Log("📦 Local startup — Current Version: " + currentAppVersion);
        Time.timeScale = 0; // Pause game while checking
        FetchAndApplyRemoteConfig();
    }

    public void OpenPlayStore()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.sagames.ananthatha");
    }

    public Task FetchAndApplyRemoteConfig()
    {
        Debug.Log("🌐 Fetching remote config...");
        return FirebaseRemoteConfig.DefaultInstance
            .FetchAsync(TimeSpan.Zero)
            .ContinueWithOnMainThread(OnRemoteConfigFetched);
    }

    private void OnRemoteConfigFetched(Task fetchTask)
    {
        if (!fetchTask.IsCompleted || fetchTask.IsFaulted)
        {
            Debug.LogError("❌ Remote Config fetch failed or not completed.");
            ResumeGame();
            return;
        }

        var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        var info = remoteConfig.Info;

        if (info.LastFetchStatus != LastFetchStatus.Success)
        {
            Debug.LogError($"❌ Fetch failed - Status: {info.LastFetchStatus}");
            ResumeGame();
            return;
        }

        remoteConfig.ActivateAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log($"✅ Remote config activated. Fetched at: {info.FetchTime}");

                string fetchedVersion = remoteConfig.GetValue("minimum_required_version").StringValue;

                if (string.IsNullOrEmpty(fetchedVersion))
                {
                    Debug.LogError("⚠️ minimum_required_version is empty or null.");
                    ResumeGame();
                    return;
                }

                Debug.Log("✅ Fetched version from Firebase: " + fetchedVersion);
                CheckVersionUpdate(fetchedVersion);
            }
            else
            {
                Debug.LogError("❌ Remote config activation failed.");
                ResumeGame();
            }
        });
    }

    void CheckVersionUpdate(string minimumVersion)
    {
        if (IsVersionLower(currentAppVersion, minimumVersion))
        {
            Debug.LogWarning($"❗ Update Required: App v{currentAppVersion} < Required v{minimumVersion}");
            updatePanel.SetActive(true);
            updateMessageText.text = $"🛠️ A new version ({minimumVersion}) of the game is available.\nPlease update to continue playing.";
            // Game stays paused
        }
        else
        {
            Debug.Log("✅ Game version is up to date.");
            updatePanel.SetActive(false);
            ResumeGame();
        }
    }

    bool IsVersionLower(string current, string required)
    {
        var currentParts = Array.ConvertAll(current.Split('.'), int.Parse);
        var requiredParts = Array.ConvertAll(required.Split('.'), int.Parse);

        for (int i = 0; i < Mathf.Max(currentParts.Length, requiredParts.Length); i++)
        {
            int c = i < currentParts.Length ? currentParts[i] : 0;
            int r = i < requiredParts.Length ? requiredParts[i] : 0;
            if (c < r) return true;
            if (c > r) return false;
        }
        return false;
    }

    void ResumeGame()
    {
        Debug.Log("▶️ Resuming game...");
        Time.timeScale = 1;
    }
}
