using UnityEngine;
using UnityEngine.InputSystem;

namespace MobaGameplay.CameraSystems
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [Header("Auto-Configuration")]
        [Tooltip("Campo de visión automático (FOV).")]
        [SerializeField] private float targetFOV = 60f;
        [Tooltip("Rotación inicial isométrica (grados).")]
        [SerializeField] private Vector3 isometricRotation = new Vector3(55f, 0f, 0f);

        [Header("Targeting & Follow")]
        [Tooltip("El transform del jugador al que la cámara debe seguir.")]
        [SerializeField] private Transform playerTarget;
        [Tooltip("La distancia y ángulo de la cámara respecto al jugador.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -10f);
        [Tooltip("Tiempo de suavizado para el movimiento de la cámara.")]
        [SerializeField] private float smoothTime = 0.15f;

        [Header("Edge Panning (Movimiento por bordes)")]
        [Tooltip("Velocidad a la que se mueve la cámara al tocar el borde.")]
        [SerializeField] private float panSpeed = 20f;
        [Tooltip("Grosor del borde de la pantalla en píxeles que activa el movimiento.")]
        [SerializeField] private float panBorderThickness = 15f;
        [Tooltip("Limites del mapa para que la cámara no se aleje infinitamente.")]
        [SerializeField] private Vector2 panLimit = new Vector2(50f, 50f);

        private Vector3 targetPosition;
        private Vector3 velocity = Vector3.zero;
        private bool isCenteringOnPlayer = false;
        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void Start()
        {
            // 1. Auto-buscar al jugador si no está asignado
            if (playerTarget == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerTarget = playerObj.transform;
                }
            }

            // 2. Configurar el FOV automáticamente
            if (cam != null)
            {
                cam.fieldOfView = targetFOV;
            }

            // 3. Forzar rotación isométrica
            transform.rotation = Quaternion.Euler(isometricRotation);

            // 4. Hacer un "snap" inmediato a la posición del jugador
            if (playerTarget != null)
            {
                targetPosition = playerTarget.position + offset;
                transform.position = targetPosition;
            }
            else
            {
                targetPosition = transform.position;
            }
        }

        private void LateUpdate()
        {
            HandleInput();
            UpdateCameraPosition();
        }

        private void HandleInput()
        {
            if (Keyboard.current == null || Mouse.current == null) return;

            // Al mantener pulsado Espacio, la cámara se centra en el jugador
            isCenteringOnPlayer = Keyboard.current.spaceKey.isPressed;

            // Si no estamos centrando la cámara, comprobar el movimiento por los bordes (Edge Panning)
            if (!isCenteringOnPlayer)
            {
                HandleEdgePanning();
            }
        }

        private void HandleEdgePanning()
        {
            Vector3 panDirection = Vector3.zero;
            Vector2 mousePos = Mouse.current.position.ReadValue();

            // Detectar si el ratón está en los bordes de la pantalla
            if (mousePos.y >= Screen.height - panBorderThickness) panDirection.z += 1f;
            if (mousePos.y <= panBorderThickness) panDirection.z -= 1f;
            if (mousePos.x >= Screen.width - panBorderThickness) panDirection.x += 1f;
            if (mousePos.x <= panBorderThickness) panDirection.x -= 1f;

            if (panDirection != Vector3.zero)
            {
                panDirection.Normalize();

                // Calcular el movimiento relativo a la rotación de la cámara (para mantener el estilo isométrico)
                Vector3 forward = transform.forward;
                forward.y = 0;
                forward.Normalize();

                Vector3 right = transform.right;
                right.y = 0;
                right.Normalize();

                Vector3 moveDelta = (forward * panDirection.z + right * panDirection.x) * panSpeed * Time.deltaTime;
                targetPosition += moveDelta;

                // Limitar la posición de la cámara dentro de los límites del mapa
                targetPosition.x = Mathf.Clamp(targetPosition.x, -panLimit.x, panLimit.x);
                targetPosition.z = Mathf.Clamp(targetPosition.z, -panLimit.y, panLimit.y);
            }
        }

        private void UpdateCameraPosition()
        {
            if (isCenteringOnPlayer && playerTarget != null)
            {
                targetPosition = playerTarget.position + offset;
            }

            // Mover la cámara suavemente hacia la posición objetivo usando SmoothDamp
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}
