using UnityEngine;
using UnityEngine.SceneManagement;
 
public class ChaseCharacter : MonoBehaviour
{
    public Transform player; // reference to the player's transform
    public Animator animator; // reference to the enemy's Animator component
 
    public float moveSpeed = 5f; // the enemy's move speed
    public float rotationSpeed = 5f; // the speed at which the enemy rotates
    public float chaseRange = 10f; // the distance at which the enemy starts chasing the player
    public float deathRange = .75f; // the distance at which the enemy kills the player
 
    private void Update()
    {
        // calculate the distance between the enemy and the player
        float distance = Vector3.Distance(player.position, transform.position);
 
        ChasePlayer(distance); // method that holds the logic for enemy to chase player
        PlayerDeath(distance); // method that reloads the level when enemy catches player
    }
 
    private void PlayerDeath(float distance)
    {
        // if the distance is close enough to the player it reloads the scene
        if (distance < deathRange)
        {
            // loads the active scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
 
    private void ChasePlayer(float distance)
    {
        // if the distance is less than a certain threshold, move towards the player
        if (distance < chaseRange)
        {
            // calculate the direction towards the player
            Vector3 direction = (player.position - transform.position).normalized;
 
            // move the enemy towards the player
            transform.position += direction * moveSpeed * Time.deltaTime;
 
            // calculate the rotation towards the player
            Quaternion lookRotation = Quaternion.LookRotation(direction);
 
            // smoothly rotate towards the player
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
 
            // if the distance is less than a certain threshold, animate the enemy as running
            animator.SetBool("isRunning", true);
        }
        else
        {
            // if the distance is more than a certain threshold, animate the enemy as idle
            animator.SetBool("isRunning", false);
        }
    }
}
