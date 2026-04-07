using UnityEngine;
using UnityEngine.InputSystem;
using MobaGameplay.Core;
using MobaGameplay.Combat;

namespace MobaGameplay.Visuals
{
    /// <summary>
    /// Visual laser sight for ranged attacks.
    /// Changes color and width based on charged attack progress.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LaserSight : MonoBehaviour
    {
        // Constants
        private const string SPRITES_SHADER = "Sprites/Default";
        private const float DEFAULT_LASER_LENGTH = 15f;
        private const float DEFAULT_NORMAL_WIDTH = 0.02f;
        private const float DEFAULT_MAX_WIDTH = 0.06f;
        private const float END_WIDTH_MULTIPLIER = 0.5f;

        // Serializable Fields
        [Header("Laser Settings")]
        [SerializeField] private float laserLength = DEFAULT_LASER_LENGTH;
        [SerializeField] private LayerMask hitLayers = ~0;
        
        [Header("Charge Colors")]
        [SerializeField] private Color normalColor = new Color(0.3f, 0.9f, 0.3f, 0.5f);
        [SerializeField] private Color chargedColor = new Color(1f, 0.4f, 0.1f, 0.8f);

        // Cached References
        private LineRenderer lineRenderer;
        private BaseEntity entity;
        private RangedCombat rangedCombat;
        private Material lineMaterial;

        // State
        private bool isInitialized = false;

        // Properties
        public float LaserLength
        {
            get => laserLength;
            set => laserLength = Mathf.Max(0f, value);
        }

        public Color NormalColor
        {
            get => normalColor;
            set => normalColor = value;
        }

        public Color ChargedColor
        {
            get => chargedColor;
            set => chargedColor = value;
        }

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            SetupLineRenderer();
            CacheEntityReferences();
            isInitialized = true;
        }

        private void SetupLineRenderer()
        {
            if (lineRenderer == null) return;

            // Create material that supports vertex colors
            lineMaterial = new Material(Shader.Find(SPRITES_SHADER));
            lineRenderer.material = lineMaterial;
            
            // Initial state
            lineRenderer.enabled = false;
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            
            // Set initial colors
            UpdateLaserColor();
        }

        private void CacheEntityReferences()
        {
            entity = GetComponentInParent<BaseEntity>();
            
            if (entity != null)
            {
                rangedCombat = entity.GetComponent<RangedCombat>();
            }
        }

        private void Update()
        {
            if (!isInitialized || !CanShowLaser())
            {
                DisableLaser();
                return;
            }

            if (IsAiming())
            {
                EnableLaser();
                DrawLaser();
                UpdateLaserColor();
            }
            else
            {
                DisableLaser();
            }
        }

        private bool CanShowLaser()
        {
            if (entity == null || entity.IsDead) return false;
            if (Mouse.current == null) return false;
            return true;
        }

        private bool IsAiming()
        {
            return Mouse.current?.rightButton.isPressed ?? false;
        }

        private void EnableLaser()
        {
            if (!lineRenderer.enabled)
            {
                lineRenderer.enabled = true;
            }
        }

        private void DisableLaser()
        {
            if (lineRenderer.enabled)
            {
                lineRenderer.enabled = false;
            }
        }

        private void DrawLaser()
        {
            Vector3 startPos = transform.position;
            Vector3 direction = transform.forward;

            if (Physics.Raycast(startPos, direction, out RaycastHit hit, laserLength, hitLayers))
            {
                lineRenderer.SetPosition(0, startPos);
                lineRenderer.SetPosition(1, hit.point);
            }
            else
            {
                lineRenderer.SetPosition(0, startPos);
                lineRenderer.SetPosition(1, startPos + direction * laserLength);
            }
        }

        private void UpdateLaserColor()
        {
            if (lineRenderer == null) return;

            float chargePercent = GetChargePercent();
            
            // Interpolate color based on charge
            Color currentColor = Color.Lerp(normalColor, chargedColor, chargePercent);
            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);
            
            // Update width based on charge
            float width = Mathf.Lerp(DEFAULT_NORMAL_WIDTH, DEFAULT_MAX_WIDTH, chargePercent);
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width * END_WIDTH_MULTIPLIER;
        }

        private float GetChargePercent()
        {
            if (rangedCombat == null || !rangedCombat.IsCharging)
            {
                return 0f;
            }

            float maxTime = rangedCombat.MaxChargeTime;
            if (maxTime <= 0f) return 0f;

            return Mathf.Clamp01(rangedCombat.ChargeProgress / maxTime);
        }

        private void OnDisable()
        {
            DisableLaser();
        }

        private void OnDestroy()
        {
            if (lineMaterial != null)
            {
                Destroy(lineMaterial);
                lineMaterial = null;
            }
        }

        /// <summary>
        /// Force update the laser appearance. Call after changing colors/width at runtime.
        /// </summary>
        public void Refresh()
        {
            if (isInitialized)
            {
                UpdateLaserColor();
            }
        }

        /// <summary>
        /// Set laser length at runtime.
        /// </summary>
        public void SetLength(float length)
        {
            laserLength = Mathf.Max(0f, length);
        }

        /// <summary>
        /// Set charge colors at runtime.
        /// </summary>
        public void SetColors(Color normal, Color charged)
        {
            normalColor = normal;
            chargedColor = charged;
            
            if (isInitialized)
            {
                UpdateLaserColor();
            }
        }
    }
}
