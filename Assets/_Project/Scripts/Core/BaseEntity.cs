using UnityEngine;
using MobaGameplay.Abilities;
using MobaGameplay.Combat;
using System;

namespace MobaGameplay.Core
{
    public abstract class BaseEntity : MonoBehaviour
    {
        [Header("Entity Stats")]
        public float MaxHealth = 1000f;
        public float CurrentHealth = 1000f;
        public float MaxMana = 500f;
        
        [SerializeField]
        private float currentMana = 500f;
        private bool manaInitialized = false;
        public float CurrentMana
        {
            get => currentMana;
            set
            {
                float oldMana = currentMana;
                currentMana = Mathf.Clamp(value, 0f, MaxMana);
                if (manaInitialized && Mathf.Abs(oldMana - currentMana) > 0.01f)
                {
                    OnManaChanged?.Invoke(oldMana, currentMana);
                }
            }
        }

        [Header("Combat Stats")]
        public float AttackDamage = 50f;
        public float AbilityPower = 0f;
        public float AttackSpeed = 1.5f; // Attacks per second
        public float PhysicalArmor = 30f;
        public float MagicResistance = 30f;
        public float MovementSpeed = 5f;

        [Header("Mana Regen")]
        [SerializeField] private float manaRegen = 5f; // Mana per second

        [Header("Critical Hit")]
        [SerializeField, Range(0f, 1f)] private float criticalChance = 0.15f;
        [SerializeField] private float criticalMultiplier = 1.5f;

        public bool IsDead => CurrentHealth <= 0;

        public event Action<DamageInfo> OnTakeDamage;
        public event Action<BaseEntity> OnDeath;
        public event Action<float, float> OnManaChanged; // (previousMana, currentMana)

        public BaseMovement Movement { get; private set; }
        public BaseCombat Combat { get; private set; }
        public AbilityController Abilities { get; private set; }

        protected virtual void Awake()
        {
            Movement = GetComponent<BaseMovement>();
            Combat = GetComponent<BaseCombat>();
            Abilities = GetComponent<AbilityController>();

            CurrentHealth = MaxHealth;
            CurrentMana = MaxMana;
        }

        private void Update()
        {
            // Mana regeneration
            if (manaRegen > 0f && CurrentMana < MaxMana)
            {
                CurrentMana = Mathf.Min(MaxMana, CurrentMana + manaRegen * Time.deltaTime);
            }

            // Mark as initialized after first frame to enable event firing
            if (!manaInitialized) manaInitialized = true;
        }

        public virtual void TakeDamage(DamageInfo damageInfo)
        {
            if (IsDead) return;

            float actualDamage = CalculateDamageReduction(damageInfo);
            CurrentHealth -= actualDamage;

            // Check critical hit
            bool isCritical = damageInfo.IsCritical || (damageInfo.Source != null && UnityEngine.Random.value < damageInfo.Source.criticalChance);
            if (isCritical)
            {
                actualDamage *= criticalMultiplier;
                CurrentHealth -= actualDamage * (criticalMultiplier - 1f);
            }

            Debug.Log($"[{gameObject.name}] took {actualDamage:F1} {damageInfo.Type} damage from {(damageInfo.Source != null ? damageInfo.Source.gameObject.name : "Unknown")}.{(isCritical ? " CRITICAL!" : "")} Health left: {CurrentHealth:F1}");

            // Spawn floating damage text
            if (UI.FloatingTextManager.Instance != null)
            {
                UI.FloatingTextManager.Instance.Spawn(transform.position + Vector3.up * 2f, actualDamage, damageInfo.Type, isCritical);
            }

            OnTakeDamage?.Invoke(damageInfo);

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        protected virtual float CalculateDamageReduction(DamageInfo info)
        {
            float reductionMultiplier = 1f;

            // Simplified MOBA armor formula: damage multiplier = 100 / (100 + armor)
            if (info.Type == DamageType.Physical)
            {
                reductionMultiplier = 100f / (100f + Mathf.Max(0, PhysicalArmor));
            }
            else if (info.Type == DamageType.Magical)
            {
                reductionMultiplier = 100f / (100f + Mathf.Max(0, MagicResistance));
            }
            // True damage is unmitigated

            return info.Amount * reductionMultiplier;
        }

        protected virtual void Die()
        {
            CurrentHealth = 0;
            Debug.Log($"[{gameObject.name}] died!");
            OnDeath?.Invoke(this);

            // 1. Disable all colliders (Hurtboxes) to avoid further hits
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders) 
            {
                col.enabled = false;
            }

            // 2. Stop and disable Movement, Combat, and Abilities
            if (Movement != null) 
            {
                Movement.Stop();
                if (Movement is MonoBehaviour monoMovement) monoMovement.enabled = false;
            }
            if (Combat != null) Combat.enabled = false;
            if (Abilities != null) Abilities.enabled = false;

            // 3. Hide floating UI
            var floatingUI = GetComponentInChildren<UI.FloatingStatusBar>();
            if (floatingUI != null) 
            {
                floatingUI.gameObject.SetActive(false);
            }

            // 4. Destroy after a delay (e.g. for death animation)
            Destroy(gameObject, 3f);
        }
    }
}
