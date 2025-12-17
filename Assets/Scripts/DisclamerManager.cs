using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DisclaimerManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject disclaimerPanel;
    [SerializeField] private Button agreeButton;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private bool disableInputDuringTransition = true;

    private const string DisclaimerKey = "DisclaimerAgreed";
    private CanvasGroup canvasGroup;
    private bool isTransitioning = false;

    private void Awake()
    {
        canvasGroup = disclaimerPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = disclaimerPanel.AddComponent<CanvasGroup>();

        disclaimerPanel.SetActive(false);
        canvasGroup.alpha = 0;
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt(DisclaimerKey, 0) == 1)
        {
            disclaimerPanel.SetActive(false); // Already agreed
        }
        else
        {
            disclaimerPanel.SetActive(true);
            StartCoroutine(FadePanel(0f, 1f, fadeDuration));
            agreeButton.onClick.AddListener(OnAgreeClicked);
        }
    }

    private void OnApplicationQuit()
    {
        // Don't save anything if they quit without agreeing
        if (PlayerPrefs.GetInt(DisclaimerKey, 0) == 0)
        {
            Debug.Log("User exited before agreeing. Disclaimer will show again next time.");
        }
    }

    private void OnAgreeClicked()
    {
        PlayerPrefs.SetInt(DisclaimerKey, 1);
        PlayerPrefs.Save();
        agreeButton.interactable = false;
        StartCoroutine(FadePanel(1f, 0f, fadeDuration, true));
    }

    private IEnumerator FadePanel(float startAlpha, float endAlpha, float duration, bool disableAfter = false)
    {
        isTransitioning = true;

        if (disableInputDuringTransition)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        float elapsed = 0f;
        canvasGroup.alpha = startAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;

        if (disableAfter)
            disclaimerPanel.SetActive(false);

        if (disableInputDuringTransition)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        isTransitioning = false;
    }
}
