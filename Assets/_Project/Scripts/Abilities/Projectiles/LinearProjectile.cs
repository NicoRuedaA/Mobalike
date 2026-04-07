using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Combat;

namespace MobaGameplay.Abilities.Projectiles
{
    public class LinearProjectile : MonoBehaviour
    {
        public float speed = 15f;
        public float maxDistance = 20f;
        public float collisionRadius = 0.5f;
        public LayerMask hitLayers;

        private Vector3 startPos;
        private Vector3 direction;
        private float damage;
        private DamageType damageType;
        private BaseEntity owner;

        public void Initialize(Vector3 dir, float dmg, DamageType type, BaseEntity caster)
        {
            direction = dir.normalized;
            damage = dmg;
            damageType = type;
            owner = caster;
            startPos = transform.position;
            transform.forward = direction;
        }

        private void Update()
        {
            if (direction == Vector3.zero) return;

            float distanceToMove = speed * Time.deltaTime;
            
            // Usar SphereCastAll para evitar que nuestro propio collider bloquee el rayo
            // y nos impida detectar al enemigo que está justo detrás.
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, collisionRadius, direction, distanceToMove, hitLayers);
            
            foreach (var hit in hits)
            {
                BaseEntity target = hit.collider.GetComponentInParent<BaseEntity>();
                
                // Ignorar colisiones con el propio lanzador
                if (target == owner) continue; 

                // Si es un enemigo, aplicar daño
                if (target != null)
                {
                    target.TakeDamage(new DamageInfo(damage, damageType, owner));
                }
                
                // Destruir el proyectil ante CUALQUIER otro impacto válido (muros, enemigos, escudos)
                HitAndDestroy();
                return;
            }

            transform.position += direction * distanceToMove;

            if (Vector3.Distance(startPos, transform.position) >= maxDistance)
            {
                Destroy(gameObject);
            }
        }

        private void HitAndDestroy()
        {
            Destroy(gameObject);
        }
    }
}
