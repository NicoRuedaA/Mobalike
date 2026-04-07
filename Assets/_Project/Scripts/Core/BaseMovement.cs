using UnityEngine;

namespace MobaGameplay.Core
{
    [RequireComponent(typeof(BaseEntity))]
    public abstract class BaseMovement : MonoBehaviour
    {
        public abstract float CurrentVelocity { get; }
        public abstract Vector3 VelocityVector { get; }
        public abstract bool IsGrounded { get; }
        public abstract bool IsJumping { get; }

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
