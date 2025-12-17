//using UnityEngine;
//using System.Collections;

//public class OptimizedFrustumCulling : MonoBehaviour
//{
//    public Camera playerCamera; // Assign the camera in the Inspector
//    private Renderer objRenderer;
//    private bool isVisible = true; // Track visibility state
//    private WaitForSeconds checkDelay = new WaitForSeconds(0.2f); // Delay between checks

//    void Start()
//    {
//        objRenderer = GetComponent<Renderer>();
//        if (playerCamera == null)
//        {
//            playerCamera = Camera.main; // Assign main camera if not set
//        }
//        StartCoroutine(CheckVisibility());
//    }

//    IEnumerator CheckVisibility()
//    {
//        while (true)
//        {
//            if (playerCamera != null && objRenderer != null)
//            {
//                bool currentlyVisible = IsVisibleFrom(playerCamera, objRenderer);

//                if (currentlyVisible != isVisible)
//                {
//                    isVisible = currentlyVisible;
//                    objRenderer.enabled = isVisible; // Enable/Disable only when needed
//                }
//            }
//            yield return checkDelay; // Check every 0.2 seconds (adjust as needed)
//        }
//    }

//    bool IsVisibleFrom(Camera cam, Renderer renderer)
//    {
//        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
//        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
//    }
//}
