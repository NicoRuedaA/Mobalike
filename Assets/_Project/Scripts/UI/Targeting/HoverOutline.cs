using UnityEngine;
using System.Collections.Generic;

namespace MobaGameplay.UI.Targeting
{
    public class HoverOutline : MonoBehaviour
    {
        public Color outlineColor = Color.red;
        public float outlineWidth = 0.015f;

        private Renderer[] renderers;
        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
        private Dictionary<Renderer, Material[]> hoveredMaterials = new Dictionary<Renderer, Material[]>();
        private Material outlineMaterial;

        private void Start()
        {
            renderers = GetComponentsInChildren<Renderer>();
            
            Shader outlineShader = Shader.Find("Custom/Outline");
            if (outlineShader == null)
            {
                Debug.LogWarning("[HoverOutline] Could not find Custom/Outline shader. Is it in the project?");
                return;
            }

            outlineMaterial = new Material(outlineShader);
            outlineMaterial.SetColor("_OutlineColor", outlineColor);
            outlineMaterial.SetFloat("_Outline", outlineWidth);

            foreach (var rend in renderers)
            {
                // Skip non-mesh renderers that shouldn't have outlines
                if (rend is ParticleSystemRenderer || rend is TrailRenderer || rend is LineRenderer) continue;

                var orig = rend.sharedMaterials;
                originalMaterials[rend] = orig;

                var hov = new Material[orig.Length + 1];
                for (int i = 0; i < orig.Length; i++) hov[i] = orig[i];
                hov[orig.Length] = outlineMaterial;
                hoveredMaterials[rend] = hov;
            }
        }

        public void SetHover(bool isHovered)
        {
            if (outlineMaterial == null || renderers == null) return;

            foreach (var rend in renderers)
            {
                if (rend is ParticleSystemRenderer || rend is TrailRenderer || rend is LineRenderer) continue;
                if (originalMaterials.ContainsKey(rend) && hoveredMaterials.ContainsKey(rend))
                {
                    // Swapping the entire material array
                    rend.sharedMaterials = isHovered ? hoveredMaterials[rend] : originalMaterials[rend];
                }
            }
        }

        private void OnDestroy()
        {
            // Restore original materials if destroyed while hovered
            SetHover(false);
            if (outlineMaterial != null)
            {
                Destroy(outlineMaterial);
            }
        }
    }
}