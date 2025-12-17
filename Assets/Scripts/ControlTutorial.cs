using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class ControlTutorial : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject joystick;
    public GameObject runButton;
    public GameObject pickupButton;       // Pickup & Hide
    public GameObject openDoorButton;
    public GameObject torchButton;        // Torch button included
    public GameObject exitDoorButton;
    public TextMeshProUGUI instructionsText;
    public GameObject nextButton;

    [Header("Highlight Settings")]
    public Color highlightColor = Color.yellow;
    public Color defaultColor = Color.white;

    [Header("Joystick Settings")]
    public RectTransform joystickTransform;
    public Vector3 joystickTargetOffset = new Vector3(50, 0, 0); // Move during tutorial
    public float joystickMoveSpeed = 2f; // Animation speed

    private int stepIndex = 0;
    private string cameFrom;
    private bool isHidden = false;

    private Vector3 joystickOriginalPos;
    private bool animateJoystick = false;

    private string[] tutorialSteps = new string[]
    {
        "Use the joystick to move your character around the environment.",
        "Hold the Run button to sprint and move faster.",
        "Press the Pickup button to pick up objects in the world.",
        "You can also use the same Pickup button to hide in hideable spots.",
        "Press the Open Door button to interact with doors and open them.",
        "Use the Torch button to turn it on and off as needed.",
        "This is the Exit Door. Press it to complete the tutorial."
    };

    void Start()
    {
        cameFrom = PlayerPrefs.GetString("CameFrom", "Unknown");

        // Hide all buttons initially
        pickupButton.SetActive(false);
        openDoorButton.SetActive(false);
        torchButton.SetActive(false);
        exitDoorButton.SetActive(false);

        if (joystickTransform != null)
            joystickOriginalPos = joystickTransform.localPosition;

        ShowStep(0);
    }

    void Update()
    {
        // Animate joystick if tutorial step 0 is active
        if (animateJoystick && joystickTransform != null)
        {
            Vector3 targetPos = joystickOriginalPos + joystickTargetOffset;
            joystickTransform.localPosition = Vector3.Lerp(joystickTransform.localPosition, targetPos, Time.deltaTime * joystickMoveSpeed);
        }
    }

    public void ShowNextStep()
    {
        // Hide current step buttons and reset joystick
        HideCurrentStepButtons();

        stepIndex++;
        if (stepIndex < tutorialSteps.Length)
        {
            ShowStep(stepIndex);
        }
        else
        {
            EndTutorial();
        }
    }

    private void ShowStep(int index)
    {
        instructionsText.text = tutorialSteps[index];

        // Reset all button colors
        ResetElement(joystick);
        ResetElement(runButton);
        ResetElement(pickupButton);
        ResetElement(openDoorButton);
        ResetElement(torchButton);
        ResetElement(exitDoorButton);

        // Show & highlight the relevant button for this step
        switch (index)
        {
            case 0:
                HighlightAndShowButton(joystick);
                animateJoystick = true;
                break;
            case 1:
                HighlightAndShowButton(runButton);
                break;
            case 2:
            case 3:
                HighlightAndShowButton(pickupButton);
                pickupButton.SetActive(true);
                break;
            case 4:
                HighlightAndShowButton(openDoorButton);
                openDoorButton.SetActive(true);
                break;
            case 5:
                HighlightAndShowButton(torchButton);
                torchButton.SetActive(true);
                break;
            case 6:
                HighlightAndShowButton(exitDoorButton);
                exitDoorButton.SetActive(true);
                break;
        }

        nextButton.SetActive(true);
    }

    private void HighlightAndShowButton(GameObject button)
    {
        button.SetActive(true);
        var image = button.GetComponent<Image>();
        if (image)
            image.color = highlightColor;
    }

    private void HideCurrentStepButtons()
    {
        switch (stepIndex)
        {
            case 0:
                joystick.SetActive(false);
                animateJoystick = false;
                if (joystickTransform != null)
                    joystickTransform.localPosition = joystickOriginalPos;
                break;
            case 1: runButton.SetActive(false); break;
            case 2:
            case 3: pickupButton.SetActive(false); break;
            case 4: openDoorButton.SetActive(false); break;
            case 5: torchButton.SetActive(false); break;
            case 6: exitDoorButton.SetActive(false); break;
        }
    }

    private void ResetElement(GameObject element)
    {
        var image = element.GetComponent<Image>();
        if (image) image.color = defaultColor;
    }

    public void OnPickupButtonPressed()
    {
        if (stepIndex == 2)
        {
            ShowNextStep();
        }
        else if (stepIndex == 3)
        {
            isHidden = !isHidden;
            instructionsText.text = isHidden ? "You are now hidden!" : "You came out from hiding!";
            ShowNextStep();
        }
    }

    public void OnOpenDoorButtonPressed()
    {
        if (stepIndex == 4)
        {
            ShowNextStep();
        }
    }

    public void OnTorchButtonPressed()
    {
        if (stepIndex == 5)
        {
            ShowNextStep();
        }
    }

    public void OnExitDoorButtonPressed()
    {
        if (stepIndex == 6)
        {
            ShowNextStep();
        }
    }

    private void EndTutorial()
    {
        instructionsText.text = "Tutorial Complete! You are ready to start the game.";
        nextButton.SetActive(false);

        joystick.SetActive(false);
        runButton.SetActive(false);
        pickupButton.SetActive(false);
        openDoorButton.SetActive(false);
        torchButton.SetActive(false);
        exitDoorButton.SetActive(false);

        if (cameFrom == "Main Menu")
        {
            PlayerPrefs.SetString("CameFrom", "");
            SceneManager.LoadScene("Game");
        }
        else
        {
            PlayerPrefs.SetString("OpenHowToPlay", "true");
            PlayerPrefs.SetString("CameFrom", "");
            SceneManager.LoadScene("Main Menu");
        }
    }
}
