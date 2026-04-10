using UnityEngine;
using MobaGameplay.Abilities;
using MobaGameplay.Combat;
using System;

namespace MobaGameplay.Core
{
    /// <summary>
    /// Base class for all entities (players, enemies, NPCs).
    /// Manages health, mana, combat stats, and lifecycle (death).
    /// </summary>
    public abstract class BaseEntity : MonoBehaviour
    {
        // Constants
        private const float MIN_MANA_CHANGE_THRESHOLD = 0.01f;
        private const float DEFAULT_ARMOR = 30f;
        private const float DEFAULT_MAGIC_RESIST = 30f;
        private const float DEFAULT_ATTACK_SPEED = 1.5f;
        private const float DEFAULT_ATTACK_DAMAGE = 50f;
        private const float DEFAULT_MAX_HEALTH = 1000f;
        private const float DEFAULT_MAX_MANA = 500f;
        private const float DEFAULT_MANA_REGEN = 5f;
        private const float DEFAULT_CRIT_CHANCE = 0.15f;
        private const float DEFAULT_CRIT_MULT = 1.5f;
        private const float DEATH_DESTROY_DELAY = 3f;

        // Damage formula constants
        private const float ARMOR_SCALING_BASE = 100f;    // 100 / (100 + armor) reduction formula

        // Visual offsets
        [Header("Visual Settings")]
        [SerializeField] private float floatingTextOffsetY = 2f;

        // Events
        public event Action<DamageInfo> OnTakeDamage;
        public event Action<BaseEntity, DamageInfo> OnDeath;
        public event Action<float, float> OnManaChanged;

        // Health
        [Header("Entity Stats")]
        [SerializeField] private float maxHealth = DEFAULT_MAX_HEALTH;
        [SerializeField] private float currentHealth = DEFAULT_MAX_HEALTH;
        
        // Mana
        [Header("Mana")]
        [SerializeField] private float maxMana = DEFAULT_MAX_MANA;
        [SerializeField] private float currentMana = DEFAULT_MAX_MANA;
        [SerializeField] private float manaRegen = DEFAULT_MANA_REGEN;
        
        // Combat Stats
        [Header("Combat Stats")]
        [SerializeField] private float baseAttackDamage = DEFAULT_ATTACK_DAMAGE;
        [SerializeField] private float baseAbilityPower = 0f;
        [SerializeField] private float attackSpeed = DEFAULT_ATTACK_SPEED;
        [SerializeField] private float physicalArmor = DEFAULT_ARMOR;
        [SerializeField] private float magicResistance = DEFAULT_MAGIC_RESIST;
        
        // Critical Hit
        [Header("Critical Hit")]
        [SerializeField, Range(0f, 1f)] private float criticalChance = DEFAULT_CRIT_CHANCE;
        [SerializeField] private float criticalMultiplier = DEFAULT_CRIT_MULT;

        // Properties
        public float MaxHealth
        {
            get => maxHealth;
            set => maxHealth = Mathf.Max(0, value);
        }

        public float CurrentHealth
        {
            get => currentHealth;
            set => currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        }

        public float MaxMana
        {
            get => maxMana;
            set => maxMana = Mathf.Max(0, value);
        }

        public float CurrentMana
        {
            get => currentMana;
            set
            {
                float oldMana = currentMana;
                currentMana = Mathf.Clamp(value, 0f, MaxMana);
                
                if (manaInitialized && Mathf.Abs(oldMana - currentMana) > MIN_MANA_CHANGE_THRESHOLD)
                {
                    OnManaChanged?.Invoke(oldMana, currentMana);
                }
            }
        }

        public float AttackDamage
        {
            get => baseAttackDamage;
            set => baseAttackDamage = Mathf.Max(0f, value);
        }
        
        public float AbilityPower
        {
            get => baseAbilityPower;
            set => baseAbilityPower = Mathf.Max(0f, value);
        }
        
        public float AttackSpeed => attackSpeed > 0 ? attackSpeed : 1f;
        
        public float PhysicalArmor
        {
            get => physicalArmor;
            set => physicalArmor = Mathf.Max(0f, value);
        }
        
        public float MagicResistance
        {
            get => magicResistance;
            set => magicResistance = Mathf.Max(0f, value);
        }
        
        public float CriticalChance => criticalChance;
        public float CriticalMultiplier => criticalMultiplier;

        public bool IsDead => currentHealth <= 0f;

        // Component References
        public BaseMovement Movement { get; private set; }
        public BaseCombat Combat { get; private set; }
        public AbilityController Abilities { get; private set; }

        /// <summary>
        /// Data-driven ability system (new). Takes priority over AbilityController when present.
        /// Added during migration — will replace Abilities entirely once complete.
        /// </summary>
        public AbilitySystem AbilitySystem { get; private set; }

        #if UNITY_EDITOR
        /// <summary>
        /// Debug: Muestra el estado actual de todos los stats en la consola.
        /// Útil para verificar que el equipamiento modifica correctamente los valores.
        /// </summary>
        [ContextMenu("Debug: Show Current Stats")]
        public void DebugShowStats()
        {
            Debug.Log($"=== {gameObject.name} Stats ===\n" +
                     $"Health: {currentHealth:F0} / {maxHealth:F0}\n" +
                     $"Mana: {currentMana:F0} / {maxMana:F0}\n" +
                     $"Attack Damage: {baseAttackDamage:F1}\n" +
                     $"Ability Power: {baseAbilityPower:F1}\n" +
                     $"Attack Speed: {attackSpeed:F2}\n" +
                     $"Armor: {physicalArmor:F1}\n" +
                     $"Magic Resist: {magicResistance:F1}\n" +
                     $"Crit Chance: {criticalChance:P0}\n" +
                     $"Crit Multiplier: {criticalMultiplier:F2}x");
        }
        #endif

