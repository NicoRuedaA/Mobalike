using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MobaGameplay.Abilities;

namespace MobaGameplay.UI
{
    public class AbilitySlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay; // Image Type = Filled, Radial 360
        [SerializeField] private TextMeshProUGUI cooldownText;

        private BaseAbility ability;

        public void AssignAbility(BaseAbility newAbility)
        {
            ability = newAbility;
            
            // Asignar el icono de la habilidad si existe
            if (ability != null && iconImage != null)
            {
                iconImage.sprite = ability.AbilityIcon;
                iconImage.enabled = ability.AbilityIcon != null;
            }
            
            // Inicializar UI limpia
            if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
            if (cooldownText != null) cooldownText.text = "";
        }

        private void Update()
        {
            if (ability == null) return;

            if (ability.IsOnCooldown)
            {
                if (cooldownOverlay != null)
                {
                    cooldownOverlay.fillAmount = ability.CurrentCooldown / ability.MaxCooldown;
                }

                if (cooldownText != null)
                {
                    cooldownText.text = ability.CurrentCooldown.ToString("F1");
                }
            }
            else
            {
                if (cooldownOverlay != null && cooldownOverlay.fillAmount > 0)
                {
                    cooldownOverlay.fillAmount = 0f;
                }

                if (cooldownText != null && cooldownText.text != "")
                {
                    cooldownText.text = "";
                }
            }
        }
    }
}