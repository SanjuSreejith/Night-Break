using UnityEngine;

public class CandleExtinguisher : MonoBehaviour
{
    public Light candleLight;              // The light affecting all candles
    public Material flameMaterial;         // The flame material (shared)
    public float extinguishTime = 5f;      // Time in seconds before extinguishing

    private void Start()
    {
        Invoke(nameof(ExtinguishCandle), extinguishTime);
    }

    private void ExtinguishCandle()
    {
        // 1. Turn off the candle light
        if (candleLight != null)
            candleLight.enabled = false;

        // 2. Make flame material black (invisible)
        if (flameMaterial != null)
        {
            flameMaterial.SetColor("_EmissionColor", Color.black);
            flameMaterial.SetColor("_BaseColor", Color.black); // If used
            DynamicGI.SetEmissive(GetComponent<Renderer>(), Color.black); // Optional
        }
    }
}
