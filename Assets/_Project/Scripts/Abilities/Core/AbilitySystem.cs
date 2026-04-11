using System.Collections.Generic;
using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.UI.Targeting;

namespace MobaGameplay.Abilities
{
    /// <summary>
    /// Data-driven ability system. Replaces the old AbilityController.
    /// 
    /// Key differences from AbilityController:
    /// - Abilities are defined as AbilityData ScriptableObjects (no MonoBehaviour per ability)
    /// - The player has ONE component (this) instead of N+1
    /// - Adding a new ability = create an AbilityData SO, no C# code needed
    /// - Slot count is configurable (default 4, expandable)
    /// - Behaviors are stateless IAbilityBehavior implementations (factory pattern)
    /// 
    /// IMPORTANT: 
    /// - HeroClass is obtained from HeroEntity component (single source of truth)
    /// - Key bindings are handled by PlayerInputController, not here
    /// - No manual ability configuration - use HeroClass instead
    /// </summary>
    [RequireComponent(typeof(BaseEntity))]
    public class AbilitySystem : MonoBehaviour
    {
        // ============================================================
        // Runtime State (obtained from HeroEntity)
        // ============================================================

        private BaseEntity owner;
        private HeroClass heroClass;
        private List<AbilityInstance> instances = new List<AbilityInstance>();
        private int activeTargetingIndex = -1;

        // ============================================================
        // Public API
        // ============================================================

        public int SlotCount => instances.Count;

        /// <summary>Currently targeting ability index (-1 = none)</summary>
        public int ActiveTargetingIndex => activeTargetingIndex;

        /// <summary>Is an ability currently being targeted?</summary>
        public bool HasActiveTargeting => activeTargetingIndex >= 0 && activeTargetingIndex < instances.Count;

        // Events (same API shape as AbilityController for easy migration)
        public event System.Action<int> OnAbilityStarted;
        public event System.Action<int> OnAbilityExecuted;
        public event System.Action<int> OnAbilityCancelled;

        // ============================================================
        // Unity Lifecycle
        // ============================================================

        // Cached ability controller for fallback
        private AbilityController cachedAbilityController;

        private void Awake()
        {
            owner = GetComponent<BaseEntity>();
            
            // Get HeroClass from HeroEntity component (single source of truth)
            var heroEntity = GetComponent<HeroEntity>();
            if (heroEntity != null)
            {
                heroClass = heroEntity.HeroClass;
            }

            // Try to get old AbilityController for fallback
            cachedAbilityController = GetComponent<AbilityController>();
            
            InitializeInstances();
        }

        private void Update()
        {
            // Tick cooldowns
            for (int i = 0; i < instances.Count; i++)
            {
                if (instances[i] != null)
                    instances[i].TickCooldown(Time.deltaTime);
            }
        }

        // ============================================================
        // Initialization
        // ============================================================

        private void InitializeInstances()
        {
            instances.Clear();

            // Get HeroClass from HeroEntity (single source of truth)
            var heroEntity = owner as HeroEntity;
            if (heroEntity != null && heroEntity.HeroClass != null)
            {
                heroClass = heroEntity.HeroClass;
            }

            // Initialize abilities from HeroClass
            if (heroClass != null && heroClass.abilities != null)
            {
                foreach (var data in heroClass.abilities)
                {
                    instances.Add(data != null ? new AbilityInstance(data, owner) : null);
                }
            }
            else
            {
                // Initialize with 4 empty slots
                for (int i = 0; i < 4; i++)
                {
                    instances.Add(null);
                }
            }
        }

        /// <summary>
        /// Cambia la clase del héroe en runtime (y por ende las habilidades).
        /// </summary>
        public void SetHeroClass(HeroClass newClass)
        {
            heroClass = newClass;
            InitializeInstances();
        }

        /// <summary>
        /// Recarga las habilidades desde HeroEntity (útil tras cambio de clase).
        /// </summary>
        public void RefreshFromHeroEntity()
        {
            InitializeInstances();
        }

        // ============================================================
        // Public Methods (same API shape as AbilityController)
        // ============================================================

        /// <summary>Get ability data for a slot index (0-based)</summary>
        public AbilityData GetAbilityData(int index)
        {
            if (index < 0 || index >= instances.Count) return null;
            return instances[index]?.Data;
        }

        /// <summary>Get ability instance for a slot index (0-based)</summary>
        public AbilityInstance GetAbilityInstance(int index)
        {
            if (index < 0 || index >= instances.Count) return null;
            return instances[index];
        }

        /// <summary>Start targeting for an ability slot (0-based index)</summary>
        public void TryStartTargeting(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= instances.Count) return;
            var instance = instances[slotIndex];
            if (instance == null) return;

            if (owner?.IsDead == true) return;
            if (!instance.CanCast()) return;

