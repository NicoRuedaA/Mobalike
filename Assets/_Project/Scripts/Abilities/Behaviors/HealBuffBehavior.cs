using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Abilities.Behaviors
{
    /// <summary>
    /// Heals the caster (or a target ally) and/or applies a buff.
    /// Used for: Heal, Shield, Speed Boost, etc.
    /// 
    /// This is a FUTURE behavior type — implemented now as a placeholder
    /// for Phase 5 (Nuevas Habilidades). It works but hasn't been
    /// thoroughly tested with all buff combinations yet.
    /// </summary>
    public class HealBuffBehavior : IAbilityBehavior
    {
        public void Execute(AbilityContext context)
        {
            if (context.Owner == null || context.Data == null) return;

            AbilityData data = context.Data;
            BaseEntity target = context.TargetEntity ?? context.Owner;

            // Apply healing
            if (data.healAmount > 0f)
            {
                target.Heal(data.healAmount);
                Debug.Log($"[HealBuffBehavior] '{data.abilityName}' healed {target.gameObject.name} for {data.healAmount}");
            }

            // Restore mana
            if (data.manaRestoreAmount > 0f)
            {
                target.RestoreMana(data.manaRestoreAmount);
                Debug.Log($"[HealBuffBehavior] '{data.abilityName}' restored {data.manaRestoreAmount} mana to {target.gameObject.name}");
            }

            // TODO: Apply buff (AttackSpeed, MoveSpeed, etc.) when BuffSystem is implemented
            if (data.buffType != BuffType.None && data.buffDuration > 0f)
            {
                Debug.Log($"[HealBuffBehavior] Buff {data.buffType} x{data.buffMultiplier} for {data.buffDuration}s — not yet implemented");
            }
        }
    }
}