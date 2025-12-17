using UnityEngine;

public class FirstPersonCameraCollision : MonoBehaviour
{
    public Transform player;  // The player character
    public float smoothSpeed = 10f;  // How smoothly the camera follows
    public float distance = 0.7f;  // Default distance of the camera from the player
    public float collisionBuffer = 0.2f;  // How much space to leave when colliding with the player model

    private Vector3 currentVelocity = Vector3.zero;  // To smooth the camera movement
    private Vector3 cameraOffset;
    private Vector3 targetPosition;

    void Start()
    {
        // Set the initial camera offset from the player
        cameraOffset = transform.localPosition;
    }

    void Update()
    {
        if (player != null)
        {
            // Calculate the direction of the camera from the player
            Vector3 direction = (transform.position - player.position).normalized;

            // Check if the camera is colliding with any object in its path (i.e., the player model)
            RaycastHit hit;
            if (Physics.Raycast(player.position, direction, out hit, distance))
            {
                // If a collision occurs, move the camera closer to the player (but still maintaining buffer space)
                targetPosition = hit.point + hit.normal * collisionBuffer;
            }
            else
            {
                // If no collision, maintain the default offset position
                targetPosition = player.position + direction * distance;
            }

            // Smoothly move the camera to the target position
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothSpeed * Time.deltaTime);
        }
    }
}
