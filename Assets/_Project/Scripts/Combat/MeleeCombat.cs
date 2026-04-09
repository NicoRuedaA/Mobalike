using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Combat
{
    [RequireComponent(typeof(BaseEntity))]
    public class MeleeCombat : BaseCombat
    {
        private BaseEntity entity;

        private void Awake()
        {
            entity = GetComponent<BaseEntity>();
        }

        public override void BasicAttack()
        {
            // Movement is not stopped here to allow walking and attacking simultaneously
            base.BasicAttack();
        }
    }
}
