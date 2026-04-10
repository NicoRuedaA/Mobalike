using UnityEngine;

namespace MobaGameplay.VFX
{
    public class SimpleVFX : MonoBehaviour
    {
        [SerializeField] private float duration = 1f;
        [SerializeField] private float maxScale = 5f;

        // Public read-only properties
        public float Duration => duration;
        public float MaxScale => maxScale;

        private float time;

        private void Update()
        {
            time += Time.deltaTime;
            float t = time / duration;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * maxScale, t);
            
            if (time >= duration)
            {
                Destroy(gameObject);
            }
        }
    }
}