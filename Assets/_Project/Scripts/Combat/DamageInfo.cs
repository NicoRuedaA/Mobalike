using UnityEngine;

namespace MobaGameplay.Combat
{
    public enum DamageType
    {
        Physical,
        Magical,
        TrueDamage
    }

    public struct DamageInfo
    {
        public float Amount;
        public DamageType Type;
        public Core.BaseEntity Source;
        public Vector3 HitPoint;
        public bool IsCritical;

        public DamageInfo(float amount, DamageType type, Core.BaseEntity source, bool isCritical = false)
        {
            Amount = amount;
            Type = type;
            Source = source;
            HitPoint = Vector3.zero;
            IsCritical = isCritical;
        }
    }
}