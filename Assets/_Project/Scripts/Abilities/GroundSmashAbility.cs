using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Combat;

namespace MobaGameplay.Abilities
{
    public class GroundSmashAbility : BaseAbility
    {
        [Header("Smash Settings")]
        [SerializeField] private float baseDamage = 100f;
        [SerializeField] private float adRatio = 0.8f;
        [SerializeField] private GameObject vfxPrefab;

        // Public read-only properties
        public float BaseDamage => baseDamage;
        public float AdRatio => adRatio;
        public GameObject VfxPrefab => vfxPrefab;

        public override void ExecuteCast(Vector3 targetPosition, BaseEntity targetEntity)
        {
            if (!CanCast()) return;

            Vector3 direction = targetPosition - ownerEntity.transform.position;
            direction.y = 0;
            if (direction.magnitude > CastRange)
            {
                targetPosition = ownerEntity.transform.position + direction.normalized * CastRange;
            }

            base.ExecuteCast(targetPosition, targetEntity); // Consumes mana + starts cooldown
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
        }
    }
}