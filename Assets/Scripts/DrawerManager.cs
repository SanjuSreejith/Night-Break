using UnityEngine;
using UnityEngine.UI;

public class DrawerManager : MonoBehaviour
{
    public Transform player; // Assign the player in the Inspector
    public Button drawerButton; // Assign the UI Button
    public float detectionRange = 2.0f; // Distance to detect drawers

    private GameObject nearestDrawer = null; // Stores the closest drawer

    void Start()
    {
        if (drawerButton != null)
        {
            drawerButton.gameObject.SetActive(false); // Hide button initially
            drawerButton.onClick.AddListener(OpenCloseNearestDrawer);
        }
    }

    void Update()
    {
        FindNearestDrawer();
        
        if (nearestDrawer != null)
        {
            drawerButton.gameObject.SetActive(true);
            PositionButton();
        }
        else
        {
            drawerButton.gameObject.SetActive(false);
        }
    }

    void FindNearestDrawer()
    {
        GameObject[] drawers = GameObject.FindGameObjectsWithTag("Drawer");
        float closestDistance = detectionRange;
        nearestDrawer = null;

        foreach (GameObject drawer in drawers)
        {
            float distance = Vector3.Distance(player.position, drawer.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestDrawer = drawer;
            }
        }
    }

    void PositionButton()
    {
        if (nearestDrawer != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(nearestDrawer.transform.position + Vector3.up * 1.0f);
            drawerButton.transform.position = screenPos;
        }
    }

    void OpenCloseNearestDrawer()
    {
        if (nearestDrawer != null)
        {
            DrawerOpenClose drawerScript = nearestDrawer.GetComponent<DrawerOpenClose>();
            if (drawerScript != null)
            {
                drawerScript.ToggleDrawer();
            }
        }
    }
}
