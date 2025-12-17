using UnityEngine;
using UnityEngine.UI;

public class FakeDoorTeleport : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform teleportPosition;
    public Button teleportButton;
    public AudioClip openSound;

    [Header("Settings")]
    public float detectionRange = 4f;
    public float teleportDelay = 0.03f; // shorter delay for responsiveness

    private AudioSource audioSource;
    private CharacterController characterController;
    private bool isNearDoor = false;

    private void Start()
    {
        // Disable in Endless mode
        if (GameSettings.CurrentMode == GameSettings.GameMode.Endless)
        {
            if (teleportButton != null)
                teleportButton.gameObject.SetActive(false);
            enabled = false;
            return;
        }

        // Setup teleport button
        if (teleportButton != null)
        {
            teleportButton.gameObject.SetActive(false);
            teleportButton.onClick.AddListener(TeleportPlayer);
        }

        // Setup AudioSource (single reusable instance)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (openSound != null)
        {
            audioSource.clip = openSound;
            audioSource.playOnAwake = false;
            audioSource.volume = 0.9f;
        }

        // Cache CharacterController
        if (player != null)
            characterController = player.GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Only check distance every few frames to save performance
        if (Time.frameCount % 5 != 0) return;

        if (player == null || teleportButton == null) return;

        float distance = Vector3.SqrMagnitude(player.position - transform.position);
        bool nearNow = distance <= detectionRange * detectionRange;

        // Only update UI if state changed
        if (nearNow != isNearDoor)
        {
            teleportButton.gameObject.SetActive(nearNow);
            isNearDoor = nearNow;
        }
    }

    private void TeleportPlayer()
    {
        if (teleportPosition == null || player == null) return;
        StartCoroutine(TeleportRoutine());
    }

    private System.Collections.IEnumerator TeleportRoutine()
    {
        // Play the sound (clip already preloaded)
        if (audioSource.clip != null)
            audioSource.Play();

        // Short delay for smoother visual transition
        yield return new WaitForSeconds(teleportDelay);

        // Temporarily disable CharacterController safely
        if (characterController != null)
        {
            characterController.enabled = false;
            player.position = teleportPosition.position;
            yield return null; // wait one frame
            characterController.enabled = true;
        }
        else
        {
            player.position = teleportPosition.position;
        }
    }
}
