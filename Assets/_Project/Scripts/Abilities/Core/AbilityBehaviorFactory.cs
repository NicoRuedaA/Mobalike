using System.Collections.Generic;
using MobaGameplay.Abilities.Behaviors;

namespace MobaGameplay.Abilities
{
    /// <summary>
    /// Factory for creating ability behaviors.
    /// Maps AbilityBehaviorType enum values to IAbilityBehavior implementations.
    /// 
    /// To add a new behavior type:
    /// 1. Add the enum value to AbilityBehaviorType
    /// 2. Create the behavior class implementing IAbilityBehavior
    /// 3. Add the mapping here in CreateBehavior()
    /// </summary>
    public static class AbilityBehaviorFactory
    {
        // Cached instances (behaviors are stateless, safe to reuse)
        private static readonly Dictionary<AbilityBehaviorType, IAbilityBehavior> cache = 
            new Dictionary<AbilityBehaviorType, IAbilityBehavior>();

        /// <summary>
        /// Get the behavior implementation for a given type.
        /// Returns a cached instance (behaviors are stateless).
        /// </summary>
        public static IAbilityBehavior GetBehavior(AbilityBehaviorType type)
        {
            if (cache.TryGetValue(type, out IAbilityBehavior cached))
            {
                return cached;
            }

            IAbilityBehavior behavior = type switch
            {
                AbilityBehaviorType.Projectile => new ProjectileBehavior(),
                AbilityBehaviorType.Dash => new DashBehavior(),
                AbilityBehaviorType.AreaOfEffect => new AreaOfEffectBehavior(),
                AbilityBehaviorType.Trail => new TrailBehavior(),
                AbilityBehaviorType.HealBuff => new HealBuffBehavior(),
                _ => null
            };

            if (behavior != null)
            {
                cache[type] = behavior;
            }

            return behavior;
        }
    }
}