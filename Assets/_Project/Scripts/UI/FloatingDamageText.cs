using UnityEngine;
using TMPro;
using MobaGameplay.Combat;

namespace MobaGameplay.UI
{
    [RequireComponent(typeof(Billboard))]
    public class FloatingDamageText : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _textMesh;
        [SerializeField] private float launchSpeed = 4f;
        [SerializeField] private float gravity = -9f;
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private float popDuration = 0.15f;
        [SerializeField] private float popScale = 1.5f;

        [Header("Colors")]
        [SerializeField] private Color physicalColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color magicalColor = new Color(0.3f, 0.6f, 1f);
        [SerializeField] private Color trueDamageColor = Color.white;
        [SerializeField] private Color criticalColor = new Color(1f, 0.2f, 0.2f);

        private float fadeTimer;
        private float popTimer;
        private Vector3 velocity;
        private Color targetColor;
        private Vector3 baseScale;

        public void Setup(float damageAmount, DamageType type, bool isCritical = false)
        {
            if (_textMesh == null) _textMesh = GetComponentInChildren<TextMeshPro>();
            
            _textMesh.text = Mathf.RoundToInt(damageAmount).ToString();
            
            switch (type)
            {
                case DamageType.Physical: targetColor = physicalColor; break;
                case DamageType.Magical: targetColor = magicalColor; break;
                case DamageType.TrueDamage: targetColor = trueDamageColor; break;
                default: targetColor = Color.yellow; break;
            }

            if (isCritical)
            {
                targetColor = criticalColor;
                _textMesh.fontSize *= 1.6f;
                _textMesh.fontStyle = FontStyles.Bold;
            }

            _textMesh.color = targetColor;
            fadeTimer = fadeDuration;
            popTimer = 0f;
            baseScale = transform.localScale;
            transform.localScale = Vector3.zero;
            
            velocity = new Vector3(0, launchSpeed, 0);
        }

        private void Update()
        {
            velocity.y += gravity * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;

            if (popTimer < popDuration)
            {
                popTimer += Time.deltaTime;
                float t = popTimer / popDuration;
                float scale = t < 1f ? popScale * t : Mathf.Lerp(popScale, 1f, (t - 1f) * 4f);
                transform.localScale = baseScale * scale;
            }

            fadeTimer -= Time.deltaTime;
            if (fadeTimer <= 0) Destroy(gameObject);
            else
            {
                float alpha = fadeTimer / fadeDuration;
                _textMesh.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            }
        }
    }
}
