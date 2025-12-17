using UnityEngine;

public class RotationController : MonoBehaviour
{
    void Start()
    {
        // Set default rotation to Landscape Left at game start
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    void Update()
    {
        // Allow only Landscape Left or Right, block Portrait
        if (Screen.orientation != ScreenOrientation.LandscapeLeft &&
            Screen.orientation != ScreenOrientation.LandscapeRight)
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
    }
}
