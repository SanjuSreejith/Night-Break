using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public EscapeGameController escapeGameController;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GameSettings.CurrentMode == GameSettings.GameMode.Normal)
        {
            escapeGameController.SetIsNearDoor(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            escapeGameController.SetIsNearDoor(false);
        }
    }
}
