using System.Collections.Generic;
using TMPro;  
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
public class GraphicsSettings : MonoBehaviour
{
    [Header("Quality Buttons")]
    public Button lowButton, mediumButton, highButton, maxButton;

    [Header("FPS Buttons")]
    public Button fps30Button, fps45Button, fps60Button;

    [Header("Show FPS Toggle Buttons")]
    public Button showFPSOnButton, showFPSOffButton;

    [Header("In-game FPS Counter")]
    public GameObject fpsDisplayObject;

    private IAdaptivePerformance ap;
    private float renderScale = 1f;
    [Header("Warning Message")]
    public CanvasGroup warningCanvasGroup;
    public TextMeshProUGUI warningText; // or use 'public Text warningText;' if not TMP


    void Awake()
    {
        SetupWarningUI();
        ApplySavedSettings();
       
    }

    void Start()
    {
        ap = Holder.Instance;

        if (!PlayerPrefs.HasKey("GraphicsQuality"))
        {
            AutoDetectGraphics();
        }

        UpdateFPSButtonStates();
        UpdateShowFPSButtons();

        if (ap != null)
        {
            ap.ThermalStatus.ThermalEvent += OnThermalEvent;
        }
    }

    // ============ Graphics Quality ============

    public void SetQualityLow() => ApplyQuality(0);
    public void SetQualityMedium() => ApplyQuality(1);
    public void SetQualityHigh() => ApplyQuality(2);
    public void SetQualityMax() => ApplyQuality(3);

    bool IsGameScene() => SceneManager.GetActiveScene().name == "Game";

    void ApplyQuality(int index)
    {
        int maxAvailable = QualitySettings.names.Length - 1;
        int savedMax = PlayerPrefs.GetInt("MaxSupportedQuality", maxAvailable);

        // ✅ Only clamp on mobile
        if (Application.isMobilePlatform)
        {
            if (Application.isMobilePlatform && index > savedMax)
            {
                Debug.LogWarning("Your device does not support this graphics level.");
                StartCoroutine(ShowWarningMessage("Your device does not support this graphics level."));
                return;
            }


            index = Mathf.Clamp(index, 0, savedMax);
        }

        QualitySettings.SetQualityLevel(index, false);
        PlayerPrefs.SetInt("GraphicsQuality", index);
        PlayerPrefs.Save();

        ApplyQualityFeatures(index);
        UpdateQualityButtonStates(index);
    }

    void UpdateQualityButtonStates(int selectedIndex)
    {
        if (lowButton) lowButton.interactable = selectedIndex != 0;
        if (mediumButton) mediumButton.interactable = selectedIndex != 1;
        if (highButton) highButton.interactable = selectedIndex != 2;
        if (maxButton) maxButton.interactable = selectedIndex != 3;
    }
    IEnumerator ShowWarningMessage(string message, float duration = 2f)
    {
        if (!warningText || !warningCanvasGroup) yield break;

        warningText.text = message;
        warningCanvasGroup.alpha = 1f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Fade out
        float fadeOut = 1f;
        while (fadeOut > 0)
        {
            fadeOut -= Time.deltaTime * 2f;
            warningCanvasGroup.alpha = fadeOut;
            yield return null;
        }

        warningText.text = "";
        warningCanvasGroup.alpha = 0;
    }

