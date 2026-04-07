using UnityEngine;
using UnityEngine.InputSystem;
using MobaGameplay.Core;
using MobaGameplay.Combat;

namespace MobaGameplay.Visuals
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserSight : MonoBehaviour
    {
        [SerializeField] private float laserLength = 15f;
        [SerializeField] private LayerMask hitLayers;
        
        [Header("Charge Colors")]
        [SerializeField] private Color normalColor = new Color(0.3f, 0.9f, 0.3f, 0.5f);
        [SerializeField] private Color chargedColor = new Color(1f, 0.4f, 0.1f, 0.8f);
        
        private LineRenderer lineRenderer;
        private BaseEntity entity;
        private RangedCombat rangedCombat;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            entity = GetComponentInParent<BaseEntity>();
            
            if (entity != null)
            {
                rangedCombat = entity.GetComponent<RangedCombat>();
            }
            
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
                UpdateLaserColor();
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

        private void UpdateLaserColor()
        {
            if (rangedCombat == null || !rangedCombat.IsCharging)
            {
                // Sin carga - color normal
                lineRenderer.startColor = normalColor;
                lineRenderer.endColor = new Color(normalColor.r, normalColor.g, normalColor.b, 0f);
                return;
            }

            // Con carga - interpolar color según progreso
            float t = rangedCombat.ChargeProgress;
            Color currentColor = Color.Lerp(normalColor, chargedColor, t);
            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);
            
            // También aumentar el ancho según la carga
            float baseWidth = 0.02f;
            float maxWidth = 0.06f;
            lineRenderer.startWidth = Mathf.Lerp(baseWidth, maxWidth, t);
            lineRenderer.endWidth = baseWidth * 0.5f;
        }
    }
}
