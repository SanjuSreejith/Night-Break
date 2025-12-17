using UnityEngine;
using StarterAssets;

public class TorchHandController : MonoBehaviour
{
    [Header("References")]
    public TorchSystem torchSystem;
    public GameObject torchObject;
    public GameObject handObject;
    public Transform idleHandPosition;
    public Transform runningHandPosition;
    public Animator torchAnimator;
    public string torchRunBool = "TorchRunning";
    public FirstPersonController playerController;

    [Header("Settings")]
    public float sprintSpeedThreshold = 5.99f;
    public float sprintCooldownThreshold = 5.5f;
    public float stateChangeDelay = 0.2f;
    public Vector3 lockedTorchScale = Vector3.one;

    private bool _isSprinting;
    private float _lastSprintChangeTime;

    private void Update()
    {
        if (playerController == null || torchSystem == null || handObject == null)
            return;

        // ----------------------------------------------------
        //  NEW: Disable arms & torch behaviour while hiding
        // ----------------------------------------------------
        if (BarrelHidingSpot.IsPlayerHiding)
        {
            handObject.SetActive(false);

            if (torchAnimator != null)
                torchAnimator.enabled = false;

            if (torchObject != null)
            {
                // Prevents movement, lock scale, and freezes animation
                torchObject.transform.localScale = lockedTorchScale;
            }

            _isSprinting = false; // Force no sprinting
            return; // Skip the rest of the logic
        }
        // ----------------------------------------------------

        float currentSpeed = playerController._speed;
        bool rawSprintState = currentSpeed >= sprintSpeedThreshold;

        if (rawSprintState && !_isSprinting)
        {
            _isSprinting = true;
            _lastSprintChangeTime = Time.time;
        }
        else if (!rawSprintState && _isSprinting && currentSpeed <= sprintCooldownThreshold)
        {
            if (Time.time - _lastSprintChangeTime >= stateChangeDelay)
                _isSprinting = false;
        }

        // Hand visibility when NOT hiding
        handObject.SetActive(_isSprinting);

        // Torch handling
        if (torchSystem.isPicked && torchObject != null)
        {
            Transform targetPosition = _isSprinting ? runningHandPosition : idleHandPosition;

            if (torchObject.transform.parent != targetPosition)
            {
                torchObject.transform.SetParent(targetPosition, false);
                torchObject.transform.localPosition = Vector3.zero;
                torchObject.transform.localRotation = Quaternion.identity;
                torchObject.transform.localScale = lockedTorchScale;
            }

            if (torchAnimator != null)
            {
                torchAnimator.enabled = true;
                torchAnimator.SetBool(torchRunBool, _isSprinting);
            }
        }
        else if (torchObject != null)
        {
            torchObject.transform.localScale = lockedTorchScale;
        }
    }

    private void LateUpdate()
    {
        if (torchObject != null)
        {
            torchObject.transform.localScale = lockedTorchScale;
        }
    }
}
