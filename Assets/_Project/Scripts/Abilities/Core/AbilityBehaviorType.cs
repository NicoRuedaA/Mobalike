using MobaGameplay.Combat;

namespace MobaGameplay.Abilities
{
    /// <summary>
    /// Defines the behavior type for an ability.
    /// Each type maps to an IAbilityBehavior implementation.
    /// Adding a new type = adding a new behavior class (no MonoBehaviour needed).
    /// </summary>
    public enum AbilityBehaviorType
    {
        /// <summary>Spawn a projectile that travels in a direction (Fireball, etc.)</summary>
        Projectile,
        /// <summary>Dash the caster in a direction</summary>
        Dash,
        /// <summary>Deal damage in an area at target position (Ground Smash, etc.)</summary>
        AreaOfEffect,
        /// <summary>Spawn a trail of damage zones along a path (Ground Trail, etc.)</summary>
        Trail,
        /// <summary>Heal or apply a buff to self or ally</summary>
        HealBuff,
    }
}