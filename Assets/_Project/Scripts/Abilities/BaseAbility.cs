using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.UI.Targeting;

namespace MobaGameplay.Abilities
{
    public abstract class BaseAbility : MonoBehaviour
    {
        [Header("Ability Settings")]
        public string abilityName = "New Ability";
        public float cooldown = 5f;
        public float castTime = 0f;
        public float manaCost = 0f;

        [Header("Targeting Settings")]
        public IndicatorType TargetingType = IndicatorType.Circle;
        public float CastRange = 10f; // Max distance from player
        public float Range = 2f; // Radius of circle or length of line
        public float Width = 1f; // Width of line or angle of cone

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
            if (TargetingManager.Instance != null)
            {
                TargetingManager.Instance.StartTargeting(this);
            }
        }

        public virtual void CancelTargeting()
        {
            if (TargetingManager.Instance != null)
            {
                TargetingManager.Instance.CancelTargeting();
            }
        }

        public virtual void ExecuteCast(Vector3 targetPosition, BaseEntity targetEntity)
        {
            if (!CanCast()) return;

            currentCooldown = cooldown;
            CancelTargeting();
        }
    }
}