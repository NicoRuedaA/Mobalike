using UnityEngine;
using System;
using System.Collections.Generic;
using MobaGameplay.Core;

namespace MobaGameplay.Abilities
{
    /// <summary>
    /// Manages ability slots and execution for an entity.
    /// Handles quick-cast mechanics and ability state.
    /// </summary>
    [RequireComponent(typeof(BaseEntity))]
    public class AbilityController : MonoBehaviour
    {
        // Constants
        private const int MAX_ABILITIES = 3;

        // Serializable Fields
        [Header("Equipped Abilities")]
        [SerializeField] private BaseAbility ability1;
        [SerializeField] private BaseAbility ability2;
        [SerializeField] private BaseAbility ability3;

        // State
        private BaseEntity entity;
        private BaseAbility activeTargetingAbility;
        private Dictionary<KeyCode, BaseAbility> keyBindings = new Dictionary<KeyCode, BaseAbility>();

        // Properties
        public BaseAbility ActiveTargetingAbility => activeTargetingAbility;
        public BaseAbility Ability1 => ability1;
        public BaseAbility Ability2 => ability2;
        public BaseAbility Ability3 => ability3;
        public bool HasActiveTargeting => activeTargetingAbility != null;

        // Events
        public event Action<BaseAbility> OnAbilityStarted;
        public event Action<BaseAbility> OnAbilityExecuted;
        public event Action<BaseAbility> OnAbilityCancelled;

        private void Awake()
        {
            entity = GetComponent<BaseEntity>();
            SetupKeyBindings();
        }

        private void Start()
        {
            InitializeAbilities();
        }

        private void SetupKeyBindings()
        {
            keyBindings.Clear();
            if (ability1 != null) keyBindings[KeyCode.Alpha1] = ability1;
            if (ability2 != null) keyBindings[KeyCode.Alpha2] = ability2;
            if (ability3 != null) keyBindings[KeyCode.Alpha3] = ability3;
        }

        private void InitializeAbilities()
        {
            if (ability1 != null) ability1.Initialize(entity);
            if (ability2 != null) ability2.Initialize(entity);
            if (ability3 != null) ability3.Initialize(entity);
        }

        public void TryStartTargetingAbility1() => StartTargeting(ability1);
        public void TryStartTargetingAbility2() => StartTargeting(ability2);
        public void TryStartTargetingAbility3() => StartTargeting(ability3);

        public bool TryStartTargetingByKey(KeyCode key)
        {
            if (keyBindings.TryGetValue(key, out BaseAbility ability))
            {
                StartTargeting(ability);
                return true;
            }
            return false;
        }

        private void StartTargeting(BaseAbility ability)
        {
            if (ability == null || entity?.IsDead == true) return;

            // Check mana
            if (!ability.HasEnoughMana)
            {
                #if UNITY_EDITOR
                Debug.Log($"[Ability] {ability.abilityName} - Not enough mana! Need {ability.manaCost:F0}");
                #endif
                return;
            }

            // Check cooldown
            if (!ability.CanCast())
            {
                #if UNITY_EDITOR
                Debug.Log($"[Ability] {ability.abilityName} is on cooldown!");
                #endif
                return;
            }

            // Cancel existing targeting
            if (activeTargetingAbility != null && activeTargetingAbility != ability)
            {
                CancelTargeting();
            }

            // Start new targeting
            activeTargetingAbility = ability;
            ability.BeginTargeting();
            OnAbilityStarted?.Invoke(ability);
        }

        /// <summary>
        /// Cancel current targeting mode.
        /// </summary>
        public void CancelTargeting()
        {
            if (activeTargetingAbility != null)
            {
                activeTargetingAbility.CancelTargeting();
                OnAbilityCancelled?.Invoke(activeTargetingAbility);
                activeTargetingAbility = null;
            }
        }

        /// <summary>
        /// Execute the active ability at target position/entity.
        /// </summary>
        public void ExecuteTargeting(Vector3 targetPosition, BaseEntity targetEntity)
        {
            if (activeTargetingAbility == null) return;

            BaseAbility ability = activeTargetingAbility;
            
            // Force UI cleanup BEFORE execution
            ability.CancelTargeting();
            
            // Execute
            ability.ExecuteCast(targetPosition, targetEntity);
            
            // Clear state
            activeTargetingAbility = null;
            
            OnAbilityExecuted?.Invoke(ability);
        }

        /// <summary>
        /// Get ability by index (1-3).
        /// </summary>
        public BaseAbility GetAbility(int index)
        {
            return index switch
            {
                1 => ability1,
                2 => ability2,
                3 => ability3,
                _ => null
            };
        }

        /// <summary>
        /// Check if any ability is currently on cooldown.
        /// </summary>
        public bool IsAnyOnCooldown()
        {
            return (ability1?.IsOnCooldown ?? false) ||
                   (ability2?.IsOnCooldown ?? false) ||
                   (ability3?.IsOnCooldown ?? false);
        }

        /// <summary>
        /// Get total cooldown percentage across all abilities.
        /// </summary>
        public float GetTotalCooldownPercent()
        {
            float total = 0f;
            int count = 0;
            
            if (ability1 != null) { total += ability1.CooldownPercent; count++; }
            if (ability2 != null) { total += ability2.CooldownPercent; count++; }
            if (ability3 != null) { total += ability3.CooldownPercent; count++; }
            
            return count > 0 ? total / count : 0f;
        }
    }
}
