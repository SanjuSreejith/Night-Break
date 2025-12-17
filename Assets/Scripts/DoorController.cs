using UnityEngine;
using UnityEngine.UI;

public class DoorInteraction : MonoBehaviour
{
    public GameObject leftDoor;
    public GameObject rightDoor;
    public Transform player;
    public float interactionDistance = 8f;
    public float openSpeed = 2f;
    public AudioClip openSound;
    public AudioClip closeSound;

    private AudioSource audioSource;
    private bool isOpen = false;
    private Quaternion leftDoorOpenRotation;
    private Quaternion rightDoorOpenRotation;
    private Quaternion leftDoorClosedRotation;
    private Quaternion rightDoorClosedRotation;
    private float doorRotationAmount = 0f;

    public GameObject interactButton;
    public Button interactButtonComponent;

    private bool isInteractionEnabled = true;

    private void Start()
    {
        // Disable interaction in Endless mode
        if (GameSettings.CurrentMode == GameSettings.GameMode.Endless)
        {
            isInteractionEnabled = false;
            if (interactButton != null)
                interactButton.SetActive(false);
            enabled = false; // Disable this script entirely
            return;
        }

        audioSource = GetComponent<AudioSource>();

        leftDoorClosedRotation = leftDoor.transform.rotation;
        rightDoorClosedRotation = rightDoor.transform.rotation;

        leftDoorOpenRotation = leftDoor.transform.rotation * Quaternion.Euler(0, -90, 0);
        rightDoorOpenRotation = rightDoor.transform.rotation * Quaternion.Euler(0, 90, 0);

        interactButton.SetActive(false);
        interactButtonComponent.onClick.AddListener(OnInteractButtonPressed);
    }

    private void Update()
    {
        if (!isInteractionEnabled) return;

        float distance = Vector3.Distance(transform.position, player.position);
        float minDistance = 1f;

        if (distance >= minDistance && distance <= interactionDistance)
        {
            if (!interactButton.activeSelf)
                interactButton.SetActive(true);
        }
        else
        {
            if (interactButton.activeSelf)
                interactButton.SetActive(false);
        }

        if (interactButton.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            ToggleDoor();
        }

        SmoothlyMoveDoors();
    }

    private void OnInteractButtonPressed()
    {
        if (isInteractionEnabled)
            ToggleDoor();
    }

    private void ToggleDoor()
    {
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }

    private void OpenDoor()
    {
        isOpen = true;
        doorRotationAmount = 0.01f; // Kickstart slight movement
        audioSource.PlayOneShot(openSound, 2.0f);
    }

    private void CloseDoor()
    {
        isOpen = false;
        doorRotationAmount = 0.99f; // Kickstart slight movement
        audioSource.PlayOneShot(closeSound, 2.0f);
    }


    private void SmoothlyMoveDoors()
    {
        leftDoor.transform.rotation = Quaternion.Slerp(leftDoor.transform.rotation,
            Quaternion.Lerp(leftDoorClosedRotation, leftDoorOpenRotation, doorRotationAmount),
            openSpeed * Time.deltaTime);

        rightDoor.transform.rotation = Quaternion.Slerp(rightDoor.transform.rotation,
            Quaternion.Lerp(rightDoorClosedRotation, rightDoorOpenRotation, doorRotationAmount),
            openSpeed * Time.deltaTime);
    }
}
