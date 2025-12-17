using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonHandler : MonoBehaviour
{
    // Method to navigate back to the Main Menu
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Main Menu"); // Replace "MainMenu" with your main menu scene name
    }
}