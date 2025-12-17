using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusicManager : MonoBehaviour
{
    public AudioSource backgroundMusic;  // Shared background music for Main Menu and Settings

    private static BackgroundMusicManager instance;

    private string mainMenuSceneName = "Main Menu";
    private string settingsSceneName = "Settings";

    void Awake()
    {
        // Singleton pattern to persist the object between scenes
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        PlayMusicForCurrentScene();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForCurrentScene();
    }

    private void PlayMusicForCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == mainMenuSceneName || currentScene == settingsSceneName)
        {
            if (!backgroundMusic.isPlaying)
            {
                backgroundMusic.Play();  // Continue playing without restarting
            }
        }
        else
        {
            backgroundMusic.Stop();  // Stop music in other scenes
        }
    }
}
