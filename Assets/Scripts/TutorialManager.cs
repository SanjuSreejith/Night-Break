using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel;
    public Button skipButton;
    public AudioSource backgroundMusic;
    public TMP_Text tutorialText;
    public Image backgroundBlockerImage;

    private CanvasGroup blockerCanvasGroup;
    private bool tutorialActive = false;

    void Start()
    {
        SetupCanvasGroup();
        ShowTutorial();
    }

    void SetupCanvasGroup()
    {
        if (backgroundBlockerImage != null)
        {
            blockerCanvasGroup = backgroundBlockerImage.GetComponent<CanvasGroup>();
            if (blockerCanvasGroup == null)
            {
                blockerCanvasGroup = backgroundBlockerImage.gameObject.AddComponent<CanvasGroup>();
            }

            blockerCanvasGroup.alpha = 1f;
            blockerCanvasGroup.interactable = false;
            blockerCanvasGroup.blocksRaycasts = true;
            backgroundBlockerImage.gameObject.SetActive(true);
        }
    }

    void ShowTutorial()
    {
        tutorialActive = true;
        tutorialPanel.SetActive(true);
        Time.timeScale = 0f;

        if (GameSettings.CurrentMode == GameSettings.GameMode.Normal)
        {
            tutorialText.text =
                "Open the Door\n\n" +
                "Go and pick up the torch on the table.\n\n" +
                "There are many doors, but only one is the escape door. " +
                "Find it and escape before the ghost catches you!";
        }
        else if (GameSettings.CurrentMode == GameSettings.GameMode.Endless)
        {
            tutorialText.text =
                "Survive As Long As You Can!\n\n" +
                "Pick up the torch on the table to begin.\n\n" +
                "Explore the area and avoid the ghost. " +
                "There is no escape — just survive as long as possible!";
        }

        if (backgroundMusic != null)
        {
            StartCoroutine(FadeAudio(backgroundMusic, 0.2f, 1f));
        }

        skipButton.onClick.AddListener(SkipTutorial);
    }

    public void SkipTutorial()
    {
        tutorialActive = false;
        tutorialPanel.SetActive(false);
        Time.timeScale = 1f;

        if (backgroundMusic != null)
        {
            StartCoroutine(FadeAudio(backgroundMusic, 1f, 1f));
        }

        if (blockerCanvasGroup != null)
        {
            StartCoroutine(FadeCanvasGroupOut(blockerCanvasGroup, 0.5f));
        }
    }

    public void TorchPickedUp()
    {
        if (tutorialActive)
        {
            SkipTutorial();
        }
    }

    private IEnumerator FadeAudio(AudioSource audioSource, float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    private IEnumerator FadeCanvasGroupOut(CanvasGroup canvasGroup, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(false);
    }
}
