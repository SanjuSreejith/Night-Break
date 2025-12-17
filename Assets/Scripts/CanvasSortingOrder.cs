using UnityEngine;

public class CanvasCameraSetter : MonoBehaviour
{
    public Canvas mainMenuCanvas;   // Assign this in the Inspector
    public Canvas settingsCanvas;   // Assign this in the Inspector
    public Camera persistentCamera; // The camera you want to use for Screen Space - Camera

    void Start()
    {
        // Set the camera for both canvases to the persistent camera
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            mainMenuCanvas.worldCamera = persistentCamera;
        }

        if (settingsCanvas != null)
        {
            settingsCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            settingsCanvas.worldCamera = persistentCamera;
        }
    }
}
