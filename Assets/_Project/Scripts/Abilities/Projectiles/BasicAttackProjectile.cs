using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Combat;

namespace MobaGameplay.Abilities.Projectiles
{
    public class BasicAttackProjectile : MonoBehaviour
    {
        private float speed = 20f;
        private float maxDistance = 15f;
        private float collisionRadius = 0.3f;
        private LayerMask hitLayers;

        private Vector3 startPos;
        private Vector3 direction;
        private float damage;
        private DamageType damageType;
        private BaseEntity owner;
        private bool isInitialized = false;

        public void Initialize(Vector3 dir, float dmg, DamageType type, BaseEntity caster, float spd, float maxDist, LayerMask layers)
        {
            direction = dir.normalized;
            damage = dmg;
            damageType = type;
            owner = caster;
            speed = spd;
            maxDistance = maxDist;
            hitLayers = layers;
            
            startPos = transform.position;
            transform.forward = direction;
            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized || direction == Vector3.zero) return;

            float distanceToMove = speed * Time.deltaTime;
            
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, collisionRadius, direction, distanceToMove, hitLayers);
            
            foreach (var hit in hits)
            {
                BaseEntity target = hit.collider.GetComponentInParent<BaseEntity>();
                
                // Ignore self
                if (target == owner) continue; 

                if (target != null)
                {
                    target.TakeDamage(new DamageInfo(damage, damageType, owner));
                    HitAndDestroy();
                    return;
                }
                else 
                {
                    // Hit a wall/obstacle
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
