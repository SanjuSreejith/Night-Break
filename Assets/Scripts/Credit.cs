using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CreditPanelSimple : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject creditPanel;
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;

    [Header("Scroll References (Optional)")]
    [SerializeField] private ScrollRect scrollRect; // Drag your ScrollRect here
    [SerializeField] private float scrollSpeed = 0.1f; // Adjust scroll speed

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private bool disableInputDuringTransition = true;

    private CanvasGroup canvasGroup;
    private bool isTransitioning = false;
    private Coroutine scrollCoroutine;

    private void Awake()
    {
        // Initialize canvas group
        canvasGroup = creditPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = creditPanel.AddComponent<CanvasGroup>();

        // Start hidden and non-interactable
        creditPanel.SetActive(false);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        openButton.onClick.AddListener(OpenPanel);
        closeButton.onClick.AddListener(ClosePanel);
    }

    private void OnDisable()
    {
        openButton.onClick.RemoveListener(OpenPanel);
        closeButton.onClick.RemoveListener(ClosePanel);
    }

    public void OpenPanel()
    {
        if (isTransitioning || creditPanel.activeSelf) return;
        StartCoroutine(DelayedOpen());
    }

    private IEnumerator DelayedOpen()
    {
        // Activate panel first
        creditPanel.SetActive(true);

        // Wait for UI layout to update (important for ScrollRect)
        yield return new WaitForEndOfFrame();

        // Reset scroll to top
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;

        // Start fade-in
        yield return StartCoroutine(FadePanel(0f, 1f, fadeDuration));

        // Enable interaction after fade
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Start auto-scrolling if scrollRect assigned
        if (scrollRect != null)
            scrollCoroutine = StartCoroutine(AutoScrollCredits());
    }

    public void ClosePanel()
    {
        if (isTransitioning || !creditPanel.activeSelf) return;

        // Disable interaction immediately to prevent input during fade out
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Stop auto scroll if running
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }

        StartCoroutine(FadeOutAndDisable());
    }

    private IEnumerator FadeOutAndDisable()
    {
        yield return StartCoroutine(FadePanel(1f, 0f, fadeDuration));
        creditPanel.SetActive(false);
    }

    private IEnumerator FadePanel(float startAlpha, float endAlpha, float duration)
    {
        isTransitioning = true;
        float elapsed = 0f;
        canvasGroup.alpha = startAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        isTransitioning = false;
    }

    private IEnumerator AutoScrollCredits()
    {
        while (scrollRect != null && scrollRect.verticalNormalizedPosition > 0f)
        {
            // Scroll down over time
            scrollRect.verticalNormalizedPosition -= Time.unscaledDeltaTime * scrollSpeed;

            // Clamp to zero to prevent overshooting
            if (scrollRect.verticalNormalizedPosition < 0f)
                scrollRect.verticalNormalizedPosition = 0f;

            yield return null;
        }
    }

    public void TogglePanel()
    {
        if (isTransitioning) return;

        if (creditPanel.activeSelf && canvasGroup.alpha > 0.9f)
            ClosePanel();
        else
            OpenPanel();
    }
}
