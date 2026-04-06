using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public class XZPlaneMovement : BaseMovement
    {
        public enum MovementMode { None, Pathing, Directional }

        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float sprintSpeed = 6f;
        [SerializeField] private float rotationSpeed = 15f;
        
        [Header("Jump & Gravity")]
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -15f;

        private CharacterController controller;
        private Vector3 targetPosition;
        private Vector3 currentDirection;
        private MovementMode currentMode = MovementMode.None;
        private float currentVelocity = 0f;
        private bool isSprinting = false;
        
        private float verticalVelocity;
        private bool isJumping = false;

        public override float CurrentVelocity => currentVelocity;
        public override bool IsGrounded => controller != null && controller.isGrounded;
        public override bool IsJumping => isJumping;

        private float CurrentSpeed => isSprinting ? sprintSpeed : walkSpeed;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            // Asegurarnos de que el collider tiene las medidas estándar humanoides
            controller.center = new Vector3(0f, 0.93f, 0f);
            controller.radius = 0.28f;
            controller.height = 1.8f;
        }

        private void Start()
        {
            targetPosition = transform.position;
        }

        public override void MoveTo(Vector3 destination)
        {
            destination.y = transform.position.y;
            targetPosition = destination;
            currentMode = MovementMode.Pathing;
        }

        public override void MoveDirection(Vector3 direction)
        {
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
            currentMode = MovementMode.None;
            targetPosition = transform.position;
            currentDirection = Vector3.zero;
            currentVelocity = 0f;
        }

        public override void Jump()
        {
            if (IsGrounded)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                isJumping = true;
            }
        }

        private void Update()
        {
            ApplyGravity();

            Vector3 movement = Vector3.zero;

            if (currentMode == MovementMode.Pathing)
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

            // Aplicar gravedad al movimiento final
            movement.y = verticalVelocity;

            // Mover físicamente al jugador (CharacterController se encarga de colisiones)
            controller.Move(movement * Time.deltaTime);
            
            // Reiniciar salto al tocar suelo de nuevo
            if (IsGrounded && verticalVelocity < 0f) {
                isJumping = false;
            }
        }

        private void ApplyGravity()
        {
            if (IsGrounded && verticalVelocity < 0.0f)
            {
                verticalVelocity = -2f; // Mantenerlo pegado al suelo
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

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

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
            
            Quaternion targetRotation = Quaternion.LookRotation(currentDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            return currentDirection * CurrentSpeed;
        }
    }
}