            // Cancel existing targeting
            if (activeTargetingIndex >= 0 && activeTargetingIndex != slotIndex)
            {
                CancelTargeting();
            }

            activeTargetingIndex = slotIndex;

            // Show targeting indicator
            if (TargetingManager.Instance != null)
            {
                TargetingManager.Instance.StartTargetingForData(instance.Data);
            }

            OnAbilityStarted?.Invoke(slotIndex);
        }

        /// <summary>Cancel current targeting mode</summary>
        public void CancelTargeting()
        {
            if (activeTargetingIndex >= 0 && activeTargetingIndex < instances.Count)
            {
                int cancelledIndex = activeTargetingIndex;

                if (TargetingManager.Instance != null)
                {
                    TargetingManager.Instance.CancelTargeting();
                }

                activeTargetingIndex = -1;
                OnAbilityCancelled?.Invoke(cancelledIndex);
            }
        }

        /// <summary>Execute the currently targeted ability at the given position/entity.</summary>
        public void ExecuteTargeting(Vector3 targetPosition, BaseEntity targetEntity)
        {
            if (activeTargetingIndex < 0 || activeTargetingIndex >= instances.Count) return;

            var instance = instances[activeTargetingIndex];
            if (instance == null) return;

            int executedIndex = activeTargetingIndex;

            // Re-validate cast conditions (mana or cooldown may have changed since targeting started)
            if (!instance.CanCast())
            {
                #if UNITY_EDITOR
                Debug.Log($"[AbilitySystem] Cannot cast '{instance.Data.abilityName}' — not enough mana or on cooldown");
                #endif
                CancelTargeting();
                return;
            }

            // Cleanup targeting UI and clear state BEFORE execution
            if (TargetingManager.Instance != null)
            {
                TargetingManager.Instance.CancelTargeting();
            }
            activeTargetingIndex = -1;

            // Consume mana and start cooldown
            instance.ConsumeMana();
            instance.StartCooldown();

            // Execute the behavior
            var behavior = AbilityBehaviorFactory.GetBehavior(instance.Data.behaviorType);
            if (behavior != null)
            {
                var context = AbilityContext.Create(owner, targetPosition, targetEntity, instance.Data);
                behavior.Execute(context);
            }
            else
            {
                Debug.LogError($"[AbilitySystem] No behavior found for type '{instance.Data.behaviorType}'");
            }

            instance.NotifyExecuted();
            OnAbilityExecuted?.Invoke(executedIndex);
        }

        /// <summary>Quick-cast ability at a position (no targeting UI)</summary>
        public void QuickCast(int slotIndex, Vector3 targetPosition, BaseEntity targetEntity = null)
        {
            if (slotIndex < 0 || slotIndex >= instances.Count) return;
            var instance = instances[slotIndex];
            if (instance == null || !instance.CanCast()) return;

            // Consume mana and start cooldown
            if (!instance.ConsumeMana()) return;
            instance.StartCooldown();

            // Execute
            var behavior = AbilityBehaviorFactory.GetBehavior(instance.Data.behaviorType);
            if (behavior != null)
            {
                var context = AbilityContext.Create(owner, targetPosition, targetEntity, instance.Data);
                behavior.Execute(context);
            }

            instance.NotifyExecuted();
            OnAbilityExecuted?.Invoke(slotIndex);
        }

        // ============================================================
        // AbilityController compatibility methods
        // (These match AbilityController's API for migration)
        // ============================================================

        /// <summary>Start targeting ability slot 1 (1-based = Alpha1)</summary>
        public void TryStartTargetingAbility1() => TryStartTargeting(0);
        public void TryStartTargetingAbility2() => TryStartTargeting(1);
        public void TryStartTargetingAbility3() => TryStartTargeting(2);
        public void TryStartTargetingAbility4() => TryStartTargeting(3);

        /// <summary>Check if ability at slot is on cooldown</summary>
        public bool IsOnCooldown(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= instances.Count) return false;
            return instances[slotIndex]?.IsOnCooldown ?? false;
        }

        /// <summary>Get cooldown remaining as percentage (0-1)</summary>
        public float GetCooldownPercent(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= instances.Count) return 0f;
            return instances[slotIndex]?.CooldownPercent ?? 0f;
        }

        /// <summary>Check if any ability is on cooldown</summary>
        public bool IsAnyOnCooldown()
        {
            foreach (var instance in instances)
            {
                if (instance != null && instance.IsOnCooldown) return true;
            }
            return false;
        }

        /// <summary>Get total cooldown percentage across all abilities</summary>
        public float GetTotalCooldownPercent()
        {
            float total = 0f;
            int count = 0;

            foreach (var instance in instances)
            {
                if (instance != null)
                {
                    total += instance.CooldownPercent;
                    count++;
                }
            }

            return count > 0 ? total / count : 0f;
        }
    }
}