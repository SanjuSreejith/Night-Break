using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class EnhancedMirrorReflection : MonoBehaviour
{
    public Camera playerCamera; // Reference to the player's camera
    public Camera mirrorCamera; // Reference to the mirror's camera
    public RenderTexture renderTexture; // Render Texture used for reflection
    public LayerMask reflectionLayerMask = -1; // Which layers should be reflected by the mirror
    public float maxReflectionAngle = 120f; // Max angle for the reflection view (from the camera's original position)

    private Quaternion initialMirrorRotation; // Store the initial rotation of the mirror camera

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main; // Automatically use the main camera if none is assigned

        if (mirrorCamera == null)
        {
            Debug.LogError("Mirror Camera is not assigned!");
            return;
        }

        // Store the initial rotation of the mirror camera
        initialMirrorRotation = mirrorCamera.transform.rotation;

        // Ensure the mirror camera uses the render texture
        mirrorCamera.targetTexture = renderTexture;

        // Apply the render texture to the mirror material
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.mainTexture = renderTexture;
        }

        // Set the initial culling mask for the mirror camera
        mirrorCamera.cullingMask = reflectionLayerMask;
    }

    void LateUpdate()
    {
        if (playerCamera == null || mirrorCamera == null) return;

        // Calculate the reflection direction based on the player's movement
        Vector3 toPlayer = playerCamera.transform.position - transform.position;

        // Reflect the player's forward vector relative to the mirror's plane
        Vector3 reflectedDirection = Vector3.Reflect(playerCamera.transform.forward, transform.forward);

        // Limit the camera's rotation to stay within a maximum angle of 120 degrees from the mirror's initial rotation
        float angleDifference = Vector3.Angle(initialMirrorRotation * Vector3.forward, reflectedDirection);

        if (angleDifference > maxReflectionAngle)
        {
            // If the angle exceeds the max, adjust the reflected direction to be within the max limit
            reflectedDirection = Vector3.RotateTowards(initialMirrorRotation * Vector3.forward, reflectedDirection, Mathf.Deg2Rad * maxReflectionAngle, 0f);
        }

        // Set the mirror camera's rotation based on the adjusted direction
        mirrorCamera.transform.rotation = Quaternion.LookRotation(reflectedDirection, Vector3.up);

        // Position the mirror camera at the same location (without altering the camera's angle)
        mirrorCamera.transform.position = new Vector3(playerCamera.transform.position.x, mirrorCamera.transform.position.y, playerCamera.transform.position.z);
    }
}
