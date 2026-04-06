using UnityEngine;
using MobaGameplay.Abilities;

namespace MobaGameplay.Core
{
    public abstract class BaseEntity : MonoBehaviour
    {
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
