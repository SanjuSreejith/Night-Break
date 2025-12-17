using UnityEngine;

public class DynamicResolution : MonoBehaviour
{
    public float targetFrameRate = 30f;
    public float scaleFactor = 1.0f; // Initial scale factor

    void Update()
    {
        // Calculate the current frame rate
        float currentFrameRate = 1.0f / Time.deltaTime;

        // Adjust the resolution scale based on the frame rate
        if (currentFrameRate < targetFrameRate)
        {
            scaleFactor = Mathf.Max(0.5f, scaleFactor - 0.05f); // Lower the resolution scale
        }
        else
        {
            scaleFactor = Mathf.Min(1.0f, scaleFactor + 0.05f); // Increase the resolution scale
        }

        // Apply the resolution scale
        Screen.SetResolution((int)(Screen.width * scaleFactor), (int)(Screen.height * scaleFactor), true);
    }
}
