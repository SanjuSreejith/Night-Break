using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class TransitionManager : MonoBehaviour {
    public static TransitionManager Instance;

    [Header("Fade Settings")]
    public Image fadeImage; // A full-screen black UI Image (its alpha should start at 0)
    public float fadeOutDuration = 1.5f;
    public float fadeInDuration = 1.5f;
    public float waitAfterFadeOut = 1f;
    public float waitAfterFadeIn = 1f;

    [Header("Audio")]
    public AudioSource heartbeatAudio;
    public AudioSource ghostLaughAudio;  // Ghost landing/laugh sound
    public AudioSource breathingAudio;
    public AudioSource recoveryAudio;
    public AudioSource finalGameOverAudio;

    [Header("Camera Effects")]
    public Camera playerCamera;
    public float lowFOV = 30f;
    public float headacheDuration = 3f;
    public AnimationCurve fovCurve;
    public AnimationCurve cameraShakeCurve;
    public float shakeMagnitude = 0.5f;

    [Header("Camera Reset")]
    public Transform cameraResetTransform; // A dedicated GameObject that holds the reset position

    [Header("Player Settings")]
    public Transform wakingUpRoom; // Teleport destination (the "waking up" room)
    public Vector3 newOrientation; // New orientation (Euler angles) for the player after teleport
    public StarterAssets.FirstPersonController playerController;

    [Header("UI")]
    public TMP_Text warningText;

    // State flag: Has the player already been captured once?
    public bool hasWokenUp = false;

    // Reference to the EscapeGameController (to reduce time left)
    public EscapeGameController gameController;

    // Reference to the GhostAI (to upgrade difficulty)
    public GhostAI ghostAI;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    // Called by GhostAI (or by EscapeGameController when time runs out)
    public void OnPlayerCaught() {
        if (!hasWokenUp) {
            // First capture: run the transition sequence.
            StartCoroutine(TransitionSequence());
        } else {
            // Second capture (or if time is over): run the final game-over sequence.
            StartCoroutine(FinalGameOverSequence());
        }
    }

    IEnumerator TransitionSequence() {
        // Disable player controls.
        playerController.controlsEnabled = false;

        // Fade out to black.
        yield return StartCoroutine(Fade(0, 1, fadeOutDuration));
        
        // Play heartbeat and ghost landing sounds.
        if (heartbeatAudio) heartbeatAudio.Play();
        if (ghostLaughAudio) ghostLaughAudio.Play();

        yield return new WaitForSecondsRealtime(waitAfterFadeOut);

        // Teleport the player to the waking-up room and adjust orientation.
        playerController.transform.position = wakingUpRoom.position;
        playerController.transform.rotation = Quaternion.Euler(newOrientation);

        // Reduce game time by half and show a warning.
       

        // Play a subtle breathing sound.
        if (breathingAudio) breathingAudio.Play();

        // Fade back in.
        yield return StartCoroutine(Fade(1, 0, fadeInDuration));

        yield return new WaitForSecondsRealtime(waitAfterFadeIn);

        // Run the headache (shake/wobble) camera effect.
        yield return StartCoroutine(HeadacheEffect());

        // Keep controls disabled for a recovery delay (e.g., 5 seconds).
        yield return new WaitForSecondsRealtime(5f);
        playerController.controlsEnabled = true;

        // Play a deep exhale / recovery sound.
        if (recoveryAudio) recoveryAudio.Play();

        // Hide the warning text after a brief period.
        if (warningText) {
            yield return new WaitForSecondsRealtime(2f);
            warningText.gameObject.SetActive(false);
        }

        // Upgrade ghost AI difficulty.
        if (ghostAI != null) {
            ghostAI.UpgradeDifficulty(1); // Pass an appropriate integer value
        }

        // Mark that the transition has occurred.
        hasWokenUp = true;
    }

    IEnumerator FinalGameOverSequence() {
        // Disable controls.
        playerController.controlsEnabled = false;

        // Play ghost landing sound (if not already playing) and shake the camera.
        if (ghostLaughAudio) ghostLaughAudio.Play();
        yield return StartCoroutine(HeadacheEffect());

        // Fade out more slowly and play a heavy final audio.
        if (finalGameOverAudio) finalGameOverAudio.Play();
        yield return StartCoroutine(Fade(0, 1, fadeOutDuration * 1.5f));

        yield return new WaitForSecondsRealtime(3f);

        // Transition to the main menu.
        SceneManager.LoadScene("Main Menu");
    }

    IEnumerator Fade(float startAlpha, float endAlpha, float duration) {
        float elapsed = 0;
        Color c = fadeImage.color;
        while (elapsed < duration) {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            c.a = alpha;
            fadeImage.color = c;
            yield return null;
        }
        c.a = endAlpha;
        fadeImage.color = c;
    }

    IEnumerator HeadacheEffect() {
        // Use the dedicated reset transform's position as the camera's base position.
        Vector3 originalPos = cameraResetTransform.localPosition;
        float originalFOV = playerCamera.fieldOfView;
        
        float elapsed = 0f;
        while (elapsed < headacheDuration) {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / headacheDuration;
            
            // Lerp the FOV from lowFOV back to the original FOV using the provided curve.
            float newFOV = Mathf.Lerp(lowFOV, originalFOV, fovCurve.Evaluate(t));
            playerCamera.fieldOfView = newFOV;

            // Apply a slight shake/wobble relative to the saved original position.
            float shakeAmount = shakeMagnitude * cameraShakeCurve.Evaluate(t);
            playerCamera.transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

            yield return null;
        }
        
        // Reset the camera's FOV and position.
        playerCamera.fieldOfView = originalFOV;
        playerCamera.transform.localPosition = originalPos;
    }
}
