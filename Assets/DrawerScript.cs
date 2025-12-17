using UnityEngine;
using System.Collections;

public class DrawerOpenClose : MonoBehaviour
{
    public Vector3 openPositionOffset = new Vector3(0, 0, 0.3f);
    public float speed = 2.0f;
    public AudioSource audioSource; // Audio source for drawer sound
    public AudioClip drawerSound;   // Sound clip for the drawer opening/closing

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;
    private bool isMoving = false;

    void Start()
    {
        closedPosition = transform.localPosition;
        openPosition = closedPosition + openPositionOffset;
    }

    public void ToggleDrawer()
    {
        if (!isMoving)
        {
            if (audioSource != null && drawerSound != null)
            {
                audioSource.PlayOneShot(drawerSound); // Play sound
            }
            StartCoroutine(MoveDrawer(isOpen ? closedPosition : openPosition));
            isOpen = !isOpen;
        }
    }

    private IEnumerator MoveDrawer(Vector3 targetPosition)
    {
        isMoving = true;
        float time = 0;
        Vector3 startPosition = transform.localPosition;

        while (time < 1)
        {
            time += Time.deltaTime * speed;
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, time);
            yield return null;
        }

        transform.localPosition = targetPosition;
        isMoving = false;
    }
}
