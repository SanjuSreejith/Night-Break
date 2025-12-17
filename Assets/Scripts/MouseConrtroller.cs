using UnityEngine;

public class MouseController : MonoBehaviour
{
    void Start()
    {
        // Ensure the cursor is visible and not locked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // Prevent the cursor from being locked or hidden when clicking
        if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Optional: Add movement controls using the mouse or Android controller
        // Example: Rotate an object using the mouse
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Apply rotation to an object (replace with your movement logic)
        transform.Rotate(Vector3.up, mouseX * 5f);
        transform.Rotate(Vector3.left, mouseY * 5f);
    }
}
