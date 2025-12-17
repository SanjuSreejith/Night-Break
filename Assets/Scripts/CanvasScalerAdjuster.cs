using UnityEngine;
using UnityEngine.UI;

public class CanvasAdjuster : MonoBehaviour
{
    public CanvasScaler canvasScaler;    // Reference to the CanvasScaler component
    public Button[] allButtons;          // Array of all buttons you want to resize or change the theme for

    public Color mobileThemeColor = Color.cyan;   // Theme color for mobile
    public Color desktopThemeColor = Color.green; // Theme color for desktop
    public float mobileTextSize = 20f; // Text size for mobile
    public float desktopTextSize = 24f; // Text size for desktop

    void Start()
    {
        AdjustCanvasScaler();
        AdjustThemeAndSize();
    }

    void AdjustCanvasScaler()
    {
        // Check if the device is mobile
        if (IsMobileDevice())
        {
            // Set Canvas Scaler for mobile devices (scale to screen size)
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1080, 1920); // Mobile resolution (portrait)
        }
        else
        {
            // Set Canvas Scaler for desktop/laptop devices (constant pixel size)
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        }
    }

    void AdjustThemeAndSize()
    {
        if (IsMobileDevice())
        {
            // Change theme for mobile (color)
            ChangeButtonTheme(mobileThemeColor);
            ChangeTextSize(mobileTextSize);
        }
        else
        {
            // Change theme for desktop (color)
            ChangeButtonTheme(desktopThemeColor);
            ChangeTextSize(desktopTextSize);
        }
    }

    void ChangeButtonTheme(Color themeColor)
    {
        foreach (Button button in allButtons)
        {
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = themeColor; // Change button background color
            }

            Text buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.color = Color.white; // Change button text color to white
            }
        }
    }

    void ChangeTextSize(float textSize)
    {
        foreach (Button button in allButtons)
        {
            Text buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.fontSize = (int)textSize; // Adjust text size
            }
        }
    }

    bool IsMobileDevice()
    {
        // Check if the current device is a mobile device based on its screen size or platform
        return Application.isMobilePlatform || Screen.width < 1920;
    }
}
