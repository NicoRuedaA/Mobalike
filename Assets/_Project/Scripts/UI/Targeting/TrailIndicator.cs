using UnityEngine;

namespace MobaGameplay.UI.Targeting
{
    /// <summary>
    /// Indicador visual de una habilidad de camino en el suelo (trail).
    /// Dibuja una línea sólida rellena desde el origen al extremo en espacio local.
    /// Puede añadirse dinámicamente sobre un GameObject que ya tenga LineRenderer.
    /// </summary>
    public class TrailIndicator : MonoBehaviour
    {
        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer == null)
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        /// <summary>
        /// Establece la longitud y el ancho del indicador de camino.
        /// Sobreescribe cualquier configuración previa del LineRenderer.
        /// </summary>
        public void SetDimensions(float length, float width)
        {
            _lineRenderer.useWorldSpace  = false;
            _lineRenderer.loop           = false;
            _lineRenderer.positionCount  = 2;
            _lineRenderer.numCapVertices = 4;
            // View: la cinta siempre mira a la cámara, se ve plana desde arriba.
            // No usar TransformZ — eso la dejaría vertical cuando Z apunta al ratón.
            _lineRenderer.alignment  = LineAlignment.View;
            _lineRenderer.startWidth = width;
            _lineRenderer.endWidth   = width;
            // La línea va a lo largo del eje Z local.
            // TargetingManager aplica LookRotation(dirRatón) que alinea Z → ratón.
            _lineRenderer.SetPosition(0, Vector3.zero);
            _lineRenderer.SetPosition(1, new Vector3(0f, 0f, length));
            // Sin tilt — LookRotation ya orienta todo correctamente en plano XZ.
            transform.eulerAngles = Vector3.zero;
        }

        /// <summary>Aplica el color al LineRenderer.</summary>
        public void SetColor(Color color)
        {
            _lineRenderer.startColor = color;
            _lineRenderer.endColor   = color;
            if (_lineRenderer.material != null)
                _lineRenderer.material.color = color;
        }
    }
}
