using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuScript : MonoBehaviour
{
    [Header("Panels")]
    public GameObject exitPanel;
    public GameObject modePanel;
    public GameObject loadingScreen;
    public GameObject settingsPanel;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Tutorial Settings")]
    [Tooltip("How many times the tutorial should auto-show before skipping")]
    [SerializeField] private int tutorialShowLimit = 2;
    private const string TutorialCountKey = "TutorialShownCount";

    private CanvasGroup modeCanvasGroup;
    private CanvasGroup exitCanvasGroup;
    private CanvasGroup loadingCanvasGroup;
    private CanvasGroup settingsCanvasGroup;

    private bool isFading = false;

    private void Awake()
    {
        modeCanvasGroup = EnsureCanvasGroup(modePanel);
        exitCanvasGroup = EnsureCanvasGroup(exitPanel);
        loadingCanvasGroup = EnsureCanvasGroup(loadingScreen);
        settingsCanvasGroup = EnsureCanvasGroup(settingsPanel);

    

    }

    private CanvasGroup EnsureCanvasGroup(GameObject panel)
    {
        if (panel == null) return null;

        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        if (group == null)
            group = panel.AddComponent<CanvasGroup>();

        panel.SetActive(false);
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        return group;
    }

    // ------------------ Main Menu Buttons ------------------

    public void ButtonStart()
    {
        if (isFading || modePanel.activeSelf) return;
        StartCoroutine(FadeInPanel(modePanel, modeCanvasGroup));
    }

    public void CloseModePanel()
    {
        if (isFading || !modePanel.activeSelf) return;
        StartCoroutine(FadeOutPanel(modePanel, modeCanvasGroup));
    }

    public void ButtonQuit()
    {
        if (isFading || exitPanel.activeSelf) return;
        StartCoroutine(FadeInPanel(exitPanel, exitCanvasGroup));
        Time.timeScale = 0f;
    }

    public void CancelQuit()
    {
        if (isFading || !exitPanel.activeSelf) return;
        StartCoroutine(FadeOutPanel(exitPanel, exitCanvasGroup));
        Time.timeScale = 1f;
    }

    public void ConfirmQuit()
    {
        Application.Quit();
    }

    public void ButtonSettings()
    {
        if (isFading || settingsPanel.activeSelf) return;
        StartCoroutine(FadeInPanel(settingsPanel, settingsCanvasGroup));
    }

    public void CloseSettings()
    {
        if (isFading || !settingsPanel.activeSelf) return;
        StartCoroutine(FadeOutPanel(settingsPanel, settingsCanvasGroup));
    }

    // ------------------ Game Mode Selection ------------------

    public void SelectNormalMode()
    {
        GameSettings.SetGameMode(GameSettings.GameMode.Normal);
        LoadNextSceneWithTutorialCheck();
    }

    public void SelectEndlessMode()
    {
        GameSettings.SetGameMode(GameSettings.GameMode.Endless);
        LoadNextSceneWithTutorialCheck();
    }

    private void LoadNextSceneWithTutorialCheck()
    {
        int shownCount = PlayerPrefs.GetInt(TutorialCountKey, 0);

        if (shownCount < tutorialShowLimit)
        {
            PlayerPrefs.SetInt(TutorialCountKey, shownCount + 1);
            PlayerPrefs.SetString("CameFrom", "Main Menu");
            StartCoroutine(LoadSceneWithFade("ControlTutorial"));
        }
        else
        {
            PlayerPrefs.SetString("CameFrom", "Main Menu");
            StartCoroutine(LoadSceneWithFade("Game"));
        }
    }

    // ------------------ Fade Handlers ------------------

    private IEnumerator FadeInPanel(GameObject panel, CanvasGroup group)
    {
        if (panel == null || group == null) yield break;

        panel.SetActive(true);
        isFading = true;

        float elapsed = 0f;
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
        isFading = false;
    }

    private IEnumerator FadeOutPanel(GameObject panel, CanvasGroup group)
    {
        if (panel == null || group == null) yield break;

        isFading = true;

        float elapsed = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        group.alpha = 0f;
        panel.SetActive(false);
        isFading = false;
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        yield return StartCoroutine(FadeInPanel(loadingScreen, loadingCanvasGroup));
        yield return new WaitForSeconds(1f); // Optional delay
        SceneManager.LoadScene(sceneName);
    }
}
