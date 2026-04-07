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
        public float CurrentMana = 500f;

        [Header("Combat Stats")]
        public float AttackDamage = 50f;
        public float AbilityPower = 0f;
        public float AttackSpeed = 1.5f; // Attacks per second
        public float PhysicalArmor = 30f;
        public float MagicResistance = 30f;
        public float MovementSpeed = 5f;

        public bool IsDead => CurrentHealth <= 0;

        public event Action<DamageInfo> OnTakeDamage;
        public event Action<BaseEntity> OnDeath;

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

        public virtual void TakeDamage(DamageInfo damageInfo)
        {
            if (IsDead) return;

            float actualDamage = CalculateDamageReduction(damageInfo);
            CurrentHealth -= actualDamage;

            Debug.Log($"[{gameObject.name}] took {actualDamage:F1} {damageInfo.Type} damage from {(damageInfo.Source != null ? damageInfo.Source.gameObject.name : "Unknown")}. Health left: {CurrentHealth:F1}");

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

            // Basic death behavior, disable components or play animation
            // Disable collider and movement
            if (TryGetComponent(out Collider col)) col.enabled = false;
            if (Movement != null) Movement.Stop();
        }
    }
}
