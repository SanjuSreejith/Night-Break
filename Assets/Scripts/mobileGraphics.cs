/*using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class MobileGraphicsSettings : MonoBehaviour
{
    public Volume postProcessingVolume; // Post-processing reference
    private int frameCheckInterval = 5; // FPS check interval (seconds)
    private int lastQualityLevel; // Last applied quality level
    private int fpsCheckCount = 0; // FPS stability counter

    void Start()
    {
        // Load saved quality settings
        lastQualityLevel = PlayerPrefs.GetInt("GraphicsQuality", 2);
        ApplyGraphicsSettings(lastQualityLevel);

        // Start optimization coroutines
        StartCoroutine(AutoAdjustGraphics());
        StartCoroutine(RefreshColliders());
    }

    void ApplyGraphicsSettings(int index)
    {
        lastQualityLevel = index;
        QualitySettings.SetQualityLevel(index, true); // Apply immediately
        OptimizeGraphics(index);
    }

    void OptimizeGraphics(int qualityLevel)
    {
        switch (qualityLevel)
        {
            case 0: // Low
                SetPerformanceSettings(30, 0, 0);
                SetGraphicsDetails(3, 2, ShadowQuality.Disable, false);
                AdjustMaterialQuality(3);
                break;
            case 1: // Medium Low
                SetPerformanceSettings(45, 1, 1);
                SetGraphicsDetails(2, 1, ShadowQuality.HardOnly, false);
                AdjustMaterialQuality(2);
                break;
            case 2: // Medium
                SetPerformanceSettings(60, 2, 1);
                SetGraphicsDetails(1, 1, ShadowQuality.All, true);
                AdjustMaterialQuality(1);
                break;
            case 3: // High
                SetPerformanceSettings(75, 2, 2);
                SetGraphicsDetails(0, 0, ShadowQuality.All, true);
                AdjustMaterialQuality(0);
                break;
            case 4: // Very High
                SetPerformanceSettings(120, 3, 4);
                SetGraphicsDetails(0, 0, ShadowQuality.All, true);
                AdjustMaterialQuality(0);
                break;
        }
        Debug.Log("Graphics settings updated: Quality Level " + qualityLevel);
    }

    void SetPerformanceSettings(int targetFPS, int shadowCascade, int antiAliasing)
    {
        Application.targetFrameRate = targetFPS;
        QualitySettings.shadowCascades = shadowCascade;
        QualitySettings.antiAliasing = antiAliasing;
    }

    void SetGraphicsDetails(int mipmapLimit, int textureQuality, ShadowQuality shadows, bool enablePostProcessing)
    {
        QualitySettings.globalTextureMipmapLimit = mipmapLimit;
        QualitySettings.globalTextureMipmapLimit = textureQuality;
        QualitySettings.shadows = shadows;

        if (postProcessingVolume != null)
        {
            postProcessingVolume.enabled = enablePostProcessing;
            Debug.Log("Post-processing set to: " + enablePostProcessing);
        }
    }

    void AdjustMaterialQuality(int qualityLevel)
    {
        foreach (var renderer in FindObjectsOfType<Renderer>())
        {
            Material mat = renderer.sharedMaterial;
            if (mat == null) continue;

            if (mat.HasProperty("_HeightMap"))
            {
                if (qualityLevel >= 2) mat.SetTexture("_HeightMap", null);
            }
            if (mat.HasProperty("_MaterialMap"))
            {
                if (qualityLevel >= 3) mat.SetTexture("_MaterialMap", null);
            }
            if (mat.HasProperty("_MainTex"))
            {
                Texture mainTexture = mat.GetTexture("_MainTex");
                if (mainTexture != null)
                {
                    if (qualityLevel == 0) mainTexture.filterMode = FilterMode.Point;
                    else if (qualityLevel == 1) mainTexture.filterMode = FilterMode.Bilinear;
                    else mainTexture.filterMode = FilterMode.Trilinear;
                }
            }
        }
        Debug.Log("Material quality adjusted for quality level " + qualityLevel);
    }

    IEnumerator AutoAdjustGraphics()
    {
        while (true)
        {
            yield return new WaitForSeconds(frameCheckInterval);
            float fps = 1.0f / Time.unscaledDeltaTime;
            Debug.Log("Current FPS: " + fps);

            if (fps < 15 && lastQualityLevel > 0)
            {
                fpsCheckCount++;
                if (fpsCheckCount >= 3)
                {
                    Debug.LogWarning("FPS too low! Lowering graphics settings...");
                    ApplyGraphicsSettings(lastQualityLevel - 1);
                    fpsCheckCount = 0;
                }
            }
            else if (fps > 50 && lastQualityLevel < 4)
            {
                fpsCheckCount++;
                if (fpsCheckCount >= 3)
                {
                    Debug.Log("FPS stable! Increasing graphics settings...");
                    ApplyGraphicsSettings(lastQualityLevel + 1);
                    fpsCheckCount = 0;
                }
            }
            else
            {
                fpsCheckCount = 0;
            }
        }
    }

    IEnumerator RefreshColliders()
    {
        yield return new WaitForSeconds(0.1f);
        Collider[] colliders = FindObjectsOfType<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
            yield return null;
            col.enabled = true;
        }
        Debug.Log("Colliders refreshed.");
    }
}
*/ 