    void ApplyQualityFeatures(int index)
    {
        bool fpsManuallySet = PlayerPrefs.GetInt("FPSManuallySet", 0) == 1;

        switch (index)
        {
            case 0:
                if (!fpsManuallySet) Application.targetFrameRate = 30;
                SetPerformanceValues(2, 0, 10f, 0);
                if (IsGameScene()) AdjustLOD(0.5f, 40f);
                if (IsGameScene()) DisableShadows();
                if (IsGameScene()) DisablePostFX();
                renderScale = 0.65f;
                break;

            case 1:
                if (!fpsManuallySet) Application.targetFrameRate = 45;
                SetPerformanceValues(1, 0, 20f, 0);
                if (IsGameScene()) AdjustLOD(0.75f, 40f);
                if (IsGameScene()) DisableShadows();
                renderScale = 0.75f;
                break;

            case 2:
                if (!fpsManuallySet) Application.targetFrameRate = 60;
                SetPerformanceValues(1, 2, 30f, 2);
                if (IsGameScene()) AdjustLOD(1.0f, 65f);
                if (IsGameScene()) EnableShadows();
                renderScale = 0.95f;
                break;

            case 3:
                if (!fpsManuallySet) Application.targetFrameRate = 60;
                SetPerformanceValues(0, 4, 75f, 4);
                if (IsGameScene()) AdjustLOD(1.5f, 85f);
                if (IsGameScene()) EnableShadows();
                renderScale = 1.0f;
                break;
        }

        RenderSettings.fog = IsGameScene();

        ScalableBufferManager.ResizeBuffers(renderScale, renderScale);
    }

    void SetPerformanceValues(int textureLimit, int shadowCascades, float shadowDist, int aa)
    {
        QualitySettings.globalTextureMipmapLimit = textureLimit;
        QualitySettings.shadowCascades = shadowCascades;
        QualitySettings.shadowDistance = shadowDist;
        QualitySettings.antiAliasing = aa;
    }
    void SetupWarningUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (!canvas)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        GameObject warningGO = new GameObject("WarningText");
        warningGO.transform.SetParent(canvas.transform);

