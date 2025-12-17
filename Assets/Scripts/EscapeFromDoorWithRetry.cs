using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EscapeGameController : MonoBehaviour
{
    public Button escapeButton;
    public TMP_Text messageText;
    public TMP_Text messageTextw;
    public TMP_Text timerText;
    public TMP_Text endlessSummaryText;
    public TMP_Text highScoreText;
    public TMP_Text newHighScoreText;

    public GameObject winBanner;
    public GameObject NormalloseBanner;
    public GameObject EndlessloseBanner;
    public GameObject blackScreenImage;

    public Button retryButton;
    public Button exitButton;
    public GameObject door;
    public Transform player;
    public Transform ghost;

    public float proximityDistance = 5f;

    public float easyTime = 420f;
    public float mediumTime = 300f;
    public float hardTime = 180f;

    private float timeLeft;
    private float survivalTime;
    private bool isEscaped = false;
    private bool isGameRunning = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip timeOutDeathSound;

    void Start()
    {
        InitializeGame();
        AchievementQueue.QueueAchievement("FirstGame");
    }

    void Update()
    {
        if (!isGameRunning) return;

        if (GameSettings.CurrentMode == GameSettings.GameMode.Normal)
            UpdateTimer();
        else
            UpdateSurvivalTimer();

        CheckProximity();
    }

    private void InitializeGame()
    {
        isEscaped = false;
        isGameRunning = true;

        winBanner.SetActive(false);
        NormalloseBanner.SetActive(false);
        EndlessloseBanner.SetActive(false);
        escapeButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);

        messageText.text = "";
        messageTextw.text = "";
        timerText.text = "";

        if (GameSettings.CurrentMode == GameSettings.GameMode.Normal)
        {
            string difficulty = PlayerPrefs.GetString("GameDifficulty", "Medium");
            switch (difficulty)
            {
                case "Easy": timeLeft = easyTime; break;
                case "Medium": timeLeft = mediumTime; break;
                case "Hard": timeLeft = hardTime; break;
                default: timeLeft = mediumTime; break;
            }

            int minutes = Mathf.FloorToInt(timeLeft / 60f);
            int seconds = Mathf.FloorToInt(timeLeft % 60f);
            timerText.text = $"Time Left:\n {minutes:D2}:{seconds:D2}";
        }
        else
        {
            SessionSettings.OverrideHardInEndless = true;
            if (blackScreenImage != null)
                StartCoroutine(ShowBlackScreenThenStart(1f));

            survivalTime = 0f;
            timerText.text = "Time Survived:\n 00:00";
        }
    }

    private System.Collections.IEnumerator ShowBlackScreenThenStart(float delay)
    {
        blackScreenImage.SetActive(true);
        yield return new WaitForSeconds(delay);
        blackScreenImage.SetActive(false);
        isGameRunning = true;
    }

    private void UpdateTimer()
    {
        timeLeft -= Time.deltaTime;
        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);
        timerText.text = $"Time Left:\n {minutes:D2}:{seconds:D2}";

        if (timeLeft <= 0f)
            ShowNormalLoseScreen(false); // Timeout death
    }

    private void UpdateSurvivalTimer()
    {
        survivalTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(survivalTime / 60f);
        int seconds = Mathf.FloorToInt(survivalTime % 60f);
        timerText.text = $"Time Survived:\n {minutes:D2}:{seconds:D2}";
    }

    private void CheckProximity()
    {
        float distanceToDoor = Vector3.Distance(player.position, door.transform.position);
        SetIsNearDoor(distanceToDoor <= proximityDistance);
    }

    public void SetIsNearDoor(bool isNear)
    {
        escapeButton.gameObject.SetActive(GameSettings.CurrentMode == GameSettings.GameMode.Normal && isNear);
    }

    public void OnEscapeButtonPressed()
    {
        if (!isEscaped)
        {
            isEscaped = true;
            ShowWinScreen();
        }
    }

    private void ShowWinScreen()
    {
        isGameRunning = false;
        winBanner.SetActive(true);
        retryButton.gameObject.SetActive(true);
        exitButton.gameObject.SetActive(true);
        escapeButton.gameObject.SetActive(false);
        messageTextw.text = "You Escaped!";

        if (timeLeft >= 360) AchievementQueue.QueueAchievement("EscapeWithin1Min");
        if (timeLeft >= 180) AchievementQueue.QueueAchievement("EscapeWithin3Min");

        string difficulty = PlayerPrefs.GetString("GameDifficulty", "Medium");
        switch (difficulty)
        {
            case "Easy": AchievementQueue.QueueAchievement("EscapeEasy"); break;
            case "Medium":
                AchievementQueue.QueueAchievement("EscapeMedium");
                AchievementQueue.QueueAchievement("UnlockEndless");
                break;
            case "Hard":
                AchievementQueue.QueueAchievement("EscapeHard");
                AchievementQueue.QueueAchievement("UnlockEndless");
                break;
        }
    }

    // Replaced by ShowNormalLoseScreen()
  /*  private void ShowLoseScreen()
    {
        if (!loseBanner.activeSelf)
        {
            isGameRunning = false;
            loseBanner.SetActive(true);
            messageText.text = "You Lost!";
            retryButton.gameObject.SetActive(true);
            exitButton.gameObject.SetActive(true);
            escapeButton.gameObject.SetActive(false);
        }
    }*/

    public void ShowNormalLoseScreen(bool caughtByGhost)
    {
        if (!EndlessloseBanner.activeSelf && !NormalloseBanner.activeSelf)
        {
            if (GameSettings.CurrentMode == GameSettings.GameMode.Endless)
            {
                ShowEndlessLoseScreen(); // fallback safety
                return;
            }

            isGameRunning = false;
            Time.timeScale = 0f;

            NormalloseBanner.SetActive(true);

            // Play sound only if timeout (not ghost)
            if (!caughtByGhost && timeOutDeathSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(timeOutDeathSound);
            }

            retryButton.gameObject.SetActive(true);
            exitButton.gameObject.SetActive(true);
            escapeButton.gameObject.SetActive(false);
        }
    }

    public void ShowEndlessLoseScreen()
    {
        if (GameSettings.CurrentMode != GameSettings.GameMode.Endless) return;
        if (!EndlessloseBanner.activeSelf)
        {
            isGameRunning = false;
            Time.timeScale = 0f;

            EndlessloseBanner.SetActive(true);
          


            int minutes = Mathf.FloorToInt(survivalTime / 60f);
            int seconds = Mathf.FloorToInt(survivalTime % 60f);
            endlessSummaryText.text = $"You survived {minutes:D2}:{seconds:D2}.";

            float previousHighScore = PlayerPrefs.GetFloat("EndlessHighScore", 0f);
            if (survivalTime > previousHighScore)
            {
                PlayerPrefs.SetFloat("EndlessHighScore", survivalTime);
                PlayerPrefs.Save();
                highScoreText.text = $"High Score: {minutes:D2}:{seconds:D2}";
                if (newHighScoreText != null)
                    newHighScoreText.text = "NEW HIGH SCORE!";

                AchievementQueue.QueueAchievement("EndlessHighScore");
            }
            else
            {
                int prevMin = Mathf.FloorToInt(previousHighScore / 60f);
                int prevSec = Mathf.FloorToInt(previousHighScore % 60f);
                highScoreText.text = $"High Score:\n {prevMin:D2}:{prevSec:D2}";
                if (newHighScoreText != null)
                    newHighScoreText.text = "";
            }

            if (survivalTime >= 300f) AchievementQueue.QueueAchievement("Survive5Min");
            if (survivalTime >= 600f) AchievementQueue.QueueAchievement("Survive10Min");

            retryButton.gameObject.SetActive(true);
            exitButton.gameObject.SetActive(true);
            escapeButton.gameObject.SetActive(false);
        }
    }

    public void OnRetryButtonPressed()
    {
        SessionSettings.OverrideHardInEndless = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnExitButtonPressed()
    {
        SessionSettings.OverrideHardInEndless = false;
        SceneManager.LoadScene("Main Menu");
    }
}
