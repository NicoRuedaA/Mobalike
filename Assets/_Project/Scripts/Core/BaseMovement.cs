using UnityEngine;

namespace MobaGameplay.Core
{
    public abstract class BaseMovement : MonoBehaviour
    {
        public abstract float CurrentVelocity { get; }
        public abstract void MoveTo(Vector3 destination);
        public abstract void MoveDirection(Vector3 direction);
        public abstract void SetSprint(bool isSprinting);
        public abstract void Stop();
    }
}
