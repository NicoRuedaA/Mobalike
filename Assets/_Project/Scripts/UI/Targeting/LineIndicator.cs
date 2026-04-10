using UnityEngine;

namespace MobaGameplay.UI.Targeting
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineIndicator : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        [SerializeField] private float lineWidth = 0.2f;

        // Public read-only property
        public float LineWidth => lineWidth;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = 5; // A rectangle boundary
            lineRenderer.loop = true;
            
            // Align with ground
            transform.eulerAngles = new Vector3(90, 0, 0); 
        }

        public void SetDimensions(float length, float width)
        {
            float halfWidth = width / 2f;
            
            // Draw a rectangle border (local space, Z is up because we rotated 90 degrees)
            lineRenderer.SetPosition(0, new Vector3(-halfWidth, 0, 0));
            lineRenderer.SetPosition(1, new Vector3(-halfWidth, length, 0));
            lineRenderer.SetPosition(2, new Vector3(halfWidth, length, 0));
            lineRenderer.SetPosition(3, new Vector3(halfWidth, 0, 0));
            lineRenderer.SetPosition(4, new Vector3(-halfWidth, 0, 0));
        }
        
        public void SetColor(Color c)
        {
            lineRenderer.startColor = c;
            lineRenderer.endColor = c;
            if (lineRenderer.material != null)
                lineRenderer.material.color = c;
        }
    }
}