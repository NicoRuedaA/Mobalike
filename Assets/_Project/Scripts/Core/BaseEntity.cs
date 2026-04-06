using UnityEngine;
using MobaGameplay.Abilities;

namespace MobaGameplay.Core
{
    public abstract class BaseEntity : MonoBehaviour
    {
        [Header("Entity Stats")]
        public float MaxHealth = 1000f;
        public float CurrentHealth = 1000f;
        public float MaxMana = 500f;
        public float CurrentMana = 500f;

        public BaseMovement Movement { get; private set; }
        public BaseCombat Combat { get; private set; }
        public AbilityController Abilities { get; private set; }

        protected virtual void Awake()
        {
            Movement = GetComponent<BaseMovement>();
            Combat = GetComponent<BaseCombat>();
            Abilities = GetComponent<AbilityController>();
        }
    }
}
