using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class PlayerControlWithUI : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private bool isCursorLocked = false;

    [Header("UI Integration")]
    public Vector2 virtualMoveInput = Vector2.zero;
    public Vector2 virtualLookInput = Vector2.zero;
    public bool virtualJumpInput = false;
    public bool virtualSprintInput = false;

    private bool isRunning = false; // Track the running state
    private bool isJumping = false; // Track the jumping state

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        LockCursor(false); // Start with the cursor visible
    }

    void Update()
    {
        HandleCursorToggle();

        if (IsPointerOverUI())
        {
            return; // Ignore player input when interacting with UI
        }

        HandleMovement();
        HandleCameraControl();
    }

    private void HandleMovement()
    {
        float moveX = virtualMoveInput.x != 0 ? virtualMoveInput.x : Input.GetAxis("Horizontal");
        float moveZ = virtualMoveInput.y != 0 ? virtualMoveInput.y : Input.GetAxis("Vertical");

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Handle running state based on button press or virtual input
        isRunning = virtualSprintInput || Input.GetKey(KeyCode.LeftShift);

        float curSpeedX = isRunning ? runSpeed * moveZ : walkSpeed * moveZ;
        float curSpeedY = isRunning ? runSpeed * moveX : walkSpeed * moveX;

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Jumping logic based on button press or virtual input
        if ((virtualJumpInput || Input.GetButton("Jump")) && characterController.isGrounded && !isJumping)
        {
            isJumping = true;
            moveDirection.y = jumpPower;
        }
        else if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else
        {
            isJumping = false; // Reset jumping state when grounded
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleCameraControl()
    {
        float lookX = virtualLookInput.x != 0 ? virtualLookInput.x : Input.GetAxis("Mouse X");
        float lookY = virtualLookInput.y != 0 ? virtualLookInput.y : Input.GetAxis("Mouse Y");

        rotationX += -lookY * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookX * lookSpeed, 0);
    }

    private void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LockCursor(!isCursorLocked);
        }
    }

    private void LockCursor(bool lockCursor)
    {
        isCursorLocked = lockCursor;
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    public void SetVirtualMoveInput(Vector2 input)
    {
        virtualMoveInput = input;
    }

    public void SetVirtualLookInput(Vector2 input)
    {
        virtualLookInput = input;
    }

    public void SetVirtualJumpInput(bool input)
    {
        virtualJumpInput = input;
    }

    public void SetVirtualSprintInput(bool input)
    {
        virtualSprintInput = input;
    }
}
