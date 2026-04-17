using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Abilities.Behaviors
{
    /// <summary>
    /// Dashes the caster in the direction of the target position.
    /// Used for: Dash, blink abilities.
    /// </summary>
    public class DashBehavior : IAbilityBehavior
    {
        private const float DIRECTION_THRESHOLD = 0.001f;

        public void Execute(AbilityContext context)
        {
            if (context.Owner == null || context.Data == null) return;

            AbilityData data = context.Data;

            // Calculate direction (flat on XZ plane)
            Vector3 direction = context.TargetPosition - context.Owner.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < DIRECTION_THRESHOLD)
            {
                direction = context.Owner.transform.forward;
            }

            direction.Normalize();

            // Dash in direction (DashAbility did NOT face target, just dashed)
            context.Owner.Movement?.Dash(direction);

            Debug.Log($"[DashBehavior] Executed '{data.abilityName}' dash");
        }
    }
}