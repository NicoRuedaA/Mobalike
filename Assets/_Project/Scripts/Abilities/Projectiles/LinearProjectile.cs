using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Combat;

namespace MobaGameplay.Abilities.Projectiles
{
    public class LinearProjectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float maxDistance = 20f;
        [SerializeField] private float collisionRadius = 0.5f;
        [SerializeField] private LayerMask hitLayers;

        [Header("Ground Detection")]
        [SerializeField] private float groundNormalThreshold = 0.8f;
        [SerializeField] private float groundHeightThreshold = 0.1f;

        // Public read-only properties
        public float Speed => speed;
        public float MaxDistance => maxDistance;
        public float CollisionRadius => collisionRadius;
        public LayerMask HitLayers => hitLayers;

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
                if (hit.collider.isTrigger) continue; // Ignoramos triggers (zonas de aggro, etc)

                BaseEntity target = hit.collider.GetComponentInParent<BaseEntity>();
                
                // Si choca con el propio lanzador (ej. su propio collider), ignorarlo por completo
                if (target == owner) continue; 

                // Si choca con otro Entity (enemigo o aliado)
                if (target != null)
                {
                    if (!target.IsDead)
                    {
                        target.TakeDamage(new DamageInfo(damage, damageType, owner));
                        HitAndDestroy();
                        return;
                    }
                }
                else
                {
                    // Si es el suelo (normal hacia arriba) lo ignoramos para evitar chocar por error
                    if (Vector3.Dot(hit.normal, Vector3.up) > groundNormalThreshold || hit.point.y < groundHeightThreshold) 
                        continue;

                    // Es una pared u obstáculo estático
                    HitAndDestroy();
                    return;
                }
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