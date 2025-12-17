using UnityEngine;

public class OptimizedRenderDistance : MonoBehaviour
{
    public GameObject playerModel;  // The actual player model (visible)
    public GameObject ghostModel;   // The actual ghost model (visible)
    public GameObject playerCapsule; // The capsule around the player, used for collisions (not visible)
    public GameObject ghostCapsule;  // The capsule around the ghost, used for collisions (not visible)
    
    public float renderDistance = 50f; // Maximum distance for rendering player/ghost models

    private void Update()
    {
        if (Camera.main == null)
        {
            Debug.LogError("No Main Camera found in the scene!");
            return;
        }

        // Get the distance from the camera to the player model
        float playerDistance = Vector3.Distance(Camera.main.transform.position, playerModel.transform.position);
        // Get the distance from the camera to the ghost model
        float ghostDistance = Vector3.Distance(Camera.main.transform.position, ghostModel.transform.position);

        Debug.Log("Player Distance: " + playerDistance + " Ghost Distance: " + ghostDistance);

        // Check if the player model should be rendered
        if (playerDistance <= renderDistance)
        {
            playerModel.SetActive(true); // Enable player model rendering
            Debug.Log("Player model is within render distance and is now visible.");
        }
        else
        {
            playerModel.SetActive(false); // Disable player model rendering
            Debug.Log("Player model is out of render distance and is now hidden.");
        }

        // Check if the ghost model should be rendered
        if (ghostDistance <= renderDistance)
        {
            ghostModel.SetActive(true); // Enable ghost model rendering
            Debug.Log("Ghost model is within render distance and is now visible.");
        }
        else
        {
            ghostModel.SetActive(false); // Disable ghost model rendering
            Debug.Log("Ghost model is out of render distance and is now hidden.");
        }

        // Optionally, if you want the capsules themselves to always stay invisible, you can turn them off
        playerCapsule.SetActive(false); 
        ghostCapsule.SetActive(false);
    }
}
