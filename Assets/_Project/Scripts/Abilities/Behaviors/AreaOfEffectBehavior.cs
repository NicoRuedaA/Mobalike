using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Combat;
using MobaGameplay.Abilities.AreaEffects;

namespace MobaGameplay.Abilities.Behaviors
{
    /// <summary>
    /// Deals damage in an area at the target position.
    /// Optionally spawns VFX and uses AoEZone for delayed damage.
    /// Used for: Ground Smash, AoE nukes.
    /// </summary>
    public class AreaOfEffectBehavior : IAbilityBehavior
    {
        private const float DIRECTION_THRESHOLD = 0.001f;

        public void Execute(AbilityContext context)
        {
            if (context.Owner == null || context.Data == null) return;

            AbilityData data = context.Data;

            // Clamp target position to cast range
            Vector3 origin = context.Owner.transform.position;
            Vector3 direction = context.TargetPosition - origin;
            direction.y = 0f;

            if (direction.magnitude > data.castRange)
            {
                context.TargetPosition = origin + direction.normalized * data.castRange;
                // Recalculate direction after clamping
                direction = context.TargetPosition - origin;
                direction.y = 0f;
            }

            // Face the target instantly (abilities snap rotation, no smooth turn)
            if (direction.sqrMagnitude > DIRECTION_THRESHOLD)
            {
                context.Owner.transform.forward = direction.normalized;
            }

            // Spawn VFX at target location
            if (data.vfxPrefab != null)
            {
                Object.Instantiate(data.vfxPrefab, context.TargetPosition, Quaternion.identity);
            }

            // Calculate damage: base + scaling
            float totalDamage = data.baseDamage 
                + (context.Owner.AttackDamage * data.adRatio)
                + (context.Owner.AbilityPower * data.apRatio);

            // Spawn AoE zone for delayed damage
            if (data.aoePrefab != null)
            {
                GameObject zoneObj = Object.Instantiate(data.aoePrefab, context.TargetPosition, Quaternion.identity);
                if (zoneObj.TryGetComponent(out AoEZone zone))
                {
                    zone.Initialize(context.Owner, data.aoeDelay, data.aoeRadius, totalDamage, ~0);
                }
            }
            else
            {
                // No AoE prefab — instant damage via OverlapSphere
                Collider[] hits = Physics.OverlapSphere(context.TargetPosition, data.aoeRadius);
                foreach (var hit in hits)
                {
                    BaseEntity target = hit.GetComponentInParent<BaseEntity>();
                    if (target != null && target != context.Owner && !target.IsDead)
                    {
                        target.TakeDamage(new DamageInfo(totalDamage, data.damageType, context.Owner));
                    }
                }
            }

            Debug.Log($"[AreaOfEffectBehavior] Executed '{data.abilityName}' at {context.TargetPosition}");
        }
    }
}