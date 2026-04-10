using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Combat;
using MobaGameplay.Abilities.Projectiles;

namespace MobaGameplay.Abilities.Behaviors
{
    /// <summary>
    /// Spawns a projectile that travels in a direction and deals damage on hit.
    /// Used for: Fireball, skillshots, any directional projectile ability.
    /// </summary>
    public class ProjectileBehavior : IAbilityBehavior
    {
        public void Execute(AbilityContext context)
        {
            if (context.Owner == null || context.Data == null) return;

            AbilityData data = context.Data;

            if (data.projectilePrefab == null)
            {
                Debug.LogError($"[ProjectileBehavior] No projectile prefab assigned for '{data.abilityName}'");
                return;
            }

            // Calculate direction (flat on XZ plane)
            Vector3 origin = context.Owner.transform.position;
            Vector3 direction = context.TargetPosition - origin;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = context.Owner.transform.forward;
            }

            direction.Normalize();

            // Face the target instantly (abilities snap rotation, no smooth turn)
            context.Owner.transform.forward = direction;

            // Spawn projectile
            Vector3 spawnPos = origin + Vector3.up * data.projectileSpawnHeight + direction * data.projectileSpawnForward;
            GameObject projObj = Object.Instantiate(data.projectilePrefab, spawnPos, Quaternion.identity);

            if (projObj.TryGetComponent(out LinearProjectile proj))
            {
                // Calculate damage: base + scaling
                float totalDamage = data.baseDamage 
                    + (context.Owner.AbilityPower * data.apRatio)
                    + (context.Owner.AttackDamage * data.adRatio);

                proj.Initialize(direction, totalDamage, data.damageType, context.Owner);
            }

            Debug.Log($"[ProjectileBehavior] Launched '{data.abilityName}' at {spawnPos}");
        }
    }
}