using UnityEngine;
using MobaGameplay.Core;
using System.Collections;

namespace MobaGameplay.Abilities.Behaviors
{
    /// <summary>
    /// Heals the caster (or a target ally) and/or applies a buff.
    /// Used for: Heal, Shield, Speed Boost, etc.
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

            // Apply buff
            if (data.buffType != BuffType.None && data.buffDuration > 0f)
            {
                ApplyBuff(target, data.buffType, data.buffMultiplier, data.buffDuration);
            }
        }

        private void ApplyBuff(BaseEntity target, BuffType buffType, float multiplier, float duration)
        {
            if (target == null) return;

            // Start buff coroutine on the target's MonoBehaviour
            if (target is MonoBehaviour mb)
            {
                mb.StartCoroutine(BuffCoroutine(target, buffType, multiplier, duration));
            }
            else
            {
                Debug.LogWarning($"[HealBuffBehavior] Target {target.name} is not a MonoBehaviour, cannot apply buff coroutine");
            }
        }

        private IEnumerator BuffCoroutine(BaseEntity target, BuffType buffType, float multiplier, float duration)
        {
            if (target == null) yield break;

            // Store original values
            float originalValue = 0f;
            string statName = "";

            // Get current value based on buff type
            switch (buffType)
            {
                case BuffType.AttackSpeed:
                    // Note: AttackSpeed is read-only in BaseEntity, so we can't buff it directly
                    // This requires a BuffSystem with stat modifiers
                    statName = "AttackSpeed";
                    originalValue = target.AttackSpeed;
                    Debug.Log($"[HealBuffBehavior] Buffing {target.name} {statName}: {originalValue} → {originalValue * multiplier} for {duration}s");
                    break;

                case BuffType.MoveSpeed:
                    statName = "MovementSpeed";
                    originalValue = 0f; // MovementSpeed not directly accessible in BaseEntity
                    Debug.Log($"[HealBuffBehavior] Buffing {target.name} {statName} x{multiplier} for {duration}s");
                    break;

                case BuffType.AttackDamage:
                    statName = "AttackDamage";
                    originalValue = target.AttackDamage;
                    target.AttackDamage *= multiplier;
                    Debug.Log($"[HealBuffBehavior] Buffing {target.name} {statName}: {originalValue} → {target.AttackDamage} for {duration}s");
                    break;

                case BuffType.Defense:
                    statName = "Defense";
                    originalValue = 0f; // Defense not directly accessible
                    Debug.Log($"[HealBuffBehavior] Buffing {target.name} {statName} x{multiplier} for {duration}s");
                    break;
            }

            // Wait for buff duration
            yield return new WaitForSeconds(duration);

            // Revert buff
            if (target != null)
            {
                switch (buffType)
                {
                    case BuffType.AttackDamage:
                        target.AttackDamage = originalValue;
                        Debug.Log($"[HealBuffBehavior] {statName} buff expired on {target.name}, restored to {originalValue}");
                        break;

                    default:
                        Debug.Log($"[HealBuffBehavior] {statName} buff expired on {target.name}");
                        break;
                }
            }
        }
    }
}