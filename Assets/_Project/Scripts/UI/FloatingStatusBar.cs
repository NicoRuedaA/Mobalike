using UnityEngine;
using UnityEngine.UI;
using MobaGameplay.Core;

namespace MobaGameplay.UI
{
    public class FloatingStatusBar : MonoBehaviour
    {
        [SerializeField] private BaseEntity targetEntity;
        
        [Header("UI References")]
        [SerializeField] private Image healthFill;
        [SerializeField] private Image manaFill;

        private void Start()
        {
            if (targetEntity == null)
            {
                targetEntity = GetComponentInParent<BaseEntity>();
            }
        }

        private void Update()
        {
            if (targetEntity == null) return;

            if (healthFill != null && targetEntity.MaxHealth > 0)
            {
                healthFill.fillAmount = Mathf.Clamp01(targetEntity.CurrentHealth / targetEntity.MaxHealth);
            }

            if (manaFill != null && targetEntity.MaxMana > 0)
            {
                manaFill.fillAmount = Mathf.Clamp01(targetEntity.CurrentMana / targetEntity.MaxMana);
            }
        }
    }
}
