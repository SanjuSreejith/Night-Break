using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class IntroVideoManager : MonoBehaviour
{
    [Header("Video & UI")]
    public VideoPlayer videoPlayer;
    public Image fadeImage;
    public TextMeshProUGUI skipText;
    public GameObject loginPanel;
    public RawImage BGImage;
    [Header("Settings")]
    public float showSkipDelay = 3f;
    public float fadeDuration = 1.5f;

    private bool isTransitioning = false;

    void Start()
    {
        fadeImage.color = new Color(0, 0, 0, 0);
        skipText.gameObject.SetActive(false);
        loginPanel.SetActive(false); // Hide login at start

        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Play();

        Invoke(nameof(ShowSkipText), showSkipDelay);
        Invoke(nameof(HideSkipText), 30f); // Hide "Touch to Skip" after 30 seconds
    }

    void Update()
    {
        if (!isTransitioning && (Input.GetMouseButtonDown(0) || Input.touchCount > 0 || Input.GetKeyDown(KeyCode.Space)))
        {
            StartCoroutine(SkipVideo());
        }
    }

    void ShowSkipText()
    {
        skipText.gameObject.SetActive(true);
    }

    void HideSkipText()
    {
        skipText.gameObject.SetActive(false);
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (!isTransitioning)
        {
            StartCoroutine(FadeAndProceed());
        }
    }

    IEnumerator SkipVideo()
    {
        isTransitioning = true;
        videoPlayer.Stop();
        yield return StartCoroutine(FadeOut());
        CheckFirebaseAndProceed();
    }

    IEnumerator FadeAndProceed()
    {
        isTransitioning = true;
        yield return StartCoroutine(FadeOut());
        CheckFirebaseAndProceed();
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    void CheckFirebaseAndProceed()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;

        if (user != null)
        {
            Debug.Log("User found. Verifying token...");
            user.TokenAsync(true).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogWarning("Token check failed. Showing login panel.");
                    ShowLoginPanel();
                }
                else
                {
                    Debug.Log("Firebase login valid. Loading menu...");
                    SceneManager.LoadScene("Main Menu"); // Replace with your actual scene name
                }
            });
        }
        else
        {
            Debug.Log("No Firebase user found. Showing login panel.");
            ShowLoginPanel();
        }
    }

    void ShowLoginPanel()
    {
        BGImage.gameObject.SetActive(true);
        videoPlayer.gameObject.SetActive(false);
        skipText.gameObject.SetActive(false);
        loginPanel.SetActive(true);
    }
}