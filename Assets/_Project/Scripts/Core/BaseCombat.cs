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
        public event Action OnReloadStart;
        public event Action<int, int> OnReloadComplete;  // current, max
        public event Action OnReloadCancelled;
        public event Action<int, int> OnAmmoChanged;  // current, max

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
        
        // Protected methods to trigger events from derived classes
        protected void TriggerOnAmmoChanged(int current, int max) => OnAmmoChanged?.Invoke(current, max);
        protected void TriggerOnReloadStart() => OnReloadStart?.Invoke();
        protected void TriggerOnReloadComplete(int current, int max) => OnReloadComplete?.Invoke(current, max);
        protected void TriggerOnReloadCancelled() => OnReloadCancelled?.Invoke();
        
        // Legacy overload without parameters
        protected void TriggerOnReloadComplete() => OnReloadComplete?.Invoke(0, 0);
    }
}
