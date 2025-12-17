using Unity.Notifications.Android;
using UnityEngine;
using UnityEngine.Android; // Required for Permission API

public class NotificationManager : MonoBehaviour
{
    void Start()
    {
        RequestNotificationPermission();
    }

    void RequestNotificationPermission()
    {
        // Check if the permission is granted
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            // Request notification permission for Android 13+
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
    }
}
