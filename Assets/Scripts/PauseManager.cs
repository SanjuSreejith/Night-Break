using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel; // Assign the pause panel UI in Inspector
    public Button resumeButton;   // Assign the Resume button in Inspector
    public Button restartButton;  // Assign the Restart button in Inspector
    public Button quitButton;     // Assign the Quit button in Inspector

    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false); // Hide pause panel at start

        // Assign button functions
        resumeButton.onClick.AddListener(ResumeGame);
        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    void Update()
    {
        // Toggle pause with the Escape key (PC) or Back button (Android)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true); // Show pause panel
        Time.timeScale = 0f; // Freeze game
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false); // Hide pause panel
        Time.timeScale = 1f; // Resume game
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Reset time scale before loading
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload current scene
    }

    public void QuitGame()
    {
        Time.timeScale = 1f; // Ensure normal speed before quitting
       SceneManager.LoadScene("Main Menu"); 
    }
}
