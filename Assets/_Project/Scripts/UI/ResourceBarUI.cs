using UnityEngine;
using UnityEngine.UI;

namespace MobaGameplay.UI
{
    public class ResourceBarUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Image fillImage; // Image Type = Filled (Horizontal)
        [SerializeField] private TMPro.TextMeshProUGUI valueText;
        [SerializeField] private float smoothSpeed = 10f;
        
        private float targetFillAmount = 1f;

        public void UpdateValue(float current, float max)
        {
            if (max <= 0) return;
            targetFillAmount = Mathf.Clamp01(current / max);
            if (valueText != null)
                valueText.text = $"{current:0} / {max:0}";
        }

        private void Update()
        {
            if (fillImage != null && fillImage.fillAmount != targetFillAmount)
            {
                fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, smoothSpeed * Time.deltaTime);
            }
        }
    }
}