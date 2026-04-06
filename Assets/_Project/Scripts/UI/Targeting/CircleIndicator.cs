using UnityEngine;

namespace MobaGameplay.UI.Targeting
{
    [RequireComponent(typeof(LineRenderer))]
    public class CircleIndicator : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        public int segments = 50;
        public float lineWidth = 0.2f;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = segments + 1;
            lineRenderer.loop = true;
            
            // Align with ground
            transform.eulerAngles = new Vector3(90, 0, 0); 
        }

        public void SetRadius(float radius)
        {
            float angle = 20f;

            for (int i = 0; i < (segments + 1); i++)
            {
                float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
                float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

                // local space
                lineRenderer.SetPosition(i, new Vector3(x, z, 0));

                angle += (360f / segments);
            }
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