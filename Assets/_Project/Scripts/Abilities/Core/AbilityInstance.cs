using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Abilities
{
    /// <summary>
    /// Runtime state for an ability. One instance per equipped slot.
    /// Tracks cooldowns and provides the public API for ability execution.
    /// This is a plain C# class — NOT a MonoBehaviour. The AbilitySystem owns and manages these.
    /// </summary>
    public class AbilityInstance
    {
        // The data definition (immutable config)
        public AbilityData Data { get; }
        
        // The entity that owns this ability
        public BaseEntity Owner { get; }
        
        // Cooldown state
        public float CurrentCooldown { get; private set; }
        public bool IsOnCooldown => CurrentCooldown > 0f;
        public float CooldownPercent => Data.cooldown > 0f ? CurrentCooldown / Data.cooldown : 0f;
        public bool HasEnoughMana => Owner != null && Owner.CurrentMana >= Data.manaCost;
        
        // Events
        public event System.Action<AbilityInstance> OnCooldownStarted;
        public event System.Action<AbilityInstance> OnAbilityExecuted;

        public AbilityInstance(AbilityData data, BaseEntity owner)
        {
            Data = data;
            Owner = owner;
        }

        /// <summary>
        /// Check if this ability can be cast right now.
        /// </summary>
        public bool CanCast()
        {
            if (Owner == null) return false;
            return !IsOnCooldown && HasEnoughMana && !Owner.IsDead;
        }

        /// <summary>
        /// Start the cooldown timer. Called after successful cast.
        /// </summary>
        public void StartCooldown()
        {
            CurrentCooldown = Data.cooldown;
            OnCooldownStarted?.Invoke(this);
        }

        /// <summary>
        /// Tick cooldown down by deltaTime. Called by AbilitySystem.Update().
        /// </summary>
        public void TickCooldown(float deltaTime)
        {
            if (CurrentCooldown > 0f)
            {
                CurrentCooldown -= deltaTime;
                if (CurrentCooldown < 0f) CurrentCooldown = 0f;
            }
        }

        /// <summary>
        /// Consume mana from the owner. Returns true if successful.
        /// </summary>
        public bool ConsumeMana()
        {
            if (Owner == null || !HasEnoughMana) return false;
            Owner.CurrentMana -= Data.manaCost;
            return true;
        }

        /// <summary>
        /// Notify that the ability was executed. Triggers event.
        /// </summary>
        public void NotifyExecuted()
        {
            OnAbilityExecuted?.Invoke(this);
        }
    }
}