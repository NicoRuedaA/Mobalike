using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MobaGameplay.Abilities;

namespace MobaGameplay.UI
{
    /// <summary>
    /// UI slot for displaying an ability icon with cooldown overlay.
    /// Supports both old BaseAbility (MonoBehaviour) and new AbilityData (data-driven).
    /// </summary>
    public class AbilitySlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay; // Image Type = Filled, Radial 360
        [SerializeField] private TextMeshProUGUI cooldownText;

        private BaseAbility legacyAbility;
        private AbilityData newAbilityData;
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

        // Legacy API (old system)
        public BaseAbility GetAbility() => legacyAbility;

        public void AssignAbility(BaseAbility newAbility)
        {
            legacyAbility = newAbility;
            newAbilityData = null;
            abilitySystem = null;
            slotIndex = -1;
            UpdateDisplay();
        }

        // New API (data-driven system)
        public void AssignAbility(AbilityData data, AbilitySystem system, int index)
        {
            legacyAbility = null;
            newAbilityData = data;
            abilitySystem = system;
            slotIndex = index;
            
            UpdateDisplay();
            
            // Force first update immediately so cooldown visualization works
            UpdateNewAbility();
        }

        private void UpdateDisplay()
        {
            string name = "";
            Sprite icon = null;
            bool hasAbility = false;

            if (legacyAbility != null)
            {
                name = legacyAbility.abilityName;
                icon = legacyAbility.AbilityIcon;
                hasAbility = true;
            }
            else if (newAbilityData != null)
            {
                name = newAbilityData.abilityName;
                icon = newAbilityData.icon;
                hasAbility = true;
            }

            if (hasAbility && iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = icon != null;
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
            if (legacyAbility != null)
            {
                UpdateLegacyAbility();
            }
            else if (abilitySystem != null && slotIndex >= 0 && newAbilityData != null)
            {
                UpdateNewAbility();
            }
        }

        private void UpdateLegacyAbility()
        {
            if (legacyAbility == null) return;

            bool onCd = legacyAbility.IsOnCooldown;
            
            if (cooldownOverlay != null)
            {
                cooldownOverlay.enabled = onCd || legacyAbility.MaxCooldown > 0;
                if (onCd && legacyAbility.MaxCooldown > 0)
                    cooldownOverlay.fillAmount = legacyAbility.CurrentCooldown / legacyAbility.MaxCooldown;
                else
                    cooldownOverlay.fillAmount = 0;
            }
                
            if (cooldownText != null)
            {
                if (onCd)
                {
                    cooldownText.text = legacyAbility.CurrentCooldown.ToString("F1");
                    cooldownText.enabled = true;
                }
                else
                {
                    cooldownText.text = "";
                    cooldownText.enabled = false;
                }
            }
        }

        private void UpdateNewAbility()
        {
            if (abilitySystem == null || newAbilityData == null || slotIndex < 0) return;

            var instance = abilitySystem.GetAbilityInstance(slotIndex);
            if (instance == null) return;

            bool onCd = instance.IsOnCooldown;
            
            if (cooldownOverlay != null)
            {
                cooldownOverlay.enabled = true;
                if (onCd && newAbilityData.cooldown > 0)
                    cooldownOverlay.fillAmount = instance.CurrentCooldown / newAbilityData.cooldown;
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