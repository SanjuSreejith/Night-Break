using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Ensure you have this if using TextMeshPro
using System.Collections;  // Required for IEnumerator

public class EscapeButtonNearDoorController : MonoBehaviour
{
    public Button escapeButton;        // The UI button for escaping the level
    public GameObject door;            // The door object in the scene
    public TMP_Text messageText;       // Text that shows win/lose messages
    public GameObject winPanel;        // The panel that will display the message when the player wins
    public float triggerDistance = 5f; // The distance within which the escape button will show

    private bool isNearDoor = false;   // Flag to check if player is near the door

    private Collider doorCollider;     // The door's collider to prevent passing through

    // Start is called before the first frame update
    void Start()
    {
        // Hide the escape button initially
        escapeButton.gameObject.SetActive(false);

        // Get the door's collider to use it for preventing the player from passing through
        doorCollider = door.GetComponent<Collider>();
    }

    // When the player enters the trigger zone (near the door)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Check if the player has entered the trigger
        {
            // Show the escape button when player enters the door's trigger zone
            escapeButton.gameObject.SetActive(true);
            isNearDoor = true;

            // Prevent the player from passing through the door by disabling the door's collider temporarily
            doorCollider.isTrigger = false;
        }
    }

    // When the player exits the trigger zone (leaves the door area)
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // Check if the player has exited the trigger
        {
            // Hide the escape button when player leaves the trigger zone
            escapeButton.gameObject.SetActive(false);
            isNearDoor = false;

            // Re-enable the door's collider to allow player to pass through again
            doorCollider.isTrigger = true;
        }
    }

    // When the player presses the escape button
    public void OnEscapeButtonPressed()
    {
        if (isNearDoor)
        {
            // Show the WinPanel
            winPanel.SetActive(true);

            // Optionally, you can add a message or change the state of the game here
            messageText.text = "You Escaped!";  // Example message

            // Example: Optionally stop player movement, play sound, or add animations here

            // Coroutine for a delay before further actions (e.g., scene change or restart)
            StartCoroutine(ShowWinPanelAndProceed());
        }
    }

    // Optional: Coroutine to show the panel and then restart the level or perform an action
    private IEnumerator ShowWinPanelAndProceed()
    {
        yield return new WaitForSeconds(3f); // Show the WinPanel for 3 seconds
        // Here, you can load a new scene or restart the level
        // Example: SceneManager.LoadScene("NextLevel");
    }
}
