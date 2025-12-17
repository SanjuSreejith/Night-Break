using UnityEngine;

public class PlayerGravity : MonoBehaviour
{
    public float gravity = -9.81f; // Standard Earth gravity
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask;
    
    private Vector3 velocity;
    private bool isGrounded;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>(); // Get the CharacterController component
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small value to keep grounded smoothly
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime); // Move the player with gravity
    }
}