        // Private
        private bool manaInitialized = false;

        protected virtual void Awake()
        {
            Movement = GetComponent<BaseMovement>();
            Combat = GetComponent<BaseCombat>();
            Abilities = GetComponent<AbilityController>();
            AbilitySystem = GetComponent<AbilitySystem>();
        }

        protected virtual void Start()
        {
            // Initialize values
            currentHealth = maxHealth;
            currentMana = maxMana;
            manaInitialized = true;
        }

        private void Update()
        {
            // Mana regeneration — use property setter to fire OnManaChanged event
            if (manaRegen > 0f && CurrentMana < MaxMana)
            {
                CurrentMana = Mathf.Min(MaxMana, CurrentMana + manaRegen * Time.deltaTime);
            }
        }

        public virtual void TakeDamage(DamageInfo damageInfo)
        {
            if (IsDead) return;

            float actualDamage = CalculateDamageReduction(damageInfo);

            // Check critical hit — apply multiplier BEFORE subtracting health
            bool isCritical = damageInfo.IsCritical || 
                (damageInfo.Source != null && UnityEngine.Random.value < damageInfo.Source.criticalChance);
            
            if (isCritical)
            {
                actualDamage *= criticalMultiplier;
            }

            currentHealth -= actualDamage;

            // Debug logging (strip in production)
            #if UNITY_EDITOR
            Debug.Log($"[{gameObject.name}] took {actualDamage:F1} {damageInfo.Type} damage " +
                     $"from {(damageInfo.Source?.gameObject.name ?? "Unknown")}. " +
                     $"{(isCritical ? "CRITICAL! " : "")}Health left: {currentHealth:F1}");
            #endif

            // Spawn floating damage text
            UI.FloatingTextManager.Instance?.Spawn(
                transform.position + Vector3.up * floatingTextOffsetY, 
                actualDamage, 
                damageInfo.Type, 
                isCritical
            );

            OnTakeDamage?.Invoke(damageInfo);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        protected virtual float CalculateDamageReduction(DamageInfo info)
        {
            float reduction = 1f;

            switch (info.Type)
            {
                case DamageType.Physical:
                    reduction = ARMOR_SCALING_BASE / (ARMOR_SCALING_BASE + Mathf.Max(0, physicalArmor));
                    break;
                case DamageType.Magical:
                    reduction = ARMOR_SCALING_BASE / (ARMOR_SCALING_BASE + Mathf.Max(0, magicResistance));
                    break;
                case DamageType.TrueDamage:
                    // No reduction
                    break;
            }

            return info.Amount * reduction;
        }

        protected virtual void Die()
        {
            currentHealth = 0f;
            
            #if UNITY_EDITOR
            Debug.Log($"[{gameObject.name}] died!");
            #endif
            
            OnDeath?.Invoke(this, new DamageInfo(0, DamageType.TrueDamage, null));

            // Disable colliders
            foreach (var col in GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            // Stop and disable components
            Movement?.Stop();
            if (Movement is MonoBehaviour monoMove) monoMove.enabled = false;
            if (Combat != null) Combat.enabled = false;
            if (Abilities != null) Abilities.enabled = false;
            if (AbilitySystem != null) AbilitySystem.enabled = false;

            // Hide floating UI
            var floatingUI = GetComponentInChildren<UI.FloatingStatusBar>();
            if (floatingUI != null)
            {
                floatingUI.gameObject.SetActive(false);
            }

            // Destroy after delay
            Destroy(gameObject, DEATH_DESTROY_DELAY);
        }

        /// <summary>
        /// Heal this entity by amount.
        /// </summary>
        public void Heal(float amount)
        {
            if (IsDead) return;
            CurrentHealth += Mathf.Max(0, amount);
        }

        /// <summary>
        /// Restore mana to this entity.
        /// </summary>
        public void RestoreMana(float amount)
        {
            if (IsDead) return;
            CurrentMana += Mathf.Max(0, amount);
        }

        /// <summary>
        /// Revive this entity with full health.
        /// Used by GameStateManager for player respawn.
        /// </summary>
        public virtual void Revive()
        {
            currentHealth = maxHealth;
            currentMana = maxMana;
            
            #if UNITY_EDITOR
            Debug.Log($"[{gameObject.name}] revived!");
            #endif
            
            // Re-enable components
            if (Movement is MonoBehaviour monoMove) monoMove.enabled = true;
            if (Combat != null) Combat.enabled = true;
            if (Abilities != null) Abilities.enabled = true;
            if (AbilitySystem != null) AbilitySystem.enabled = true;
            
            // Re-enable colliders
            foreach (var col in GetComponentsInChildren<Collider>())
            {
                col.enabled = true;
            }
            
            // Show floating UI
            var floatingUI = GetComponentInChildren<UI.FloatingStatusBar>();
            if (floatingUI != null)
            {
                floatingUI.gameObject.SetActive(true);
            }
        }
    }
}
