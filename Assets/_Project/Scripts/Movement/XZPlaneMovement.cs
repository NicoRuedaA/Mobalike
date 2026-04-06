using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public class XZPlaneMovement : BaseMovement
    {
        public enum MovementMode { None, Pathing, Directional, Dashing }

        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float rotationSpeed = 25f;
        
        [Header("Jump & Gravity")]
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -20f;

        [Header("Dash Settings")]
        [SerializeField] private float dashSpeed = 25f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 2f;

        private CharacterController controller;
        private Vector3 targetPosition;
        private Vector3 currentDirection;
        private MovementMode currentMode = MovementMode.None;
        private float currentVelocity = 0f;
        private bool isSprinting = false;
        
        private float verticalVelocity;
        private bool isJumping = false;

        private float dashTimer = 0f;
        private float dashCooldownTimer = 0f;
        private Vector3 dashDirection;
        private Vector3 lookTarget;

        public override float CurrentVelocity => currentVelocity;
        public override bool IsGrounded => controller != null && controller.isGrounded;
        public override bool IsJumping => isJumping;

        private float CurrentSpeed => isSprinting ? sprintSpeed : walkSpeed;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            controller.center = new Vector3(0f, 0.93f, 0f);
            controller.radius = 0.28f;
            controller.height = 1.8f;
        }

        private void Start()
        {
            targetPosition = transform.position;
            lookTarget = transform.position + transform.forward;
        }

        public override void MoveTo(Vector3 destination)
        {
            if (currentMode == MovementMode.Dashing) return;
            destination.y = transform.position.y;
            targetPosition = destination;
            currentMode = MovementMode.Pathing;
        }

        public override void MoveDirection(Vector3 direction)
        {
            if (currentMode == MovementMode.Dashing) return;
            currentDirection = direction.normalized;
            if (currentDirection.sqrMagnitude > 0.01f)
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

        public override void Stop()
        {
            if (currentMode == MovementMode.Dashing) return;
            currentMode = MovementMode.None;
            targetPosition = transform.position;
            currentDirection = Vector3.zero;
            currentVelocity = 0f;
        }

        public override void Jump()
        {
            if (IsGrounded && currentMode != MovementMode.Dashing)
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
            if (dashCooldownTimer <= 0f)
            {
                currentMode = MovementMode.Dashing;
                dashDirection = direction.normalized;
                if (dashDirection == Vector3.zero) dashDirection = transform.forward;
                dashTimer = dashDuration;
                dashCooldownTimer = dashCooldown;
                verticalVelocity = 0f; // Reiniciar gravedad para un dash recto en el aire
            }
        }

        private void Update()
        {
            if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;

            ApplyGravity();
            HandleRotation();

            Vector3 movement = Vector3.zero;

            if (currentMode == MovementMode.Dashing)
            {
                movement = dashDirection * dashSpeed;
                currentVelocity = dashSpeed;
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0f)
                {
                    Stop();
                }
            }
            else if (currentMode == MovementMode.Pathing)
            {
                movement = HandlePathingMovement();
            }
            else if (currentMode == MovementMode.Directional)
            {
                movement = HandleDirectionalMovement();
            }
            else
            {
                currentVelocity = 0f;
            }

            movement.y = verticalVelocity;
            controller.Move(movement * Time.deltaTime);
            
            if (IsGrounded && verticalVelocity < 0f) {
                isJumping = false;
            }
        }

        private void HandleRotation()
        {
            Vector3 dir = (lookTarget - transform.position).normalized;
            if (dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }

        private void ApplyGravity()
        {
            if (IsGrounded && verticalVelocity < 0.0f)
            {
                verticalVelocity = -2f;
            }
            verticalVelocity += gravity * Time.deltaTime;
        }

        private Vector3 HandlePathingMovement()
        {
            Vector3 direction = (targetPosition - transform.position);
            direction.y = 0;
            if (direction.magnitude < 0.1f)
            {
                Stop();
                return Vector3.zero;
            }
            direction.Normalize();
            currentVelocity = CurrentSpeed;
            return direction * CurrentSpeed;
        }

        private Vector3 HandleDirectionalMovement()
        {
            if (currentDirection == Vector3.zero)
            {
                Stop();
                return Vector3.zero;
            }
            currentVelocity = CurrentSpeed;
            return currentDirection * CurrentSpeed;
        }
    }
}
