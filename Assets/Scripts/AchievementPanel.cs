using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

public class AchievementManager : MonoBehaviour
{
    [Header("Achievements Data")]
    public List<Achievement> achievements;

    [Header("Popup UI")]
    public GameObject popupPanel;
    public TextMeshProUGUI popupTitle;
    public TextMeshProUGUI popupDescription;
    public float popupDuration = 4f;

    [Header("Achievements Panel")]
    public GameObject achievementsPanel;
    public TextMeshProUGUI achievementsSummaryText;
    public GameObject noAchievementsText;
    public TextMeshProUGUI toggleAchievementsButtonText;

    private bool showingPopup = false;
    private FirebaseFirestore firestore;
    private bool hasLoadedFromCloud = false;

    void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;

        // Load local progress
        foreach (var a in achievements)
        {
            if (PlayerPrefs.GetInt("achievement_" + a.id, 0) == 1)
            {
                a.unlocked = true;
            }
        }

        // Process any queued achievements from previous session/scene
        foreach (string id in AchievementQueue.GetAndClearQueuedAchievements())
        {
            Debug.Log("📥 Processing queued achievement: " + id);
            UnlockAchievement(id);
        }

        // Load Firestore if signed in
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null && !hasLoadedFromCloud)
        {
            LoadAchievementsFromFirestore(user);
        }

        achievementsPanel.SetActive(false);
        UpdateAchievementsSummary();
    }

    public void UnlockAchievement(string id)
    {
        Debug.Log("🎯 Attempting to unlock achievement: " + id);

        Achievement a = achievements.Find(x => x.id == id);
        if (a == null)
        {
            Debug.LogError("❌ Achievement ID not found: " + id);
            return;
        }

        if (a.unlocked)
        {
            Debug.Log("✅ Already unlocked: " + id);
            return;
        }

        a.unlocked = true;
        PlayerPrefs.SetInt("achievement_" + a.id, 1);
        PlayerPrefs.Save();

        ShowPopup(a.title, a.description);
        UpdateAchievementsSummary();

        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
        {
            SaveAchievementToFirestore(user, a.id);
        }
    }

    public void ShowPopup(string title, string description)
    {
        if (showingPopup) return;

        showingPopup = true;
        popupTitle.text = title;
        popupDescription.text = description;
        popupPanel.SetActive(true);
        StartCoroutine(HidePopup());
    }

    IEnumerator HidePopup()
    {
        yield return new WaitForSeconds(popupDuration);
        popupPanel.SetActive(false);
        showingPopup = false;
    }

    public void ToggleAchievementsPanel()
    {
        if (achievementsPanel.activeSelf)
        {
            achievementsPanel.SetActive(false);
            toggleAchievementsButtonText.text = "Show Achievements";
        }
        else
        {
            achievementsPanel.SetActive(true);
            toggleAchievementsButtonText.text = "Hide Achievements";
            UpdateAchievementsSummary();
        }
    }

    private void UpdateAchievementsSummary()
    {
        string summary = "";
        int unlockedCount = 0;

        foreach (var a in achievements)
        {
            if (a.unlocked)
            {
                summary += "🏆 " + a.title + "\n";
                unlockedCount++;
            }
        }

        achievementsSummaryText.text = summary;
        achievementsSummaryText.gameObject.SetActive(unlockedCount > 0);
        noAchievementsText.SetActive(unlockedCount == 0);
    }

    private void LoadAchievementsFromFirestore(FirebaseUser user)
    {
        if (hasLoadedFromCloud) return;

        DocumentReference docRef = firestore.Collection("users").Document(user.UserId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                var data = task.Result.ToDictionary();
                if (data.TryGetValue("achievements", out object obj) && obj is Dictionary<string, object> unlocked)
                {
                    foreach (var a in achievements)
                    {
                        if (unlocked.ContainsKey(a.id) && (bool)unlocked[a.id])
                        {
                            if (!a.unlocked)
                            {
                                Debug.Log("☁️ Synced from Firestore: " + a.id);
                                a.unlocked = true;
                                PlayerPrefs.SetInt("achievement_" + a.id, 1);
                            }
                        }
                    }
                    PlayerPrefs.Save();
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Firestore load failed or no document found.");
            }

            hasLoadedFromCloud = true;
            UpdateAchievementsSummary();
        });
    }

    private void SaveAchievementToFirestore(FirebaseUser user, string achievementId)
    {
        if (string.IsNullOrEmpty(achievementId)) return;

        DocumentReference docRef = firestore.Collection("users").Document(user.UserId);

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { $"achievements.{achievementId}", true },
            { "lastUpdated", Timestamp.GetCurrentTimestamp() }
        };

        docRef.SetAsync(updates, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
                Debug.LogError("❌ Firestore save failed: " + task.Exception);
            else
                Debug.Log("✅ Saved to Firestore: " + achievementId);
        });
    }

    [ContextMenu("Test Achievement Popup")]
    public void TestPopup()
    {
        ShowPopup("Test Achievement", "This is a test popup.");
    }

    [ContextMenu("Reset All Achievements")]
    public void ResetAllAchievements()
    {
        foreach (var a in achievements)
        {
            a.unlocked = false;
            PlayerPrefs.DeleteKey("achievement_" + a.id);
        }
        PlayerPrefs.Save();
        UpdateAchievementsSummary();
        Debug.Log("🔁 All achievements reset.");
    }
}
