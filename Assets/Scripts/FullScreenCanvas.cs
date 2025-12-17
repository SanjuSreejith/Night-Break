using UnityEngine;
using UnityEngine.UI; // This is required for CanvasScaler and UI components.

[RequireComponent(typeof(Canvas))]
public class FullScreenCanvas : MonoBehaviour
{
    public CanvasScaler.ScreenMatchMode screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
    public float matchWidthOrHeight = 0.5f; // 0 = width, 1 = height, 0.5 = balanced

    void Start()
    {
        // Ensure the Canvas component exists
        Canvas canvas = GetComponent<Canvas>();

        // Set the render mode to Screen Space - Overlay for full screen
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Ensure the Canvas Scaler component exists
        CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
        if (canvasScaler == null)
        {
            canvasScaler = gameObject.AddComponent<CanvasScaler>();
        }

        // Configure Canvas Scaler settings for full screen
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080); // Reference resolution
        canvasScaler.screenMatchMode = screenMatchMode;
        canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
    }
}
