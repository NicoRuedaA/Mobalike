using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Abilities.Projectiles;

namespace MobaGameplay.Abilities.Types
{
    public class ProjectileAbility : BaseAbility
    {
        [Header("Skillshot Settings")]
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform spawnPoint; // From where the projectile is shot (e.g. hand, staff)
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private float damage = 50f;
        [SerializeField] private float maxRange = 15f;

        public override void ExecuteCast(Vector3 targetPosition, BaseEntity targetEntity)
        {
            if (!CanCast()) return;

            if (projectilePrefab == null)
            {
                Debug.LogError($"[ProjectileAbility] No projectile prefab assigned for {abilityName}");
                return;
            }

            // Determine spawn position
            Vector3 startPos = spawnPoint != null ? spawnPoint.position : ownerEntity.transform.position + Vector3.up * 1f;

            // Calculate direction ignoring Y axis to keep it on the plane
            Vector3 direction = targetPosition - startPos;
            direction.y = 0f;
            direction.Normalize();

            // Rotate player to face target (optional but recommended for skillshots)
            ownerEntity.Movement.LookAtPoint(targetPosition);

            // Instantiate and initialize the projectile
            Projectile proj = Instantiate(projectilePrefab, startPos, Quaternion.identity);
            proj.Initialize(ownerEntity, direction, projectileSpeed, maxRange, damage);

            // Base class ExecuteCast handles Cooldown setup
            base.ExecuteCast(targetPosition, targetEntity);
        }
    }
}