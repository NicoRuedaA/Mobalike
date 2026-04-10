namespace MobaGameplay.Abilities
{
    /// <summary>
    /// Interface for ability behaviors. Each AbilityBehaviorType maps to
    /// one implementation. Behaviors are stateless — they receive all data
    /// via AbilityContext and spawn/modify objects as needed.
    /// 
    /// To add a new ability type:
    /// 1. Add a value to AbilityBehaviorType enum
    /// 2. Create a new class implementing IAbilityBehavior
    /// 3. Register it in AbilityBehaviorFactory
    /// </summary>
    public interface IAbilityBehavior
    {
        /// <summary>
        /// Execute the ability behavior. Spawn projectiles, apply damage, etc.
        /// All context is provided — no internal state needed.
        /// </summary>
        void Execute(AbilityContext context);
    }
}