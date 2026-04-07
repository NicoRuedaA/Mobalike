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
        [Tooltip("La distancia y ángulo base de la cámara respecto al jugador.")]
        [SerializeField] private Vector3 baseOffset = new Vector3(0f, 15f, -10f);
        [Tooltip("Tiempo de suavizado para el movimiento de la cámara.")]
        [SerializeField] private float smoothTime = 0.15f;

        [Header("Hero Shooter Settings")]
        [Tooltip("Si es true, la cámara sigue al jugador y se desplaza hacia el ratón.")]
        [SerializeField] private bool heroShooterMode = true;
        [Tooltip("Cuánto se aleja la cámara hacia el cursor del ratón.")]
        [SerializeField] private float maxMouseOffset = 5f;
        [Tooltip("Suavizado adicional para el offset del ratón.")]
        [SerializeField] private float mouseOffsetSmoothTime = 0.1f;

        [Header("Zoom Settings")]
        [Tooltip("Velocidad de zoom con la rueda del ratón.")]
        [SerializeField] private float zoomSpeed = 1f;
        [Tooltip("Distancia mínima de zoom (multiplicador).")]
        [SerializeField] private float minZoom = 0.4f;
        [Tooltip("Distancia máxima de zoom (multiplicador).")]
        [SerializeField] private float maxZoom = 2.5f;

        [Header("Edge Panning (Movimiento por bordes)")]
        [Tooltip("Velocidad a la que se mueve la cámara al tocar el borde.")]
        [SerializeField] private float panSpeed = 20f;
        [Tooltip("Grosor del borde de la pantalla en píxeles que activa el movimiento.")]
        [SerializeField] private float panBorderThickness = 15f;
        [Tooltip("Limites del mapa para que la cámara no se aleje infinitamente.")]
        [SerializeField] private Vector2 panLimit = new Vector2(50f, 50f);

        // Variables internas
        private Camera cam;
        private Vector3 cameraVelocity = Vector3.zero;
        
        // Punto en el suelo que la cámara está mirando
        private Vector3 lookAtPosition;
        private bool isCenteringOnPlayer = false;

        // Hero Shooter Mouse Offset
        private Vector3 currentMouseOffset = Vector3.zero;
        private Vector3 mouseOffsetVelocity = Vector3.zero;
        private Plane groundPlane;

        // Zoom state
        private float currentZoom = 1f;
        private float targetZoom = 1f;
        private float zoomVelocity = 0f;

        // Calcula el offset real aplicando el multiplicador de zoom
        private Vector3 CurrentOffset => baseOffset * currentZoom;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void Start()
        {
            // Auto-buscar al jugador si no está asignado
            if (playerTarget == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerTarget = playerObj.transform;
                }
            }

            // Configurar FOV y Rotación
            if (cam != null) cam.fieldOfView = targetFOV;
            transform.rotation = Quaternion.Euler(isometricRotation);

            // Inicializar la posición que miramos
            if (playerTarget != null)
            {
                lookAtPosition = playerTarget.position;
                transform.position = lookAtPosition + CurrentOffset;
                groundPlane = new Plane(Vector3.up, new Vector3(0, playerTarget.position.y, 0));
            }
            else
            {
                lookAtPosition = transform.position - CurrentOffset;
                groundPlane = new Plane(Vector3.up, Vector3.zero);
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

            // En modo hero shooter siempre seguimos al jugador (a menos que quieran liberar la cámara, pero para simplificar siempre se sigue al jugador con el offset).
            isCenteringOnPlayer = Keyboard.current.spaceKey.isPressed || heroShooterMode;

            // Manejar el Zoom
            HandleZoom();

            if (heroShooterMode && playerTarget != null)
            {
                HandleHeroShooterCamera();
            }
            else if (!isCenteringOnPlayer)
            {
                HandleEdgePanning();
                currentMouseOffset = Vector3.SmoothDamp(currentMouseOffset, Vector3.zero, ref mouseOffsetVelocity, mouseOffsetSmoothTime);
            }
            else if (playerTarget != null)
            {
                lookAtPosition = playerTarget.position;
                currentMouseOffset = Vector3.SmoothDamp(currentMouseOffset, Vector3.zero, ref mouseOffsetVelocity, mouseOffsetSmoothTime);
            }
        }

        private void HandleHeroShooterCamera()
        {
            // Base player position
            lookAtPosition = playerTarget.position;

            // Only offset towards mouse if aiming or shooting?
            // "Add a dynamic camera offset that pans slightly towards the mouse cursor when the player is aiming."
            // But we can just make it dynamic based on Right Click.
            bool isAiming = Mouse.current.rightButton.isPressed;
            Vector3 targetMouseOffset = Vector3.zero;

            if (isAiming)
            {
                Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (groundPlane.Raycast(ray, out float enter))
                {
                    Vector3 mouseHitPoint = ray.GetPoint(enter);
                    // Distance from player to mouse
                    Vector3 offsetDir = mouseHitPoint - playerTarget.position;
                    offsetDir.y = 0;
                    
                    // We only go up to maxMouseOffset distance
                    if (offsetDir.magnitude > maxMouseOffset)
                    {
                        offsetDir = offsetDir.normalized * maxMouseOffset;
                    }
                    
                    // Or we can just interpolate 50% between player and mouse, clamped.
                    // A simple clamp is offsetDir.
                    targetMouseOffset = offsetDir;
                }
            }

            // Smoothly move the offset
            currentMouseOffset = Vector3.SmoothDamp(currentMouseOffset, targetMouseOffset, ref mouseOffsetVelocity, mouseOffsetSmoothTime);
            
            // Add offset to lookAtPosition
            lookAtPosition += currentMouseOffset;
        }

        private void HandleZoom()
        {
            float scrollValue = Mouse.current.scroll.ReadValue().y;
            
            if (scrollValue != 0f)
            {
                // Normalizar el scroll a +1 o -1 para tener un zoom consistente independiente del ratón
                targetZoom -= Mathf.Sign(scrollValue) * zoomSpeed * 0.2f;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }

            // Suavizar el zoom
            currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, 0.1f);
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

                // Calcular el movimiento relativo a la rotación de la cámara
                Vector3 forward = transform.forward;
                forward.y = 0;
                forward.Normalize();

                Vector3 right = transform.right;
                right.y = 0;
                right.Normalize();

                // Mover el punto que miramos (lookAtPosition) en lugar de la cámara directamente
                Vector3 moveDelta = (forward * panDirection.z + right * panDirection.x) * panSpeed * Time.deltaTime;
                lookAtPosition += moveDelta;

                // Limitar la posición dentro de los límites del mapa
                lookAtPosition.x = Mathf.Clamp(lookAtPosition.x, -panLimit.x, panLimit.x);
                lookAtPosition.z = Mathf.Clamp(lookAtPosition.z, -panLimit.y, panLimit.y);
            }
        }

        private void UpdateCameraPosition()
        {
            // La posición objetivo de la cámara es siempre el punto que miramos + el offset actual (que incluye el zoom)
            Vector3 desiredCameraPosition = lookAtPosition + CurrentOffset;

            // Mover la cámara suavemente hacia esa posición
            transform.position = Vector3.SmoothDamp(transform.position, desiredCameraPosition, ref cameraVelocity, smoothTime);
        }
    }
}