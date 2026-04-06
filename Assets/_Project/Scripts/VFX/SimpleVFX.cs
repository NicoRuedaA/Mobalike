using UnityEngine;

namespace MobaGameplay.VFX
{
    public class SimpleVFX : MonoBehaviour
    {
        public float duration = 1f;
        public float maxScale = 5f;
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