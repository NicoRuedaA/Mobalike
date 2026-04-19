using UnityEngine;
using UnityEngine.UI;
using MobaGameplay.Combat;
using MobaGameplay.Core;

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
        [Tooltip("RangedCombat component to listen to. Auto-finds from Player if not set.")]
        [SerializeField] private RangedCombat rangedCombat;
        
        // State
        private bool isInitialized = false;
        private HeroEntity heroEntity;
        
        private void Awake()
        {
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
            
            // Try to find Player's HeroEntity
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                heroEntity = player.GetComponent<HeroEntity>();
            }
            
            isInitialized = true;
        }
        
        private void Start()
        {
            if (!isInitialized) return;
            
            // Try to find RangedCombat from HeroEntity
            if (rangedCombat == null && heroEntity != null)
            {
                if (heroEntity.Combat is RangedCombat rc)
                {
                    rangedCombat = rc;
                }
            }
            
            // If still null, try to find any RangedCombat in scene
            if (rangedCombat == null)
            {
                rangedCombat = FindObjectOfType<RangedCombat>();
            }
            
            // If still null, wait for HeroEntity to be ready
            if (rangedCombat == null && heroEntity != null)
            {
                Debug.Log("[AmmoUI] Waiting for HeroEntity to initialize combat...");
                Invoke(nameof(TryInitialize), 0.5f);
                return;
            }
            
            if (rangedCombat == null)
            {
                Debug.LogWarning("[AmmoUI] No RangedCombat found. UI will not update.");
                return;
            }
            
            InitializeSubscriptions();
        }
        
        private void TryInitialize()
        {
            if (heroEntity != null && heroEntity.Combat is RangedCombat rc)
            {
                rangedCombat = rc;
                InitializeSubscriptions();
            }
            else
            {
                // Retry in another 0.5s
                Invoke(nameof(TryInitialize), 0.5f);
            }
        }
        
        private void InitializeSubscriptions()
        {
            if (rangedCombat == null) return;
            
            // Subscribe to events with explicit lambda wrappers to avoid ambiguity
            rangedCombat.OnAmmoChanged += UpdateAmmoUI;
            rangedCombat.OnReloadStart += ShowReloadIndicator;
            rangedCombat.OnReloadComplete += (current, max) => HideReloadIndicatorAfterReload(current, max);
            rangedCombat.OnReloadCancelled += () => HideReloadIndicatorAfterCancel();
            
            // Initial update
            if (rangedCombat.HasAmmoSystem)
            {
                UpdateAmmoUI(rangedCombat.CurrentAmmo, rangedCombat.MaxAmmo);
                Debug.Log($"[AmmoUI] Initialized with ammo: {rangedCombat.CurrentAmmo}/{rangedCombat.MaxAmmo}");
            }
            else
            {
                ammoText.text = "∞";
                Debug.Log("[AmmoUI] Initialized with infinite ammo");
            }
            
            // Hide reload indicator by default
            if (reloadIndicator != null)
            {
                reloadIndicator.SetActive(false);
            }
        }
        
        private void OnDestroy()
        {
            CancelInvoke(nameof(TryInitialize));
            
            if (rangedCombat != null)
            {
                rangedCombat.OnAmmoChanged -= UpdateAmmoUI;
                rangedCombat.OnReloadStart -= ShowReloadIndicator;
                // Note: C# allows unsubscribing without specifying which overload
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
        /// Hide reload indicator after reload completes (wrapper to avoid event ambiguity).
        /// </summary>
        private void HideReloadIndicatorAfterReload(int current, int max)
        {
            if (reloadIndicator != null)
            {
                reloadIndicator.SetActive(false);
            }
            
            // Refresh ammo text with provided values
            UpdateAmmoUI(current, max);
            Debug.Log($"[AmmoUI] Reload complete - displaying: {current}/{max}");
        }
        
        /// <summary>
        /// Hide reload indicator after reload is cancelled (wrapper to avoid event ambiguity).
        /// </summary>
        private void HideReloadIndicatorAfterCancel()
        {
            if (reloadIndicator != null)
            {
                reloadIndicator.SetActive(false);
            }
            
            // Refresh ammo text from combat directly
            if (rangedCombat != null && ammoText != null)
            {
                UpdateAmmoUI(rangedCombat.CurrentAmmo, rangedCombat.MaxAmmo);
                Debug.Log($"[AmmoUI] Reload cancelled - displaying: {rangedCombat.CurrentAmmo}/{rangedCombat.MaxAmmo}");
            }
        }
    }
}
