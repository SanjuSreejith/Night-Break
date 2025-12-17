using UnityEngine;

public class PersistentCamera : MonoBehaviour
{
    private static PersistentCamera instance;

    void Awake()
    {
        // Check if there is already an instance of this camera
        if (instance != null)
        {
            // If there is an instance, destroy this duplicate
            Destroy(gameObject);
        }
        else
        {
            // Set the current camera as the singleton instance
            instance = this;

            // Make sure this camera persists between scenes
            DontDestroyOnLoad(gameObject);
        }
    }
}