        RectTransform rect = warningGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.9f);
        rect.anchorMax = new Vector2(0.5f, 0.9f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(800, 100);

        warningText = warningGO.AddComponent<TextMeshProUGUI>();
        warningText.alignment = TextAlignmentOptions.Center;
        warningText.fontSize = 32;
        warningText.color = new Color(1f, 0.3f, 0.3f, 1f); // reddish
        warningText.text = "";

        warningCanvasGroup = warningGO.AddComponent<CanvasGroup>();
        warningCanvasGroup.alpha = 0f;
    }

    void EnableShadows()
    {
        QualitySettings.shadows = ShadowQuality.All;
        foreach (var light in FindObjectsOfType<Light>())
        {
            light.shadows = LightShadows.Soft;
        }
    }

    void DisableShadows()
    {
        QualitySettings.shadows = ShadowQuality.Disable;
        foreach (var light in FindObjectsOfType<Light>())
        {
            light.shadows = LightShadows.None;
        }
    }

    void DisablePostFX()
    {
        var volumes = FindObjectsOfType<UnityEngine.Rendering.Volume>();
        foreach (var v in volumes)
        {
            v.enabled = false;
        }
    }

    void AdjustLOD(float bias, float clip)
    {
        QualitySettings.lodBias = bias;
        if (Camera.main)
            Camera.main.farClipPlane = clip;
    }

    // ============ FPS Buttons ============

    public void SetFPS30() => SetFPS(30);
    public void SetFPS45() => SetFPS(45);
    public void SetFPS60() => SetFPS(60);

    void SetFPS(int fps)
    {
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt("TargetFPS", fps);
        PlayerPrefs.SetInt("FPSManuallySet", 1);
        PlayerPrefs.Save();

        UpdateFPSButtonStates();
    }

    void UpdateFPSButtonStates()
    {
        int currentFPS = PlayerPrefs.GetInt("TargetFPS", 45);

        if (fps30Button) fps30Button.interactable = currentFPS != 30;
        if (fps45Button) fps45Button.interactable = currentFPS != 45;
        if (fps60Button) fps60Button.interactable = currentFPS != 60;
    }

    // ============ Show FPS Toggle ============

    public void ShowFPSOn()
    {
        PlayerPrefs.SetInt("ShowFPS", 1);
        PlayerPrefs.Save();
        UpdateShowFPSButtons();
    }

    public void ShowFPSOff()
    {
        PlayerPrefs.SetInt("ShowFPS", 0);
        PlayerPrefs.Save();
        UpdateShowFPSButtons();
    }

    void UpdateShowFPSButtons()
    {
        bool show = PlayerPrefs.GetInt("ShowFPS", 0) == 1;

        if (fpsDisplayObject) fpsDisplayObject.SetActive(show);

        if (showFPSOnButton) showFPSOnButton.interactable = !show;
        if (showFPSOffButton) showFPSOffButton.interactable = show;
    }

    // ============ Auto-Detect Quality ============

    void AutoDetectGraphics()
    {
        string cpu = SystemInfo.processorType.ToLower();
        int ram = SystemInfo.systemMemorySize; // in MB
        int graphicsIndex = 2; // Default to medium

        bool isHighEndCPU = cpu.Contains("snapdragon 865") ||
                            cpu.Contains("snapdragon 870") ||
                            cpu.Contains("snapdragon 888") ||
                            cpu.Contains("snapdragon 8") || // Covers 8 Gen 1, 8+ Gen 1
                            cpu.Contains("dimensity 9000") ||
                            cpu.Contains("dimensity 9200") ||
                            cpu.Contains("exynos 2200") ||
                            cpu.Contains("apple a14") ||
                            cpu.Contains("apple a15") ||
                            cpu.Contains("apple a16");

        if (ram < 3000)
            graphicsIndex = 0; // Low
        else if (ram < 6000)
            graphicsIndex = 1; // Medium
        else if (ram < 8000)
            graphicsIndex = 2; // High
        else
            graphicsIndex = 3; // Max

        // ✅ CPU override
        if (isHighEndCPU)
            graphicsIndex = 3;

        graphicsIndex = Mathf.Clamp(graphicsIndex, 0, QualitySettings.names.Length - 1);

        PlayerPrefs.SetInt("GraphicsQuality", graphicsIndex);

        if (Application.isMobilePlatform)
            PlayerPrefs.SetInt("MaxSupportedQuality", graphicsIndex);
        else
            PlayerPrefs.SetInt("MaxSupportedQuality", QualitySettings.names.Length - 1);

        PlayerPrefs.Save();

        Debug.Log($"Auto-Detect → CPU: {cpu}, RAM: {ram}MB, Quality: {graphicsIndex}, High-End CPU: {isHighEndCPU}");
        ApplyQuality(graphicsIndex);
    }

    // ============ Thermal Warning Handling ============

    void OnThermalEvent(ThermalMetrics t)
    {
        if (t.WarningLevel == WarningLevel.ThrottlingImminent)
        {
            int current = QualitySettings.GetQualityLevel();
            if (current > 0)
            {
                Debug.Log("Thermal Warning: Reducing quality.");
                ApplyQuality(current - 1);
            }
        }
    }
    public static void ApplyLowQualityExternally()
    {
        QualitySettings.SetQualityLevel(0, false); // Set level
        PlayerPrefs.SetInt("GraphicsQuality", 0);
        PlayerPrefs.Save();

        var instance = FindObjectOfType<GraphicsSettings>();
        if (instance != null)
        {
            instance.ApplyQualityFeatures(0);
            instance.UpdateQualityButtonStates(0);
        }
        else
        {
            Debug.LogWarning("GraphicsSettings instance not found in scene. Cannot apply features.");
        }
    }
    // ============ Apply Saved on Launch ============

    void ApplySavedSettings()
    {
        int quality = PlayerPrefs.GetInt("GraphicsQuality", 2);
        int fps = PlayerPrefs.GetInt("TargetFPS", 45);

        QualitySettings.SetQualityLevel(quality, false);
        Application.targetFrameRate = fps;

        ApplyQualityFeatures(quality);
        UpdateQualityButtonStates(quality);
        UpdateFPSButtonStates();
        UpdateShowFPSButtons();
    }
}
