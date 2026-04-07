using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Abilities
{
    public class DashAbility : BaseAbility
    {
        [Header("Dash Settings")]
        public float dashSpeed = 20f;
        public float dashDuration = 0.2f;

        public override void ExecuteCast(Vector3 targetPosition, BaseEntity targetEntity)
        {
            if (!CanCast()) return;

            base.ExecuteCast(targetPosition, targetEntity); // Consumes mana + starts cooldown

            Vector3 dir = (targetPosition - ownerEntity.transform.position).normalized;
            dir.y = 0;

            if (ownerEntity.Movement != null)
            {
                ownerEntity.Movement.Dash(dir);
            }
        }
    }
}