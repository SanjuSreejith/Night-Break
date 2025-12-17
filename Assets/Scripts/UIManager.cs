using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;

    void Awake()
    {
        // Prevent multiple instances of UIManager
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // You can add additional UI management methods here, like handling buttons, sliders, etc.
}

        
