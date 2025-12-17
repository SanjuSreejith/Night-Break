using UnityEngine;

public class GhostAnimatorController : MonoBehaviour
{
    private Animator animator;  // This will hold the Animator component
    private UnityEngine.AI.NavMeshAgent navMeshAgent;  // For controlling the Ghost's movement

    // Animation parameters
    private bool isRunning = false;
    private bool isTurning = false;

    void Start()
    {
        // Automatically get the Animator component attached to this GameObject
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component is missing from this GameObject!");
        }

        // Get the NavMeshAgent to control ghost movement
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent component is missing from this GameObject!");
        }
    }

    void Update()
    {
        // Example logic for when the Ghost should run
        if (isRunning)
        {
            animator.SetBool("isRunning", true);
            animator.SetBool("isWalking", false);
            animator.SetBool("isTurning", false);
        }
        else if (isTurning)
        {
            animator.SetBool("isTurning", true);
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
        }
        else
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", false);
            animator.SetBool("isTurning", false);
        }
        
        // You can add more logic here for ghost behavior, like when it detects the player
        // Example: If the player is within a certain distance, set isRunning to true
        // If the ghost collides with something, set isTurning to true
    }

    public void SetRunning(bool running)
    {
        isRunning = running;
    }

    public void SetTurning(bool turning)
    {
        isTurning = turning;
    }
}
