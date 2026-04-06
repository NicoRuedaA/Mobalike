using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Abilities
{
    public abstract class BaseAbility : MonoBehaviour
    {
        [Header("Ability Settings")]
        public string abilityName = "New Ability";
        public float cooldown = 5f;
        public float castTime = 0f;
        public float manaCost = 0f;

        protected float currentCooldown = 0f;
        protected BaseEntity ownerEntity;

        public bool IsOnCooldown => currentCooldown > 0f;
        public float CurrentCooldown => currentCooldown;
        public float MaxCooldown => cooldown;

        public virtual void Initialize(BaseEntity owner)
        {
            ownerEntity = owner;
        }

        private void Update()
        {
            if (currentCooldown > 0f)
            {
                currentCooldown -= Time.deltaTime;
            }
        }

        public virtual bool CanCast()
        {
            return !IsOnCooldown;
        }

        public virtual void BeginTargeting()
        {
            // Override this if the ability needs aiming before casting
            Debug.Log($"Targeting started for {abilityName}");
        }

        public virtual void CancelTargeting()
        {
            Debug.Log($"Targeting cancelled for {abilityName}");
        }

        public virtual void ExecuteCast(Vector3 targetPosition, BaseEntity targetEntity)
        {
            if (!CanCast()) return;

            currentCooldown = cooldown;
            Debug.Log($"Cast executed for {abilityName}");
        }
    }
}