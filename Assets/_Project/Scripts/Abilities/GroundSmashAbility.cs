using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Combat;

namespace MobaGameplay.Abilities
{
    public class GroundSmashAbility : BaseAbility
    {
        [Header("Smash Settings")]
        public float baseDamage = 100f;
        public float adRatio = 0.8f;
        public GameObject vfxPrefab;

        public override void ExecuteCast(Vector3 targetPosition, BaseEntity targetEntity)
        {
            if (!CanCast()) return;
            if (ownerEntity.CurrentMana < manaCost) return;

            Vector3 direction = targetPosition - ownerEntity.transform.position;
            direction.y = 0;
            if (direction.magnitude > CastRange)
            {
                targetPosition = ownerEntity.transform.position + direction.normalized * CastRange;
            }

            ownerEntity.CurrentMana -= manaCost;
            currentCooldown = cooldown;
            ownerEntity.transform.forward = direction.normalized;

            if (vfxPrefab != null) Instantiate(vfxPrefab, targetPosition, Quaternion.identity);

            float totalDamage = baseDamage + (ownerEntity.AttackDamage * adRatio);
            Collider[] hitColliders = Physics.OverlapSphere(targetPosition, Range);
            foreach (var hit in hitColliders)
            {
                BaseEntity target = hit.GetComponentInParent<BaseEntity>();
                if (target != null && target != ownerEntity)
                {
                    target.TakeDamage(new DamageInfo(totalDamage, DamageType.Physical, ownerEntity));
                }
            }

            CancelTargeting();
        }
    }
}
