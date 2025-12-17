/*using UnityEngine;

public class ConstantFPS : MonoBehaviour
{
    [Header("Target Frame Rate")]
    public int targetFPS = 30;

    void Awake()
    {
        Application.targetFrameRate = targetFPS;
        QualitySettings.vSyncCount = 0; // Disable VSync to allow targetFrameRate to work
    }

    void Update()
    {
        if (Application.targetFrameRate != targetFPS)
        {
            Application.targetFrameRate = targetFPS;
        }
    }
}
*/