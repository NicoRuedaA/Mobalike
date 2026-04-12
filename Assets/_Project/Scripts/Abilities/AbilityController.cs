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
        private const int MAX_ABILITIES = 4;

        // Serializable Fields
        [Header("Equipped Abilities")]
        [SerializeField] private BaseAbility ability1;
        [SerializeField] private BaseAbility ability2;
        [SerializeField] private BaseAbility ability3;
        [SerializeField] private BaseAbility ability4;

        // State
        private BaseEntity entity;
        private BaseAbility activeTargetingAbility;
        private Dictionary<KeyCode, BaseAbility> keyBindings = new Dictionary<KeyCode, BaseAbility>();

        // Properties
        public BaseAbility ActiveTargetingAbility => activeTargetingAbility;
        public BaseAbility Ability1 => ability1;
        public BaseAbility Ability2 => ability2;
        public BaseAbility Ability3 => ability3;
        public BaseAbility Ability4 => ability4;
        public bool HasAnyAbilities => ability1 != null || ability2 != null || ability3 != null || ability4 != null;
        public bool HasActiveTargeting => activeTargetingAbility != null;

        // Events
        public event Action<BaseAbility> OnAbilityStarted;
        public event Action<BaseAbility> OnAbilityExecuted;
        public event Action<BaseAbility> OnAbilityCancelled;

        private void Awake()
        {
            entity = GetComponent<BaseEntity>();
            FindAndAssignAbilities(); // Buscar abilities correctas por tipo
            InitializeAbilities();
            SetupKeyBindings();
            AutoFixAbilityIcons(); // Arreglar iconos si están null
        }

        private void FindAndAssignAbilities()
        {
            // Solo buscar abilities si las referencias son null
            // NO sobrescribir abilities ya asignadas en el prefab (preservan íconos y config)
            if (ability1 == null)
                ability1 = GetComponent<FireballAbility>();
            if (ability2 == null)
                ability2 = GetComponent<GroundSmashAbility>();
            if (ability3 == null)
                ability3 = GetComponent<DashAbility>();
            if (ability4 == null)
                ability4 = GetComponent<GroundTrailAbility>();

            Debug.Log($"[AbilityController] Found abilities: A1={ability1?.abilityName}({ability1?.GetType().Name}), A2={ability2?.abilityName}({ability2?.GetType().Name}), A3={ability3?.abilityName}({ability3?.GetType().Name}), A4={ability4?.abilityName}({ability4?.GetType().Name})");
        }

        [ContextMenu("Fix Ability Icons")]
        public void AutoFixAbilityIcons()
        {
            #if UNITY_EDITOR
            // Only run in editor - load icons from Assets folder
            string[] iconPaths = new string[]
            {
                "Assets/_Project/Art/Icons/Abilities/1.png",
                "Assets/_Project/Art/Icons/Abilities/2.png",
                "Assets/_Project/Art/Icons/Abilities/3.png",
                "Assets/_Project/Art/Icons/Abilities/4.png"
            };
            
            Sprite[] icons = new Sprite[4];
            for (int i = 0; i < 4; i++)
            {
                icons[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(iconPaths[i]);
            }
            
            FixAbilityIcon(ability1, icons[0], "FireballAbility", "Fireball");
            FixAbilityIcon(ability2, icons[1], "GroundSmashAbility", "Ground Smash");
            FixAbilityIcon(ability3, icons[2], "DashAbility", "Dash");
            FixAbilityIcon(ability4, icons[3], "GroundTrailAbility", "Ground Trail");
            
            // Mark abilities as dirty so they save
            if (ability1 != null) UnityEditor.EditorUtility.SetDirty(ability1);
            if (ability2 != null) UnityEditor.EditorUtility.SetDirty(ability2);
            if (ability3 != null) UnityEditor.EditorUtility.SetDirty(ability3);
            if (ability4 != null) UnityEditor.EditorUtility.SetDirty(ability4);
            
            Debug.Log("[AbilityController] Auto-fixed ability icons");
            #endif
        }
        
        private void FixAbilityIcon(BaseAbility ability, Sprite icon, string typeName, string displayName)
        {
            if (ability == null) return;
            
            // Use public property setters instead of reflection
            if (ability.AbilityIcon == null && icon != null)
            {
                ability.AbilityIcon = icon;
                Debug.Log($"[AbilityController] Assigned icon '{icon.name}' to {typeName}");
            }
            
            // Fix the name if it's the default "New Ability"
            if (string.IsNullOrEmpty(ability.abilityName) || ability.abilityName == "New Ability")
            {
                ability.abilityName = displayName;
            }
        }

        private void SetupKeyBindings()
        {
            keyBindings.Clear();
            if (ability1 != null) keyBindings[KeyCode.Alpha1] = ability1;
            if (ability2 != null) keyBindings[KeyCode.Alpha2] = ability2;
            if (ability3 != null) keyBindings[KeyCode.Alpha3] = ability3;
            if (ability4 != null) keyBindings[KeyCode.Alpha4] = ability4;
        }

        private void InitializeAbilities()
        {
            if (ability1 != null) ability1.Initialize(entity);
            if (ability2 != null) ability2.Initialize(entity);
            if (ability3 != null) ability3.Initialize(entity);
            if (ability4 != null) ability4.Initialize(entity);
        }

        public void TryStartTargetingAbility1() => StartTargeting(ability1);
        public void TryStartTargetingAbility2() => StartTargeting(ability2);
        public void TryStartTargetingAbility3() => StartTargeting(ability3);
        public void TryStartTargetingAbility4() => StartTargeting(ability4);

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
        /// Get ability by index (1-4).
        /// </summary>
        public BaseAbility GetAbility(int index)
        {
            return index switch
            {
                1 => ability1,
                2 => ability2,
                3 => ability3,
                4 => ability4,
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
                   (ability3?.IsOnCooldown ?? false) ||
                   (ability4?.IsOnCooldown ?? false);
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
            if (ability4 != null) { total += ability4.CooldownPercent; count++; }

            return count > 0 ? total / count : 0f;
        }
    }
}
