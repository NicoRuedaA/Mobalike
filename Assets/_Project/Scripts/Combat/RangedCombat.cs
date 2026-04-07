using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Abilities.Projectiles;

namespace MobaGameplay.Combat
{
    /// <summary>
    /// Ranged combat system with charged attack mechanic.
    /// Supports charging by holding attack button while aiming.
    /// </summary>
    [RequireComponent(typeof(BaseEntity))]
    public class RangedCombat : BaseCombat
    {
        // Constants
        private const float DEFAULT_CHARGE_TIME = 1f;
        private const float DEFAULT_COOLDOWN = 2f;
        private const float MIN_CHARGE_THRESHOLD = 0.1f;
        private const float PROJECTILE_SPAWN_HEIGHT = 1f;
        private const float PROJECTILE_SPAWN_FORWARD_OFFSET = 0.5f;
        private const float DEFAULT_PROJECTILE_SPEED = 25f;
        private const float DEFAULT_PROJECTILE_DISTANCE = 20f;
        private const float DEFAULT_DAMAGE = 10f;

        // Serializable Fields
        [Header("Projectile Setup")]
        [SerializeField] private GameObject basicAttackProjectilePrefab;
        [SerializeField] private Transform projectileSpawnPoint;
        
        [Header("Projectile Stats")]
        [SerializeField] private float projectileSpeed = DEFAULT_PROJECTILE_SPEED;
        [SerializeField] private float projectileMaxDistance = DEFAULT_PROJECTILE_DISTANCE;
        [SerializeField] private LayerMask hitLayers = ~0;

        [Header("Charged Attack")]
        [SerializeField] private float chargedDamageMultiplier = 1.5f;
        [SerializeField] private float chargedSpeedMultiplier = 1.3f;
        [SerializeField] private float chargedSizeMultiplier = 1.5f;
        [SerializeField] private float maxChargeTime = DEFAULT_CHARGE_TIME;
        [SerializeField] private float chargedCooldown = DEFAULT_COOLDOWN;

        // State
        private BaseEntity entity;
        private float lastAttackTime = float.MinValue;
        private float lastChargedTime = float.MinValue;

        // Properties
        public bool IsCharging { get; private set; }
        public float ChargeProgress { get; private set; }
        public float MaxChargeTime => maxChargeTime;
        public bool CanCharge => !IsOnChargedCooldown;
        public bool IsOnChargedCooldown => Time.time < lastChargedTime + chargedCooldown;
        public float ChargePercent => Mathf.Clamp01(ChargeProgress / maxChargeTime);

        // Properties for external access
        public float ChargedDamageMultiplier => chargedDamageMultiplier;
        public float ChargedSpeedMultiplier => chargedSpeedMultiplier;
        public float ChargedSizeMultiplier => chargedSizeMultiplier;

        private void Awake()
        {
            entity = GetComponent<BaseEntity>();
        }

        public override void BasicAttack()
        {
            if (entity == null || entity.IsDead) return;

            float atkSpeed = entity.AttackSpeed;
            float attackInterval = 1f / atkSpeed;

            if (Time.time >= lastAttackTime + attackInterval)
            {
                lastAttackTime = Time.time;
                
                bool isCharged = IsCharging && ChargeProgress > MIN_CHARGE_THRESHOLD;
                FireProjectile(isCharged);
                ResetCharge();
                
                base.BasicAttack();
            }
        }

        /// <summary>
        /// Begin charging a charged attack. Call when player starts aiming.
        /// </summary>
        public void StartCharging()
        {
            if (IsOnChargedCooldown || entity?.IsDead == true) return;
            
            IsCharging = true;
            ChargeProgress = 0f;
        }

        /// <summary>
        /// Update charge progress. Call every frame while charging.
        /// </summary>
        public void UpdateCharge()
        {
            if (!IsCharging || entity?.IsDead == true) return;
            
            if (ChargeProgress < maxChargeTime)
            {
                ChargeProgress += Time.deltaTime;
                ChargeProgress = Mathf.Min(ChargeProgress, maxChargeTime);
            }
        }

        /// <summary>
        /// Reset current charge. Call when player stops aiming or attacks.
        /// </summary>
        public void ResetCharge()
        {
            IsCharging = false;
            ChargeProgress = 0f;
        }

        /// <summary>
        /// Cancel charge without firing. Call when charge should be aborted.
        /// </summary>
        public void CancelCharge()
        {
            if (IsCharging)
            {
                ResetCharge();
            }
        }

        private void FireProjectile(bool isCharged)
        {
            if (basicAttackProjectilePrefab == null) return;
            
            // Calculate spawn position
            Vector3 spawnPos = projectileSpawnPoint != null 
                ? projectileSpawnPoint.position 
                : CalculateDefaultSpawnPosition();

            // Calculate flat direction (Y = 0)
            Vector3 dir = transform.forward;
            dir.y = 0;
            dir.Normalize();

            // Spawn projectile
            GameObject projObj = Instantiate(basicAttackProjectilePrefab, spawnPos, Quaternion.LookRotation(dir));
            BasicAttackProjectile projectile = projObj.GetComponent<BasicAttackProjectile>();
            
            if (projectile != null)
            {
                float damage = entity?.AttackDamage ?? DEFAULT_DAMAGE;
                projectile.Initialize(dir, damage, DamageType.Physical, entity, 
                    projectileSpeed, projectileMaxDistance, hitLayers);

                if (isCharged)
                {
                    ApplyChargedEffects(projectile);
                }
            }
        }

        private Vector3 CalculateDefaultSpawnPosition()
        {
            return transform.position + Vector3.up * PROJECTILE_SPAWN_HEIGHT + 
                   transform.forward * PROJECTILE_SPAWN_FORWARD_OFFSET;
        }

        private void ApplyChargedEffects(BasicAttackProjectile projectile)
        {
            float t = ChargeProgress / maxChargeTime; // Normalized 0-1
            
            float sizeMult = Mathf.Lerp(1f, chargedSizeMultiplier, t);
            float speedMult = Mathf.Lerp(1f, chargedSpeedMultiplier, t);
            float damageMult = Mathf.Lerp(1f, chargedDamageMultiplier, t);
            
            projectile.ApplyChargeMultiplier(sizeMult, speedMult, damageMult);
            lastChargedTime = Time.time;
        }

        /// <summary>
        /// Get current charge level as a percentage (0-1).
        /// </summary>
        public float GetNormalizedCharge()
        {
            return maxChargeTime > 0f ? ChargeProgress / maxChargeTime : 0f;
        }
    }
}
