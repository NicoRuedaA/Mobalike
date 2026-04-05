using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Velocidad de movimiento del jugador.")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("Velocidad de rotación al cambiar de dirección.")]
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Animation Settings")]
    [Tooltip("Referencia al Animator del personaje. Si está vacío, se buscará automáticamente.")]
    [SerializeField] private Animator animator;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private Plane groundPlane;
    private Camera mainCamera;

    // IDs de los parámetros del Animator (Starter Assets)
    private int animIDSpeed;
    private int animIDMotionSpeed;
    
    // Valor interpolado para la animación suave
    private float animationBlend;

    private void Start()
    {
        mainCamera = Camera.main;
        
        // Creamos un plano matemático infinito en Y=0 mirando hacia arriba (Vector3.up)
        groundPlane = new Plane(Vector3.up, Vector3.zero);
        targetPosition = transform.position;

        // Intentar obtener el Animator del objeto actual o sus hijos
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        AssignAnimationIDs();
    }

    private void AssignAnimationIDs()
    {
        // Convierte los nombres de los parámetros a IDs (es mucho más eficiente)
        animIDSpeed = Animator.StringToHash("Speed");
        animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void Update()
    {
        HandleInput();
        MovePlayer();
        UpdateAnimations();
    }

    private void HandleInput()
    {
        // Verificar que hay un ratón conectado y leer el click derecho
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return; // Si no hay cámara, no podemos calcular el click

            // 1. Obtener la posición del ratón en la pantalla
            Vector2 mousePos = Mouse.current.position.ReadValue();

            // 2. Lanzar un rayo desde la cámara a través de ese punto de la pantalla
            Ray ray = mainCamera.ScreenPointToRay(mousePos);

            // 3. Comprobar dónde choca el rayo con nuestro plano matemático 2D (suelo)
            if (groundPlane.Raycast(ray, out float enterDistance))
            {
                // Obtener el punto exacto en el espacio 3D
                Vector3 hitPoint = ray.GetPoint(enterDistance);
                
                // Asegurarnos de que el jugador se mueva solo en XZ (ignoramos desniveles)
                hitPoint.y = transform.position.y;
                
                targetPosition = hitPoint;
                isMoving = true;
            }
        }
    }

    private void MovePlayer()
    {
        if (!isMoving) return;

        // 1. Mover hacia el objetivo
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

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
            isMoving = false;
        }
    }

    private void UpdateAnimations()
    {
        // Si no tenemos animator, no hacemos nada
        if (animator == null) return;

        // Calculamos la velocidad objetivo (si se mueve, es moveSpeed; si no, es 0)
        float targetSpeed = isMoving ? moveSpeed : 0f;

        // Suavizamos la transición entre quieto y moviéndose (como hace el script original de Starter Assets)
        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * 10f);
        if (animationBlend < 0.01f) animationBlend = 0f;

        // Actualizamos los parámetros del Animator
        animator.SetFloat(animIDSpeed, animationBlend);
        animator.SetFloat(animIDMotionSpeed, 1f); // Velocidad normal de reproducción
    }
}
