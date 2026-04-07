using System;
using UnityEngine;

namespace MobaGameplay.Core
{
    /// <summary>
    /// Base class for all combat systems.
    /// Handles basic attack events and provides a foundation for ranged/melee combat.
    /// </summary>
    public abstract class BaseCombat : MonoBehaviour
    {
        // Events
        public event Action OnBasicAttack;

        // Properties
        protected BaseEntity Owner => GetComponent<BaseEntity>();

        /// <summary>
        /// Execute a basic attack. Override for custom behavior.
        /// </summary>
        public virtual void BasicAttack()
        {
            OnBasicAttack?.Invoke();
        }

        /// <summary>
        /// Check if the owner can perform a basic attack.
        /// Override to add cooldown or other restrictions.
        /// </summary>
        public virtual bool CanBasicAttack()
        {
            return Owner != null && !Owner.IsDead;
        }
    }
}
