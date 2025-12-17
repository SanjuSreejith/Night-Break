using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Player")]
        public float MoveSpeed = 4.0f;
        public float SprintSpeed = 6.0f;
        public float RotationSpeed = 1.0f;
        public float SpeedChangeRate = 10.0f;
        public float Gravity = -15.0f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.5f;
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 90.0f;
        public float BottomClamp = -90.0f;
        public float RotationSmoothTime = 0.05f;

        [Header("Head Bobbing")]
        public float walkBobSpeed = 14f;
        public float walkBobAmount = 0.05f;
        public float sprintBobSpeed = 18f;
        public float sprintBobAmount = 0.1f;
        public float crouchBobSpeed = 8f;
        public float crouchBobAmount = 0.025f;
        private float defaultYPos = 0;
        private float timer = 0;

        [Header("Touch Settings")]
        public float TouchSensitivity = 1f;

        [Header("Control Settings")]
        public bool controlsEnabled = true;

        [Header("Audio Coordination")]
        public PlayerFearEffect fearEffect;

        private Vector2 _touchStart;
        private Vector2 _touchDelta;
        private bool _isSwiping = false;

        private float _cinemachineTargetPitch;
        public float _speed;
        private float _verticalVelocity;
        private float _rotationVelocity;

        private float _targetRotation;
        private float _currentRotation;
        private float _rotationSmoothVelocity;
        private float _speedVelocity;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private Animator _animator;

        private bool movementEnabled = true;
        private const float _threshold = 0.01f;
        // -------------------------
        // FREEZE MOVEMENT ADDITION
        // -------------------------
        private float savedMoveSpeed;
        private float savedSprintSpeed;

        public void FreezeMovement(bool freeze)
        {
            if (freeze)
            {
                savedMoveSpeed = MoveSpeed;
                savedSprintSpeed = SprintSpeed;

                MoveSpeed = 0f;
                SprintSpeed = 0f;

                EnableMovement(false); // your existing system to block input
            }
            else
            {
                MoveSpeed = savedMoveSpeed;
                SprintSpeed = savedSprintSpeed;

                EnableMovement(true);
            }
        }

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
            _animator = GetComponent<Animator>();
            if (CinemachineCameraTarget != null)
            {
                defaultYPos = CinemachineCameraTarget.transform.localPosition.y;
            }
        }

        public void EnableMovement(bool enable)
        {
            movementEnabled = enable;
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("Starter Assets package is missing dependencies.");
#endif
        }

        private void Update()
        {
            if (!movementEnabled || !controlsEnabled) return;
            HandleInput();
            GroundedCheck();
            Move();
            HandleHeadBob();
        }

        private void LateUpdate()
        {
            if (!controlsEnabled) return;
            CameraRotation();
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        private void HandleInput()
        {
            if (Application.isMobilePlatform)
                HandleTouchInput();
            else
                HandleMouseInput();
        }

        private void HandleMouseInput()
        {
            if (_input.look.sqrMagnitude >= _threshold)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                // Clamp input just in case of weird values
                Vector2 lookInput = new Vector2(
                    Mathf.Clamp(_input.look.x, -1000f, 1000f),
                    Mathf.Clamp(_input.look.y, -1000f, 1000f)
                );

                _cinemachineTargetPitch += lookInput.y * RotationSpeed * deltaTimeMultiplier;
                _rotationVelocity = lookInput.x * RotationSpeed * deltaTimeMultiplier;

                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

                _targetRotation += _rotationVelocity;
                _currentRotation = Mathf.SmoothDampAngle(_currentRotation, _targetRotation, ref _rotationSmoothVelocity, RotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, _currentRotation, 0.0f);
            }
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount > 0)
            {
                foreach (UnityEngine.Touch touch in Input.touches)
                {
                    if (touch.position.x > Screen.width / 2)
                    {
                        switch (touch.phase)
                        {
                            case UnityEngine.TouchPhase.Began:
                                _touchStart = touch.position;
                                _isSwiping = true;
                                break;

                            case UnityEngine.TouchPhase.Moved:
                                if (_isSwiping)
                                {
                                    _touchDelta = touch.deltaPosition;
                                    float smoothFactor = 0.1f;

                                    _cinemachineTargetPitch += -_touchDelta.y * TouchSensitivity * smoothFactor;
                                    _rotationVelocity = _touchDelta.x * TouchSensitivity * smoothFactor;

                                    _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
                                    CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

                                    _targetRotation += _rotationVelocity;
                                    _currentRotation = Mathf.SmoothDampAngle(_currentRotation, _targetRotation, ref _rotationSmoothVelocity, RotationSmoothTime);
                                    transform.rotation = Quaternion.Euler(0.0f, _currentRotation, 0.0f);
                                }
                                break;

                            case UnityEngine.TouchPhase.Ended:
                            case UnityEngine.TouchPhase.Canceled:
                                _isSwiping = false;
                                break;
                        }
                    }
                }
            }
        }

        private void HandleHeadBob()
        {
            if (!Grounded) return;

            if (Mathf.Abs(_input.move.x) > 0.1f || Mathf.Abs(_input.move.y) > 0.1f)
            {
                float bobSpeed = _input.sprint ? sprintBobSpeed : walkBobSpeed;
                float bobAmount = _input.sprint ? sprintBobAmount : walkBobAmount;

                timer += Time.deltaTime * bobSpeed;
                CinemachineCameraTarget.transform.localPosition = new Vector3(
                    CinemachineCameraTarget.transform.localPosition.x,
                    defaultYPos + Mathf.Sin(timer) * bobAmount,
                    CinemachineCameraTarget.transform.localPosition.z);
            }
            else
            {
                // Reset to default position when not moving
                timer = 0;
                CinemachineCameraTarget.transform.localPosition = new Vector3(
                    CinemachineCameraTarget.transform.localPosition.x,
                    Mathf.Lerp(CinemachineCameraTarget.transform.localPosition.y, defaultYPos, Time.deltaTime * walkBobSpeed),
                    CinemachineCameraTarget.transform.localPosition.z);
            }
        }

        private void Move()
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            _speed = Mathf.SmoothDamp(_speed, targetSpeed, ref _speedVelocity, 0.1f);

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            if (_input.move != Vector2.zero)
            {
                inputDirection = (transform.right * _input.move.x + transform.forward * _input.move.y).normalized;
            }

            if (Grounded)
            {
                if (_verticalVelocity < 0)
                {
                    _verticalVelocity = -2f;
                }
            }
            else
            {
                _verticalVelocity += Gravity * Time.deltaTime;
                _verticalVelocity = Mathf.Clamp(_verticalVelocity, -50f, 20f);
            }

            _controller.Move(inputDirection * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            float animationSpeedPercent = _speed / SprintSpeed;
            _animator.SetFloat("Speed", animationSpeedPercent, 0.1f, Time.deltaTime);
            _animator.SetBool("isWalking", _speed > 0 && _speed < SprintSpeed);
            _animator.SetBool("isRunning", _speed >= SprintSpeed);

            if (fearEffect != null)
            {
                fearEffect.UpdateMovementState(_speed, _input.move != Vector2.zero, _input.sprint, Grounded);
            }
        }

        public bool IsMoving()
        {
            return _input.move != Vector2.zero;
        }

        public bool IsSprinting()
        {
            return _input.sprint;
        }

        private void CameraRotation()
        {
            // Already handled in Input methods
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
            Gizmos.color = Grounded ? transparentGreen : transparentRed;
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }
    }
}


