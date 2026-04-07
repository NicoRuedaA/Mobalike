using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Abilities.Projectiles;

namespace MobaGameplay.Combat
{
    [RequireComponent(typeof(BaseEntity))]
    public class RangedCombat : BaseCombat
    {
        [Header("Ranged Setup")]
        [SerializeField] private GameObject basicAttackProjectilePrefab;
        [SerializeField] private Transform projectileSpawnPoint;
        
        [Header("Projectile Stats")]
        [SerializeField] private float projectileSpeed = 25f;
        [SerializeField] private float projectileMaxDistance = 20f;
        [SerializeField] private LayerMask hitLayers = ~0;

        private BaseEntity entity;
        private float lastAttackTime = -999f;

        private void Awake()
        {
            entity = GetComponent<BaseEntity>();
        }

        public override void BasicAttack()
        {
            if (entity == null) return;

            float atkSpeed = entity.AttackSpeed > 0 ? entity.AttackSpeed : 1f;

            if (Time.time >= lastAttackTime + (1f / atkSpeed))
            {
                lastAttackTime = Time.time;
                FireProjectile();
                base.BasicAttack(); // Invokes the OnBasicAttack event (e.g. for animations)
            }
        }

        private void FireProjectile()
        {
            if (basicAttackProjectilePrefab == null) return;
            
            // Garantizar que la posición de spawn esté a 1.0 de altura y su trayectoria sea perfectamente plana
            Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : (transform.position + Vector3.up * 1.0f + transform.forward * 0.5f);
            
            Vector3 dir = transform.forward;
            dir.y = 0;
            dir.Normalize();

            GameObject projObj = Instantiate(basicAttackProjectilePrefab, spawnPos, Quaternion.LookRotation(dir));
            BasicAttackProjectile projectile = projObj.GetComponent<BasicAttackProjectile>();
            
            if (projectile != null)
            {
                float damage = entity != null ? entity.AttackDamage : 10f;
                projectile.Initialize(dir, damage, DamageType.Physical, entity, projectileSpeed, projectileMaxDistance, hitLayers);
            }
        }
    }
}
