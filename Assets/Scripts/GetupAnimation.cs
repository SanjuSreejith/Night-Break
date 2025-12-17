using System.Collections;
using UnityEngine;

public class PlayerGetUpAnimationController : MonoBehaviour
{
    public Animator playerAnimator;  // Reference to the Animator
    public Camera playerCamera;      // Reference to the camera
    public float animationDuration = 2f; // Set the duration for the get-up animation
    private bool isAnimationFinished = false;

    private PlayerMovement playerMovement; // Reference to the player movement script

    void Start()
    {
        // Disable the player's movement initially
        playerMovement = GetComponent<PlayerMovement>();
        playerMovement.enabled = false;

        // Start the get-up animation
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("GetUp");  // Trigger the get-up animation
        }

        // Start a coroutine to wait for the animation to complete
        StartCoroutine(WaitForAnimationToFinish());
    }

    // Coroutine to wait for the get-up animation to finish
    private IEnumerator WaitForAnimationToFinish()
    {
        // Wait for the animation duration
        yield return new WaitForSeconds(animationDuration);

        // Animation is finished, enable player controls
        isAnimationFinished = true;

        // Enable the player's movement after animation finishes
        playerMovement.enabled = true;
    }

    // Optional: Camera following the player (in case you need to tweak this)
    void Update()
    {
        if (isAnimationFinished)
        {
            // Allow the camera to move freely with the player
            // Assuming you have a camera script handling movement based on player input
        }
    }
}
