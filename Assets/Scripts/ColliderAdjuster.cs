using UnityEngine;

public class ColliderAdjuster : MonoBehaviour
{
    public GameObject targetObject; // The parent object containing the cubes.

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object not assigned!");
            return;
        }

        AdjustColliders(targetObject);
    }

    void AdjustColliders(GameObject parent)
    {
        // Get all children with colliders
        Collider[] colliders = parent.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            // Ensure the collider is a BoxCollider
            if (col is BoxCollider boxCollider)
            {
                AdjustBoxCollider(boxCollider);
            }
        }
        Debug.Log("Colliders adjusted successfully!");
    }

    void AdjustBoxCollider(BoxCollider collider)
    {
        // Expand collider size slightly to reduce gaps
        collider.size += new Vector3(0.01f, 0.01f, 0.01f);

        // Optional: Offset the collider slightly to ensure it aligns better
        collider.center = Vector3.zero;
    }
}
