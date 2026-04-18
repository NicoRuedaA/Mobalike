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
        public event Action<Vector3> OnDashWallHit;
        
        protected void TriggerOnDashStart()
        {
            OnDashStart?.Invoke();
        }
        
        protected void TriggerOnDashWallHit(Vector3 hitPoint)
        {
            OnDashWallHit?.Invoke(hitPoint);
        }

        
        // Properties
        public abstract float CurrentVelocity { get; }
        public abstract Vector3 VelocityVector { get; }
        public abstract bool IsGrounded { get; }
        public abstract bool IsJumping { get; }
        public abstract bool IsDashing { get; }
        public abstract bool IsSprinting { get; }

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
