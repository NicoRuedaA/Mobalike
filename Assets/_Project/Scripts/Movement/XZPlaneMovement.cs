using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Movement
{
    /// <summary>
    /// XZ-plane movement system for MOBA-style characters.
    /// Supports walking, sprinting, aiming, dashing, and jumping.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class XZPlaneMovement : BaseMovement
    {
        // Constants
        private const float GROUND_DEADZONE = 0.01f;
        private const float DIRECTION_DEADZONE = 0.01f;
        private const float PATHING_STOP_DISTANCE = 0.1f;
        private const float MIN_VERTICAL_VELOCITY = -20f;
        private const float GROUND_BASELINE = -2f;
        
        // Movement Settings
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float aimSpeed = 3f;
        [SerializeField] private float rotationSpeed = 25f;
        [SerializeField] private LayerMask groundLayer = ~0;
        
        // Jump & Gravity
        [Header("Jump & Gravity")]
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -20f;

        // Dash Settings
        [Header("Dash Settings")]
        [SerializeField] private float dashSpeed = 25f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 2f;

        // State
        private CharacterController controller;
        private Vector3 targetPosition;
        private Vector3 currentDirection;
        private MovementMode currentMode = MovementMode.None;
        private float currentVelocity = 0f;
        private bool isSprinting = false;
        private bool isAiming = false;
        
        // Vertical
        private float verticalVelocity;
        private bool isJumping = false;

        // Dash State
        private float dashTimer = 0f;
        private float dashCooldownTimer = 0f;
        private Vector3 dashDirection;
        private Vector3 lookTarget;

        // Properties
        public override float CurrentVelocity => currentVelocity;
        public override Vector3 VelocityVector => controller?.velocity ?? Vector3.zero;
        public override bool IsGrounded => CheckGrounded();
        public override bool IsJumping => isJumping;
        public override bool IsDashing => currentMode == MovementMode.Dashing;
        
        public MovementMode CurrentMovementMode => currentMode;

        private float CurrentSpeed
        {
            get
            {
                if (isAiming) return aimSpeed;
                return isSprinting ? sprintSpeed : walkSpeed;
            }
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            ConfigureCharacterController();
        }

        private void Start()
        {
            targetPosition = transform.position;
            lookTarget = transform.position + transform.forward;
        }

        private void ConfigureCharacterController()
        {
            if (controller == null) return;
            
            controller.center = new Vector3(0f, 0.93f, 0f);
            controller.radius = 0.28f;
            controller.height = 1.8f;
            controller.minMoveDistance = 0.001f;
            controller.stepOffset = 0.3f;
        }

        public override void MoveTo(Vector3 destination)
        {
            if (IsDashing) return;
            
            destination.y = transform.position.y;
            targetPosition = destination;
            currentMode = MovementMode.Pathing;
        }

        public override void MoveDirection(Vector3 direction)
        {
            if (IsDashing) return;
            
            currentDirection = direction.normalized;
            
            if (currentDirection.sqrMagnitude > DIRECTION_DEADZONE)
            {
                currentMode = MovementMode.Directional;
            }
            else if (currentMode == MovementMode.Directional)
            {
                Stop();
            }
        }

        public override void SetSprint(bool sprint)
        {
            isSprinting = sprint;
        }

        public override void SetAiming(bool aiming)
        {
            isAiming = aiming;
        }

        public override void Stop()
        {
            if (IsDashing) return;
            
            currentMode = MovementMode.None;
            targetPosition = transform.position;
            currentDirection = Vector3.zero;
            currentVelocity = 0f;
        }

        public override void Jump()
        {
            if (IsGrounded && !IsDashing)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                isJumping = true;
            }
        }

        public override void LookAtPoint(Vector3 point)
        {
            lookTarget = point;
            lookTarget.y = transform.position.y;
        }

        public override void Dash(Vector3 direction)
        {
            if (dashCooldownTimer > 0f) return;

            currentMode = MovementMode.Dashing;
            dashDirection = direction.normalized;
            
            if (dashDirection.sqrMagnitude < DIRECTION_DEADZONE)
            {
                dashDirection = transform.forward;
            }
            
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            verticalVelocity = 0f; // Reset gravity for straight dash
        }

        private void Update()
        {
            // Tick cooldown
            if (dashCooldownTimer > 0f)
            {
                dashCooldownTimer -= Time.deltaTime;
            }

            // Physics
            ApplyGravity();
            HandleRotation();

            // Movement
            Vector3 movement = CalculateMovement();
            
            movement.y = verticalVelocity;
            controller.Move(movement * Time.deltaTime);

            // Landing
            if (IsGrounded && verticalVelocity < 0f)
            {
                isJumping = false;
            }
        }

        private Vector3 CalculateMovement()
        {
            Vector3 movement = Vector3.zero;

            switch (currentMode)
            {
                case MovementMode.Dashing:
                    movement = HandleDashing();
                    break;
                    
                case MovementMode.Pathing:
                    movement = HandlePathing();
                    break;
                    
                case MovementMode.Directional:
                    movement = HandleDirectional();
                    break;
                    
                default:
                    currentVelocity = 0f;
                    break;
            }

            return movement;
        }

        private Vector3 HandleDashing()
        {
            currentVelocity = dashSpeed;
            dashTimer -= Time.deltaTime;
            
            if (dashTimer <= 0f)
            {
                EndDash();
            }
            
            return dashDirection * dashSpeed;
        }

        private void EndDash()
        {
            currentMode = MovementMode.None;
            currentVelocity = 0f;
        }

        private Vector3 HandlePathing()
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0;
            
            float distance = direction.magnitude;
            
            if (distance < PATHING_STOP_DISTANCE)
            {
                Stop();
                return Vector3.zero;
            }
            
            currentVelocity = CurrentSpeed;
            direction.Normalize();
            
            return direction * CurrentSpeed;
        }

        private Vector3 HandleDirectional()
        {
            if (currentDirection.sqrMagnitude < DIRECTION_DEADZONE)
            {
                Stop();
                return Vector3.zero;
            }
            
            currentVelocity = CurrentSpeed;
            return currentDirection * CurrentSpeed;
        }

        private void HandleRotation()
        {
            Vector3 direction = lookTarget - transform.position;
            
            if (direction.sqrMagnitude > GROUND_DEADZONE)
            {
                direction.y = 0;
                direction.Normalize();
                
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        private void ApplyGravity()
        {
            if (IsGrounded && verticalVelocity < 0f)
            {
                // Snap to ground
                verticalVelocity = GROUND_BASELINE;
            }
            else
            {
                // Apply gravity
                verticalVelocity += gravity * Time.deltaTime;
                verticalVelocity = Mathf.Max(verticalVelocity, MIN_VERTICAL_VELOCITY);
            }
        }

        private bool CheckGrounded()
        {
            if (controller != null && controller.isGrounded)
            {
                return true;
            }

            // Extra ground check
            Vector3 spherePosition = new Vector3(
                transform.position.x,
                transform.position.y + controller.radius - 0.1f,
                transform.position.z
            );
            
            return Physics.CheckSphere(
                spherePosition,
                controller.radius,
                groundLayer & ~(1 << 8),
                QueryTriggerInteraction.Ignore
            );
        }

        /// <summary>
        /// Check if dash is available (cooldown finished).
        /// </summary>
        public bool CanDash => dashCooldownTimer <= 0f;

        /// <summary>
        /// Get remaining dash cooldown as percentage.
        /// </summary>
        public float DashCooldownPercent => 
            dashCooldown > 0f ? Mathf.Clamp01(dashCooldownTimer / dashCooldown) : 0f;

        /// <summary>
        /// Get remaining dash cooldown in seconds.
        /// </summary>
        public float DashCooldownRemaining => Mathf.Max(0f, dashCooldownTimer);

        // Movement Mode Enum
        public enum MovementMode
        {
            None,
            Pathing,
            Directional,
            Dashing
        }
    }
}
