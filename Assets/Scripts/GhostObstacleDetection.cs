using UnityEngine;

public class GhostObstacleDetection : MonoBehaviour
{
    public float detectionDistance = 2f; // Distance to detect obstacles
    public float turnSpeed = 100f; // Speed at which the ghost turns

    private void Update()
    {
        DetectAndAvoidObstacle();
    }

    private void DetectAndAvoidObstacle()
    {
        // Cast a ray in front of the ghost to detect obstacles
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, detectionDistance))
        {
            // If an obstacle is detected, turn the ghost
            if (hit.collider != null)
            {
                // Turn away from the obstacle (simple left or right turn)
                transform.Rotate(0, turnSpeed * Time.deltaTime, 0);
            }
        }
    }
}
