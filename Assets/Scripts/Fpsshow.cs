using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText; // Reference to the UI Text for displaying FPS
    private float deltaTime = 0.0f;

    void Start()
    {
        // Load setting from PlayerPrefs
        bool showFPS = PlayerPrefs.GetInt("ShowFPS", 0) == 1;
        if (fpsText) fpsText.gameObject.SetActive(showFPS);
    }

    void Update()
    {
        if (fpsText && fpsText.gameObject.activeSelf)
        {
            // Calculate FPS
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            fpsText.text = "FPS: " + Mathf.Ceil(fps).ToString();
        }
    }

    public void ToggleFPS(bool isOn)
    {
        // Save the setting
        PlayerPrefs.SetInt("ShowFPS", isOn ? 1 : 0);
        PlayerPrefs.Save();

        // Apply the setting
        if (fpsText) fpsText.gameObject.SetActive(isOn);
    }
}
