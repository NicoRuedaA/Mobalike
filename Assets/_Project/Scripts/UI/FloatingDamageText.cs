using UnityEngine;
using TMPro;
using MobaGameplay.Combat;

namespace MobaGameplay.UI
{
    [RequireComponent(typeof(Billboard))]
    public class FloatingDamageText : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _textMesh;
        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private float _fadeDuration = 1f;

        private float _fadeTimer;
        private Color _initialColor;

        public void Setup(float damageAmount, DamageType type)
        {
            if (_textMesh == null) _textMesh = GetComponentInChildren<TextMeshPro>();

            _textMesh.text = Mathf.RoundToInt(damageAmount).ToString();
            
            switch (type)
            {
                case DamageType.Physical:
                    _initialColor = new Color(1f, 0.4f, 0f); // Orange-Red
                    break;
                case DamageType.Magical:
                    _initialColor = new Color(0.2f, 0.8f, 1f); // Cyan
                    break;
                case DamageType.TrueDamage:
                    _initialColor = Color.white;
                    break;
                default:
                    _initialColor = Color.yellow;
                    break;
            }

            _textMesh.color = _initialColor;
            _fadeTimer = _fadeDuration;
        }

        private void Update()
        {
            // Move upwards
            transform.position += Vector3.up * _moveSpeed * Time.deltaTime;

            // Fade out
            _fadeTimer -= Time.deltaTime;
            if (_fadeTimer <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                float alpha = _fadeTimer / _fadeDuration;
                _textMesh.color = new Color(_initialColor.r, _initialColor.g, _initialColor.b, alpha);
            }
        }
    }
}