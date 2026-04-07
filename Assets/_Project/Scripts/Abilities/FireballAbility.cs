using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Combat;
using MobaGameplay.Abilities.Projectiles;

namespace MobaGameplay.Abilities
{
    public class FireballAbility : BaseAbility
    {
        [Header("Fireball Settings")]
        public GameObject projectilePrefab;
        public float baseDamage = 80f;
        public float apRatio = 0.6f;

        public override void ExecuteCast(Vector3 targetPosition, BaseEntity targetEntity)
        {
            if (!CanCast()) return;

            base.ExecuteCast(targetPosition, targetEntity); // Consumes mana + starts cooldown

            Vector3 dir = (targetPosition - ownerEntity.transform.position).normalized;
            dir.y = 0;
            ownerEntity.transform.forward = dir;

            if (projectilePrefab != null)
            {
                Vector3 spawnPos = ownerEntity.transform.position + Vector3.up * 1f + dir * 1f;
                GameObject projObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
                Debug.Log($"[FireballAbility] Launched fireball at {spawnPos} facing {dir}");
                if (projObj.TryGetComponent(out LinearProjectile proj))
                {
                    float totalDamage = baseDamage + (ownerEntity.AbilityPower * apRatio);
                    proj.Initialize(dir, totalDamage, DamageType.Magical, ownerEntity);
                }
            }
            else
            {
                Debug.LogError("[FireballAbility] projectilePrefab is null!");
            }
        }
    }
}
