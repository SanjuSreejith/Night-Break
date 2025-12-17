using UnityEngine;

public class MobileFrameRateLimiter : MonoBehaviour
{
    public int targetFrameRate = 30; // Set to your desired frame rate, like 30 or 60

    void Start()
    {
        // Limit the frame rate for mobile devices
        if (Application.isMobilePlatform)
        {
            Application.targetFrameRate = targetFrameRate;
        }
    }
}
