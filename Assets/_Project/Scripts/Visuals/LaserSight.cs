using UnityEngine;
using UnityEngine.InputSystem;
using MobaGameplay.Core;

namespace MobaGameplay.Visuals
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserSight : MonoBehaviour
    {
        [SerializeField] private float laserLength = 15f;
        [SerializeField] private LayerMask hitLayers;
        
        private LineRenderer lineRenderer;
        private BaseEntity entity;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            entity = GetComponentInParent<BaseEntity>();
            
            // Ensure the line is disabled by default
            lineRenderer.enabled = false;
        }

        private void Update()
        {
            if (Mouse.current == null || entity == null || entity.IsDead)
            {
                lineRenderer.enabled = false;
                return;
            }

            // Muestra el láser si se mantiene click derecho
            bool isAiming = Mouse.current.rightButton.isPressed;
            
            if (isAiming)
            {
                lineRenderer.enabled = true;
                DrawLaser();
            }
            else
            {
                lineRenderer.enabled = false;
            }
        }

        private void DrawLaser()
        {
            Vector3 startPos = transform.position;
            Vector3 forward = transform.forward;

            // Optional: Raycast to stop laser at walls
            if (Physics.Raycast(startPos, forward, out RaycastHit hit, laserLength, hitLayers))
            {
                lineRenderer.SetPosition(0, startPos);
                lineRenderer.SetPosition(1, hit.point);
            }
            else
            {
                lineRenderer.SetPosition(0, startPos);
                lineRenderer.SetPosition(1, startPos + forward * laserLength);
            }
        }
    }
}
