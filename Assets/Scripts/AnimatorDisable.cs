using UnityEngine;

public class AnimatorDistanceCuller : MonoBehaviour
{
    public Transform player;              // Assign the Player transform in the Inspector
    public float disableDistance = 30f;   // Distance beyond which the Animator is disabled
    public float checkInterval = 1f;      // How often to check distance

    private Animator ghostAnimator;
    private bool isAnimatorEnabled = true;

    void Start()
    {
        ghostAnimator = GetComponent<Animator>();
        InvokeRepeating(nameof(CheckDistance), 0f, checkInterval);
    }

    void CheckDistance()
    {
        if (player == null || ghostAnimator == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > disableDistance && isAnimatorEnabled)
        {
            ghostAnimator.enabled = false;
            isAnimatorEnabled = false;
        }
        else if (distance <= disableDistance && !isAnimatorEnabled)
        {
            ghostAnimator.enabled = true;
            isAnimatorEnabled = true;
        }
    }
}
