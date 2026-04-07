using UnityEngine;
using System.Collections.Generic;

namespace MobaGameplay.UI.Targeting
{
    /// <summary>
    /// Renders an outline effect on hover for targeting UI.
    /// Uses a custom outline shader to highlight selectable objects.
    /// </summary>
    public class HoverOutline : MonoBehaviour
    {
        // Serializable Fields
        [SerializeField] public Color outlineColor = Color.red;
        [SerializeField] public float outlineWidth = 0.015f;

        // Shader name constant
        private const string OUTLINE_SHADER_NAME = "Custom/Outline";
        private const string OUTLINE_COLOR_PROPERTY = "_OutlineColor";
        private const string OUTLINE_WIDTH_PROPERTY = "_Outline";

        // State
        private Renderer[] renderers;
        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
        private Dictionary<Renderer, Material[]> hoveredMaterials = new Dictionary<Renderer, Material[]>();
        private Material outlineMaterial;
        private bool isInitialized = false;
        private bool isHovered = false;

        // Properties
        public bool IsHovered => isHovered;
        public Color OutlineColor
        {
            get => outlineColor;
            set
            {
                outlineColor = value;
                if (outlineMaterial != null)
                {
                    outlineMaterial.SetColor(OUTLINE_COLOR_PROPERTY, outlineColor);
                }
            }
        }

        private void Awake()
        {
            CacheRenderers();
        }

        private void Start()
        {
            InitializeOutline();
        }

        private void CacheRenderers()
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        private void InitializeOutline()
        {
            if (renderers == null || renderers.Length == 0)
            {
                Debug.LogWarning("[HoverOutline] No renderers found for outline effect.");
                return;
            }

            Shader outlineShader = Shader.Find(OUTLINE_SHADER_NAME);
            if (outlineShader == null)
            {
                Debug.LogWarning($"[HoverOutline] Shader '{OUTLINE_SHADER_NAME}' not found. " +
                               "Outline effect will not be visible.");
                return;
            }

            // Create outline material
            outlineMaterial = new Material(outlineShader);
            outlineMaterial.SetColor(OUTLINE_COLOR_PROPERTY, outlineColor);
            outlineMaterial.SetFloat(OUTLINE_WIDTH_PROPERTY, outlineWidth);

            // Setup material arrays
            foreach (var rend in renderers)
            {
                if (ShouldSkipRenderer(rend)) continue;

                Material[] original = rend.sharedMaterials;
                originalMaterials[rend] = original;

                // Create hovered materials array with outline added
                Material[] hovered = new Material[original.Length + 1];
                for (int i = 0; i < original.Length; i++)
                {
                    hovered[i] = original[i];
                }
                hovered[original.Length] = outlineMaterial;
                hoveredMaterials[rend] = hovered;
            }

            isInitialized = true;
        }

        /// <summary>
        /// Enable or disable the hover outline effect.
        /// </summary>
        public void SetHover(bool hovered)
        {
            if (!isInitialized || isHovered == hovered) return;
            
            isHovered = hovered;

            if (outlineMaterial == null || renderers == null) return;

            foreach (var rend in renderers)
            {
                if (ShouldSkipRenderer(rend)) continue;
                
                if (originalMaterials.TryGetValue(rend, out Material[] original) &&
                    hoveredMaterials.TryGetValue(rend, out Material[] hoveredArr))
                {
                    rend.sharedMaterials = isHovered ? hoveredArr : original;
                }
            }
        }

        /// <summary>
        /// Toggle the hover state.
        /// </summary>
        public void ToggleHover()
        {
            SetHover(!isHovered);
        }

        private bool ShouldSkipRenderer(Renderer rend)
        {
            return rend is ParticleSystemRenderer ||
                   rend is TrailRenderer ||
                   rend is LineRenderer;
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void OnDisable()
        {
            // Restore original materials when disabled
            if (isHovered)
            {
                SetHover(false);
            }
        }

        private void Cleanup()
        {
            // Restore original materials
            if (isHovered && renderers != null)
            {
                foreach (var rend in renderers)
                {
                    if (ShouldSkipRenderer(rend)) continue;
                    
                    if (originalMaterials.TryGetValue(rend, out Material[] original))
                    {
                        rend.sharedMaterials = original;
                    }
                }
            }

            // Destroy outline material
            if (outlineMaterial != null)
            {
                Destroy(outlineMaterial);
                outlineMaterial = null;
            }

            isInitialized = false;
            isHovered = false;
        }

        /// <summary>
        /// Update outline color at runtime.
        /// </summary>
        public void SetOutlineColor(Color color)
        {
            outlineColor = color;
            if (outlineMaterial != null)
            {
                outlineMaterial.SetColor(OUTLINE_COLOR_PROPERTY, color);
            }
        }

        /// <summary>
        /// Update outline width at runtime.
        /// </summary>
        public void SetOutlineWidth(float width)
        {
            outlineWidth = width;
            if (outlineMaterial != null)
            {
                outlineMaterial.SetFloat(OUTLINE_WIDTH_PROPERTY, width);
            }
        }
    }
}
