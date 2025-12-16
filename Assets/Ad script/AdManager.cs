using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Advertisements;
using System.Collections;

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [Header("Ad Settings")]
    [SerializeField] private string androidGameId = "5778447";
    [SerializeField] private string interstitialAdUnitId = "ca-app-pub-9159912808299098/3842535873";
    [SerializeField] private bool testMode = true;

    [Header("UI")]
    public GameObject adPanel;
    public TextMeshProUGUI adMessageText;

    [Header("Tutorial Logic")]
    [SerializeField] private int tutorialShowLimit = 2;
    private const string TutorialCountKey = "TutorialShownCount";

    private string nextScene = "";
    private bool adLoaded = false;
    private bool isLoadingAd = false;
    private float reloadTimer = 0f;
    private bool isRetry = false;

    private void Start()
    {
        Advertisement.Initialize(androidGameId, testMode, this);
    }

    private void Update()
    {
        if (!adLoaded)
        {
            reloadTimer += Time.deltaTime;
            if (reloadTimer >= 10f)
            {
                reloadTimer = 0f;
                AttemptLoadAd();
            }
        }
    }

    // -------------------- Public Buttons --------------------

    public void LoadNormalMode()
    {
        GameSettings.SetGameMode(GameSettings.GameMode.Normal);
        nextScene = GetSceneBasedOnTutorialLimit();
        TryShowAdWithChance(0.6f);
    }

    public void LoadEndlessMode()
    {
        GameSettings.SetGameMode(GameSettings.GameMode.Endless);
        nextScene = GetSceneBasedOnTutorialLimit();
        TryShowAdWithChance(0.7f);
    }

    public void RetryLevel()
    {
        isRetry = true;
        nextScene = SceneManager.GetActiveScene().name;
        TryShowAdWithChance(0.4f);
    }

    public void ExitToMenu()
    {
        nextScene = "Main Menu";
        TryShowAdWithChance(0.7f);
    }

    // -------------------- Tutorial Logic --------------------

    private string GetSceneBasedOnTutorialLimit()
    {
        int shownCount = PlayerPrefs.GetInt(TutorialCountKey, 0);

        if (shownCount < tutorialShowLimit)
        {
            PlayerPrefs.SetInt(TutorialCountKey, shownCount + 1);
            PlayerPrefs.SetString("CameFrom", "Main Menu");
            return "ControlTutorial";
        }

        return "Game";
    }

    // -------------------- Ad Logic --------------------

    private void TryShowAdWithChance(float chance)
    {
        if (Random.value < chance && adLoaded)
        {
            StartCoroutine(ShowAdWithDelay(0.4f));
        }
        else
        {
            HandleSceneLoad(nextScene);
        }
    }

    private IEnumerator ShowAdWithDelay(float delay)
    {
        if (adPanel != null)
        {
            adPanel.SetActive(true);
            adMessageText.text = "Loading Ad...";
        }

        yield return new WaitForSecondsRealtime(delay);
        Advertisement.Show(interstitialAdUnitId, this);
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adPanel != null) adPanel.SetActive(false);
        adLoaded = false;
        AttemptLoadAd();
        HandleSceneLoad(nextScene);
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"[AdManager] Ad failed: {message}");
        if (adPanel != null) adPanel.SetActive(false);
        adLoaded = false;
        AttemptLoadAd();
        HandleSceneLoad(nextScene);
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }

    public void OnInitializationComplete()
    {
        AttemptLoadAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"[AdManager] Initialization failed: {error} - {message}");
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        if (adUnitId == interstitialAdUnitId)
        {
            adLoaded = true;
            isLoadingAd = false;
        }
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning($"[AdManager] Ad load failed: {message}");
        StartCoroutine(RetryAdLoad());
    }

    private void AttemptLoadAd()
    {
        if (!isLoadingAd && !adLoaded)
        {
            isLoadingAd = true;
            Advertisement.Load(interstitialAdUnitId, this);
        }
    }

    private IEnumerator RetryAdLoad()
    {
        yield return new WaitForSecondsRealtime(2f);
        AttemptLoadAd();
    }

    // -------------------- Scene Loading Logic --------------------

    private void HandleSceneLoad(string targetScene)
    {
        if (isRetry)
        {
            isRetry = false;
            SceneManager.LoadScene(targetScene);
            return;
        }

        if (targetScene == "Main Menu")
        {
            SceneManager.LoadScene("Main Menu");
            return;
        }

        int sceneIndex = SceneUtility.GetBuildIndexByScenePath(targetScene);
        if (sceneIndex < 0)
        {
            sceneIndex = SceneManager.GetSceneByName(targetScene).buildIndex;
        }

        LoadingSceneController.LoadSceneByID(sceneIndex);
    }
}
