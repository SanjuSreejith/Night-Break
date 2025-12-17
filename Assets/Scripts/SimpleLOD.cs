using UnityEngine;

public class SimpleLOD : MonoBehaviour
{
    public GameObject highDetailModel;  // High-detail model to use when close
    public GameObject lowDetailModel;   // Low-detail model to use when far
    public float lodDistance = 50f;     // Distance at which to switch models

    void Update()
    {
        float distance = Vector3.Distance(Camera.main.transform.position, transform.position);

        if (distance > lodDistance)
        {
            // If the player is far enough, show low-detail model
            highDetailModel.SetActive(false);
            lowDetailModel.SetActive(true);
        }
        else
        {
            // If the player is close, show high-detail model
            highDetailModel.SetActive(true);
            lowDetailModel.SetActive(false);
        }
    }
}
