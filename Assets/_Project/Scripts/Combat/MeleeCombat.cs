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
            // 1. Detenemos el movimiento al lanzar el puñetazo
            if (entity.Movement != null)
            {
                entity.Movement.Stop();
            }

            // 2. Disparamos el evento (esto avisará al animador)
            base.BasicAttack();

            // Aquí en el futuro añadiremos lógica de cooldowns y daño
        }
    }
}
