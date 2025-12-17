using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SmoothTransition : MonoBehaviour
{
    public Image fadePanel;  // The panel used for the fade effect
    public float fadeDuration = 1f;  // Duration for the fade effect

    void Start()
    {
        // Initially make sure the fade panel is fully opaque (invisible)
        fadePanel.gameObject.SetActive(true);
        fadePanel.color = new Color(0, 0, 0, 1);  // Set to fully opaque black
        StartCoroutine(FadeIn());
    }

    // Function to fade in the panel (make it invisible)
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            fadePanel.color = new Color(0, 0, 0, Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fadePanel.color = new Color(0, 0, 0, 0);  // Ensure it's fully transparent
        fadePanel.gameObject.SetActive(false);  // Hide the panel after fade-out
    }

    // Function to fade out the panel (make it visible)
    private IEnumerator FadeOut()
    {
        fadePanel.gameObject.SetActive(true);
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            fadePanel.color = new Color(0, 0, 0, Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        fadePanel.color = new Color(0, 0, 0, 1);  // Ensure it's fully opaque
    }

    // Function to load the scene with fade-out and fade-in effect
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // First, fade out
        yield return StartCoroutine(FadeOut());

        // Load the new scene
        SceneManager.LoadScene(sceneName);

        // Wait until the scene is loaded
        yield return null;

        // Then fade in after the scene has loaded
        yield return StartCoroutine(FadeIn());
    }
}
