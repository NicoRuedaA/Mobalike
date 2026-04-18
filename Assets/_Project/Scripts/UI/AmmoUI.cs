using UnityEngine;
using UnityEngine.UI;
using MobaGameplay.Combat;

namespace MobaGameplay.UI
{
    /// <summary>
    /// UI component that displays current ammo count.
    /// Attach to a GameObject with a Text component.
    /// Automatically subscribes to RangedCombat ammo events.
    /// </summary>
    public class AmmoUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Text component to display ammo count (e.g., '6/6')")]
        [SerializeField] private Text ammoText;
        
        [Tooltip("Optional: Image/icon that shows reload indicator")]
        [SerializeField] private GameObject reloadIndicator;
        
        [Header("Combat Reference")]
        [Tooltip("RangedCombat component to listen to. Auto-finds if not set.")]
        [SerializeField] private RangedCombat rangedCombat;
        
        // State
        private bool isInitialized = false;
        
        private void Awake()
        {
            // Auto-find RangedCombat if not set
            if (rangedCombat == null)
            {
                rangedCombat = FindObjectOfType<RangedCombat>();
            }
            
            // Auto-find Text if not set
            if (ammoText == null)
            {
                ammoText = GetComponent<Text>();
            }
            
            if (ammoText == null)
            {
                Debug.LogError("[AmmoUI] No Text component found! Attach Text to this GameObject or assign in Inspector.");
                enabled = false;
                return;
            }
            
            isInitialized = true;
        }
        
        private void Start()
        {
            if (!isInitialized || rangedCombat == null) return;
            
            // Subscribe to events
            rangedCombat.OnAmmoChanged += UpdateAmmoUI;
            rangedCombat.OnReloadStart += ShowReloadIndicator;
            rangedCombat.OnReloadComplete += HideReloadIndicator;  // Uses (int, int) overload
            rangedCombat.OnReloadCancelled += HideReloadIndicator;  // Uses () overload
            
            // Initial update
            UpdateAmmoUI(rangedCombat.CurrentAmmo, rangedCombat.MaxAmmo);
            
            // Hide reload indicator by default
            if (reloadIndicator != null)
            {
                reloadIndicator.SetActive(false);
            }
        }
        
        private void OnDestroy()
        {
            if (rangedCombat != null)
            {
                rangedCombat.OnAmmoChanged -= UpdateAmmoUI;
                rangedCombat.OnReloadStart -= ShowReloadIndicator;
                // Note: C# allows unsubscribing without specifying which overload
                // The compiler will match the correct delegate type
                rangedCombat.OnReloadComplete -= HideReloadIndicator;
                rangedCombat.OnReloadCancelled -= HideReloadIndicator;
            }
        }
        
        /// <summary>
        /// Update ammo text display.
        /// </summary>
        private void UpdateAmmoUI(int current, int max)
        {
            if (ammoText == null) return;
            
            ammoText.text = $"{current}/{max}";
            
            // Color changes based on ammo state
            if (current <= 0)
            {
                ammoText.color = Color.red;  // No ammo - critical
            }
            else if (current < max * 0.5f)
            {
                ammoText.color = Color.yellow;  // Low ammo - warning
            }
            else
            {
                ammoText.color = Color.white;  // Good ammo
            }
        }
        
        /// <summary>
        /// Show reload indicator (spinning icon, text, etc.).
        /// </summary>
        private void ShowReloadIndicator()
        {
            if (reloadIndicator != null)
            {
                reloadIndicator.SetActive(true);
            }
            
            if (ammoText != null)
            {
                ammoText.text = "RELOADING...";
            }
        }
        
        /// <summary>
        /// Hide reload indicator (called when reload completes).
        /// </summary>
        private void HideReloadIndicator(int current, int max)
        {
            if (reloadIndicator != null)
            {
                reloadIndicator.SetActive(false);
            }
            
            // Refresh ammo text with provided values
            UpdateAmmoUI(current, max);
        }
        
        /// <summary>
        /// Hide reload indicator (called when reload is cancelled).
        /// </summary>
        private void HideReloadIndicator()
        {
            if (reloadIndicator != null)
            {
                reloadIndicator.SetActive(false);
            }
            
            // Refresh ammo text
            if (rangedCombat != null && ammoText != null)
            {
                UpdateAmmoUI(rangedCombat.CurrentAmmo, rangedCombat.MaxAmmo);
            }
        }
    }
}
