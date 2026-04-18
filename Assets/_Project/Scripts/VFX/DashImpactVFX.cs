using UnityEngine;

namespace MobaGameplay.VFX
{
    /// <summary>
    /// VFX component for dash impact. Scales up, fades out, and auto-destroys.
    /// Attach to a GameObject with a Renderer (Sphere, Quad, etc.).
    /// </summary>
    public class DashImpactVFX : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float maxScale = 2f;
        
        [Header("Colors")]
        [SerializeField] private Color startColor = Color.white;
        [SerializeField] private Color endColor = Color.clear;

        private float timer;
        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // Escalar y fade out
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * maxScale, t);
            
            if (_renderer != null)
            {
                Color c = Color.Lerp(startColor, endColor, t);
                _renderer.material.color = c;
            }

            if (timer >= duration)
            {
                Destroy(gameObject);
            }
        }
    }
}
