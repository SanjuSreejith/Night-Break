using UnityEngine;

public class DynamicLightingManager : MonoBehaviour
{
    [Header("Shadow Settings")]
    public int ultraQualityShadowResolution = 8192; // Ultra quality shadow map resolution
    public int highQualityShadowResolution = 4096;  // High quality shadow map resolution
    public int mediumQualityShadowResolution = 2048; // Medium quality shadow map resolution
    public int lowQualityShadowResolution = 1024;   // Low quality shadow map resolution
    public int veryLowQualityShadowResolution = 512; // Very low quality shadow map resolution

    [Header("Light Settings")]
    public Light[] lights; // Add all the lights in your scene here
    public bool enableSoftShadows = true;

    void Start()
    {
        AdjustLightingBasedOnQuality();
    }

    private void AdjustLightingBasedOnQuality()
    {
        // Get the current quality level
        int qualityLevel = QualitySettings.GetQualityLevel();

        // Adjust settings based on quality level
        switch (qualityLevel)
        {
            case 0: // Very Low quality
                SetShadowResolution(veryLowQualityShadowResolution);
                ConfigureLights(1, false); // Limit to 1 shadow-casting light, no soft shadows
                DisableGlobalIllumination();
                break;

            case 1: // Low quality
                SetShadowResolution(lowQualityShadowResolution);
                ConfigureLights(2, false); // Limit to 2 shadow-casting lights, no soft shadows
                DisableGlobalIllumination();
                break;

            case 2: // Medium quality
                SetShadowResolution(mediumQualityShadowResolution);
                ConfigureLights(3, enableSoftShadows); // Limit to 3 shadow-casting lights, soft shadows if enabled
                EnableGlobalIllumination();
                break;

            case 3: // High quality
                SetShadowResolution(highQualityShadowResolution);
                ConfigureLights(4, enableSoftShadows); // Up to 4 shadow-casting lights, soft shadows if enabled
                EnableGlobalIllumination();
                break;

            case 4: // Ultra quality
                SetShadowResolution(ultraQualityShadowResolution);
                ConfigureLights(6, enableSoftShadows); // Up to 6 shadow-casting lights, soft shadows enabled
                EnableGlobalIllumination();
                break;

            default:
                Debug.LogWarning("Unknown quality level: " + qualityLevel);
                break;
        }
    }

    private void SetShadowResolution(int resolution)
    {
        QualitySettings.shadowResolution = (ShadowResolution)Mathf.Log(resolution / 512, 2);
        Debug.Log("Shadow resolution set to: " + resolution);
    }

    private void ConfigureLights(int maxShadowCastingLights, bool enableSoftShadows)
    {
        int shadowCastingLightCount = 0;

        foreach (var light in lights)
        {
            if (shadowCastingLightCount < maxShadowCastingLights && light.type != LightType.Point)
            {
                light.shadows = enableSoftShadows ? LightShadows.Soft : LightShadows.Hard;
                shadowCastingLightCount++;
            }
            else
            {
                light.shadows = LightShadows.None; // Disable shadows for excess lights
            }
        }

        Debug.Log("Max shadow-casting lights set to: " + maxShadowCastingLights);
    }

    private void DisableGlobalIllumination()
    {
        DynamicGI.updateThreshold = 0.1f; // Minimize updates to the GI system
        Debug.Log("Real-time Global Illumination disabled.");
    }

    private void EnableGlobalIllumination()
    {
        DynamicGI.updateThreshold = 1.0f; // Allow regular updates to the GI system
        Debug.Log("Real-time Global Illumination enabled.");
    }
}
