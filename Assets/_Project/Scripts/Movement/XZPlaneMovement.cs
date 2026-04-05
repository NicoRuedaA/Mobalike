using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Movement
{
    public class XZPlaneMovement : BaseMovement
    {
        public enum MovementMode { None, Pathing, Directional }

        [Header("Movement Settings")]
        [Tooltip("Velocidad de movimiento de la entidad.")]
        [SerializeField] private float moveSpeed = 5f;
        [Tooltip("Velocidad de rotación al cambiar de dirección.")]
        [SerializeField] private float rotationSpeed = 15f;

        private Vector3 targetPosition;
        private Vector3 currentDirection;
        private MovementMode currentMode = MovementMode.None;
        private float currentVelocity = 0f;

        // Propiedad que lee el animador u otros scripts para saber la velocidad
        public override float CurrentVelocity => currentVelocity;

        private void Start()
        {
            targetPosition = transform.position;
        }

        public override void MoveTo(Vector3 destination)
        {
            // Asegurarnos de que la entidad se mueva solo en XZ (ignoramos desniveles por ahora)
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
                // Solo detenemos si veníamos de modo direccional
                Stop();
            }
        }

        public override void Stop()
        {
            currentMode = MovementMode.None;
            targetPosition = transform.position;
            currentDirection = Vector3.zero;
            currentVelocity = 0f;
        }

        private void Update()
        {
            if (currentMode == MovementMode.None)
            {
                currentVelocity = 0f;
                return;
            }

            if (currentMode == MovementMode.Pathing)
            {
                HandlePathingMovement();
            }
            else if (currentMode == MovementMode.Directional)
            {
                HandleDirectionalMovement();
            }
        }

        private void HandlePathingMovement()
        {
            // 1. Mover hacia el objetivo
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            currentVelocity = moveSpeed; // Para el animator

            // 2. Rotar hacia el objetivo (para que mire hacia donde camina)
            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // 3. Comprobar si hemos llegado (con un pequeño margen de error)
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                Stop();
            }
        }

        private void HandleDirectionalMovement()
        {
            if (currentDirection == Vector3.zero)
            {
                Stop();
                return;
            }

            // 1. Mover en la dirección
            Vector3 movement = currentDirection * moveSpeed * Time.deltaTime;
            transform.position += movement;
            currentVelocity = moveSpeed; // Para el animator

            // 2. Rotar suavemente hacia la dirección
            Quaternion targetRotation = Quaternion.LookRotation(currentDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
