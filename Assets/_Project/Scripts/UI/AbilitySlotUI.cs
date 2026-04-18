using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MobaGameplay.Abilities;

namespace MobaGameplay.UI
{
    /// <summary>
    /// UI slot for displaying an ability icon with cooldown overlay.
    /// Uses the data-driven AbilitySystem (no legacy support).
    /// </summary>
    public class AbilitySlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay; // Image Type = Filled, Radial 360
        [SerializeField] private TextMeshProUGUI cooldownText;

        private AbilityData abilityData;
        private AbilitySystem abilitySystem;
        private int slotIndex = -1;

        private void Awake()
        {
            // Auto-find child UI elements if not assigned
            if (iconImage == null)
                iconImage = GetComponentInChildren<Image>();
            
            // Look for cooldown overlay as sibling (not child, as it's used as fill)
            if (cooldownOverlay == null)
            {
                var images = GetComponentsInChildren<Image>();
                foreach (var img in images)
                {
                    if (img.gameObject.name.Contains("CooldownOverlay"))
                    {
                        cooldownOverlay = img;
                        break;
                    }
                }
            }
            
            if (cooldownText == null)
            {
                var texts = GetComponentsInChildren<TMPro.TextMeshProUGUI>();
                foreach (var txt in texts)
                {
                    if (txt.gameObject.name.Contains("CooldownText"))
                    {
                        cooldownText = txt;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Assign an ability from the data-driven system to this slot.
        /// </summary>
        public void AssignAbility(AbilityData data, AbilitySystem system, int index)
        {
            abilityData = data;
            abilitySystem = system;
            slotIndex = index;
            
            UpdateDisplay();
            
            // Force first update immediately so cooldown visualization works
            UpdateCooldown();
        }

        private void UpdateDisplay()
        {
            if (abilityData != null && iconImage != null)
            {
                iconImage.sprite = abilityData.icon;
                iconImage.enabled = abilityData.icon != null;
            }
            else
            {
                if (iconImage != null)
                {
                    iconImage.sprite = null;
                    iconImage.enabled = false;
                }
            }

            // Ensure overlay is always enabled when there's an ability
            if (cooldownOverlay != null)
            {
                cooldownOverlay.enabled = true; // Always enable, toggle visibility via fillAmount
                cooldownOverlay.fillAmount = 0f; // Start at 0 (no cooldown)
            }
            if (cooldownText != null)
            {
                cooldownText.text = "";
                cooldownText.enabled = false;
            }
        }

        private void Update()
        {
            if (abilitySystem != null && slotIndex >= 0 && abilityData != null)
            {
                UpdateCooldown();
            }
        }

        private void UpdateCooldown()
        {
            if (abilitySystem == null || abilityData == null || slotIndex < 0) return;

            var instance = abilitySystem.GetAbilityInstance(slotIndex);
            if (instance == null) return;

            bool onCd = instance.IsOnCooldown;
            
            if (cooldownOverlay != null)
            {
                cooldownOverlay.enabled = true;
                if (onCd && abilityData.cooldown > 0)
                    cooldownOverlay.fillAmount = instance.CurrentCooldown / abilityData.cooldown;
                else
                    cooldownOverlay.fillAmount = 0;
            }
                
            if (cooldownText != null)
            {
                if (onCd)
                {
                    cooldownText.text = instance.CurrentCooldown.ToString("F1");
                    cooldownText.enabled = true;
                }
                else
                {
                    cooldownText.text = "";
                    cooldownText.enabled = false;
                }
            }
        }

        private void ClearCooldownDisplay()
        {
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 0f;
            }
            if (cooldownText != null)
            {
                cooldownText.text = "";
                cooldownText.enabled = false;
            }
        }
    }
}