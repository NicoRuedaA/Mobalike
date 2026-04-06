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
    // Ya no detenemos el movimiento aquí para poder caminar y atacar a la vez.
    // if (entity.Movement != null)
    // {
    //     entity.Movement.Stop();
    // }

    // Disparamos el evento (esto avisará al animador)
    base.BasicAttack();
}
    }
}
