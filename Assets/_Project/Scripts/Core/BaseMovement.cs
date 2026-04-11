using System;
using UnityEngine;

namespace MobaGameplay.Core
{
    /// <summary>
    /// Base interface for all movement systems.
    /// Provides a unified API for player movement, dash, and aiming.
    /// </summary>
    [RequireComponent(typeof(BaseEntity))]
    public abstract class BaseMovement : MonoBehaviour
    {
        // Constants
        protected const float DEFAULT_DEADZONE = 0.01f;
        
        // Events
        public event Action OnDashStart;
        
        protected void TriggerOnDashStart()
        {
            OnDashStart?.Invoke();
        }

        
        // Properties
        public abstract float CurrentVelocity { get; }
        public abstract Vector3 VelocityVector { get; }
        public abstract bool IsGrounded { get; }
        public abstract bool IsJumping { get; }
        public abstract bool IsDashing { get; }

        // Movement API
        public abstract void MoveTo(Vector3 destination);
        public abstract void MoveDirection(Vector3 direction);
        public abstract void SetSprint(bool sprint);
        public abstract void SetAiming(bool aiming);
        public abstract void Stop();
        public abstract void Jump();
        public abstract void LookAtPoint(Vector3 point);
        public abstract void Dash(Vector3 direction);
    }
}
