using UnityEngine;
using UnityEngine.UI;

public class KeyPickup : MonoBehaviour
{
    public GameObject pickupButton; // UI button to pick up key
    public AudioClip pickupSound;   // Sound effect when picking up the key
    private AudioSource audioSource;
    private bool isPlayerNear = false;

    public static bool hasKey = false; // Track if player has picked the key

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        pickupButton.SetActive(false); // Hide pickup button initially
    }

    private void Update()
    {
        if (isPlayerNear && !hasKey && Input.GetKeyDown(KeyCode.E))
        {
            PickupKey();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasKey)
        {
            pickupButton.SetActive(true);
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            pickupButton.SetActive(false);
            isPlayerNear = false;
        }
    }

    public void PickupKey()
    {
        if (!hasKey)
        {
            hasKey = true; // Set key status
            pickupButton.SetActive(false); // Hide button after pickup
            audioSource.PlayOneShot(pickupSound); // Play sound
            gameObject.SetActive(false); // Hide key
        }
    }
}
