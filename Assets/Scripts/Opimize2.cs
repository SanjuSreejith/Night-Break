using UnityEngine;

public class AutoDistanceCulling : MonoBehaviour
{
    public float maxVisibleDistance = 50f; // Maximum visible distance
    private Transform camTransform;
    private GameObject[] allObjects; // Automatically detect all objects

    void Start()
    {
        camTransform = Camera.main.transform;
        allObjects = FindObjectsOfType<GameObject>(); // Get all objects in the scene
    }

    void Update()
    {
        foreach (GameObject obj in allObjects)
        {
            if (obj == null || obj.CompareTag("MainCamera")) continue; // Ignore camera itself

            float distance = Vector3.Distance(camTransform.position, obj.transform.position);
            bool isVisible = IsObjectVisible(obj);

            // Hide objects beyond max distance, unless visible through a door
            obj.SetActive(distance <= maxVisibleDistance || isVisible);
        }
    }

    bool IsObjectVisible(GameObject obj)
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(obj.transform.position);

        // Check if the object is inside the camera's view
        bool inView = viewPos.z > 0 && viewPos.x > 0 && viewPos.x < 1 && viewPos.y > 0 && viewPos.y < 1;

        if (!inView) return false;

        // Raycast from the camera to the object to check if it’s truly visible
        RaycastHit hit;
        if (Physics.Linecast(camTransform.position, obj.transform.position, out hit))
        {
            return hit.transform == obj.transform; // Return true if it's actually visible
        }

        return false;
    }
}
