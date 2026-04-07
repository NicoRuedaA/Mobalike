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

        // Events
        public event Action<DamageInfo> OnTakeDamage;
        public event Action<BaseEntity> OnDeath;
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

        // Private
        private bool manaInitialized = false;

        protected virtual void Awake()
        {
            Movement = GetComponent<BaseMovement>();
            Combat = GetComponent<BaseCombat>();
            Abilities = GetComponent<AbilityController>();
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
            // Mana regeneration
            if (manaRegen > 0f && currentMana < MaxMana)
            {
                currentMana = Mathf.Min(MaxMana, currentMana + manaRegen * Time.deltaTime);
            }
        }

        public virtual void TakeDamage(DamageInfo damageInfo)
        {
            if (IsDead) return;

            float actualDamage = CalculateDamageReduction(damageInfo);
            currentHealth -= actualDamage;

            // Check critical hit
            bool isCritical = damageInfo.IsCritical || 
                (damageInfo.Source != null && UnityEngine.Random.value < damageInfo.Source.criticalChance);
            
            if (isCritical)
            {
                float bonusDamage = actualDamage * (criticalMultiplier - 1f);
                currentHealth -= bonusDamage;
                actualDamage *= criticalMultiplier;
            }

            // Debug logging (strip in production)
            #if UNITY_EDITOR
            Debug.Log($"[{gameObject.name}] took {actualDamage:F1} {damageInfo.Type} damage " +
                     $"from {(damageInfo.Source?.gameObject.name ?? "Unknown")}. " +
                     $"{(isCritical ? "CRITICAL! " : "")}Health left: {currentHealth:F1}");
            #endif

            // Spawn floating damage text
            UI.FloatingTextManager.Instance?.Spawn(
                transform.position + Vector3.up * 2f, 
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
                    reduction = 100f / (100f + Mathf.Max(0, physicalArmor));
                    break;
                case DamageType.Magical:
                    reduction = 100f / (100f + Mathf.Max(0, magicResistance));
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
            
            OnDeath?.Invoke(this);

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
    }
}
