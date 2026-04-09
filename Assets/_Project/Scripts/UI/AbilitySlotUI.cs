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

        public BaseAbility GetAbility() => ability;

        public void AssignAbility(BaseAbility newAbility)
        {
            ability = newAbility;
            
            if (ability != null && iconImage != null)
            {
                iconImage.sprite = ability.AbilityIcon;
                iconImage.enabled = ability.AbilityIcon != null;
                Debug.Log($"[AbilitySlotUI] Assigned '{ability.abilityName}' | Icon: {(ability.AbilityIcon != null ? ability.AbilityIcon.name : "NULL")} | Slot: {gameObject.name}");
            }
            else if (ability == null)
            {
                // No desactivar el iconImage completamente, solo limpiar el sprite
                // Esto evita que el slot desaparezca visualmente
                if (iconImage != null)
                {
                    iconImage.sprite = null;
                    // Mantener enabled en true para que el slot siga visible
                }
            }
            
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