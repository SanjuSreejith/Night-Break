using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SubtitleDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private float typingSpeed = 0.05f; // Adjust to make it faster/slower

    private Coroutine currentRoutine;

    private void Start()
    {
        subtitleText.text = ""; // Clear on start
    }

    public void Show(string subtitle, float duration)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        gameObject.SetActive(true);
        currentRoutine = StartCoroutine(TypeText(subtitle, duration));
    }

    public void Hide()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        subtitleText.text = "";
        gameObject.SetActive(false);
    }

    private IEnumerator TypeText(string fullText, float duration)
    {
        subtitleText.text = "";

        foreach (char c in fullText)
        {
            subtitleText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(duration);
        subtitleText.text = "";
        gameObject.SetActive(false);
    }
}
