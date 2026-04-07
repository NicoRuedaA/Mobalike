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
        [SerializeField] private LayerMask hitLayers;

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
            
            Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : (transform.position + Vector3.up * 1f + transform.forward * 0.5f);
            
            GameObject projObj = Instantiate(basicAttackProjectilePrefab, spawnPos, transform.rotation);
            BasicAttackProjectile projectile = projObj.GetComponent<BasicAttackProjectile>();
            
            if (projectile != null)
            {
                float damage = entity != null ? entity.AttackDamage : 10f;
                projectile.Initialize(transform.forward, damage, DamageType.Physical, entity, projectileSpeed, projectileMaxDistance, hitLayers);
            }
        }
    }
}
