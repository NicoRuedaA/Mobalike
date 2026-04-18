using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Abilities;
using MobaGameplay.Abilities.Projectiles;
using MobaGameplay.Visuals;

namespace MobaGameplay.Combat
{
    /// <summary>
    /// Ranged combat system with charged attack mechanic.
    /// Supports charging by holding attack button while aiming.
    /// 
    /// Configuration should come from HeroClass - use ConfigureFromHeroClass().
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

        // Serializable Fields (can be set from HeroClass or editor)
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
        
        [Header("Ammo System")]
        [SerializeField] private bool hasAmmoSystem = false;
        [SerializeField] private int maxAmmo = 6;
        [SerializeField] private float reloadTime = 2f;

        // State
        private BaseEntity entity;
        private float lastAttackTime = float.MinValue;
        private int currentAmmo;
        private bool isReloading = false;
        private float reloadCooldownTimer = 0f;
        private Coroutine reloadCoroutine;

        // Properties
        public bool IsCharging { get; private set; }
        public float ChargeProgress { get; private set; }
        public float MaxChargeTime => maxChargeTime;
        public bool CanCharge => !IsCharging;
        public float ChargePercent => Mathf.Clamp01(ChargeProgress / maxChargeTime);
        
        // Ammo Properties
        public int CurrentAmmo => currentAmmo;
        public int MaxAmmo => maxAmmo;
        public bool IsReloading => isReloading;
        public bool HasAmmoSystem => hasAmmoSystem;

        // Properties for external access
        public float ChargedDamageMultiplier => chargedDamageMultiplier;
        public float ChargedSpeedMultiplier => chargedSpeedMultiplier;
        public float ChargedSizeMultiplier => chargedSizeMultiplier;

        private void Awake()
        {
            entity = GetComponent<BaseEntity>();
        }

        /// <summary>
        /// Configure this combat component from HeroClass data.
        /// Call this from HeroEntity when setting up the hero.
        /// </summary>
        public void ConfigureFromHeroClass(HeroClass heroClass)
        {
            Debug.Log("[RangedCombat] ConfigureFromHeroClass START");
            
            if (heroClass == null) {
                Debug.LogWarning("[RangedCombat] ConfigureFromHeroClass - heroClass is null!");
                return;
            }

            basicAttackProjectilePrefab = heroClass.basicAttackProjectilePrefab;
            projectileSpeed = heroClass.projectileSpeed;
            projectileMaxDistance = heroClass.projectileMaxDistance;
            chargedDamageMultiplier = heroClass.chargedDamageMultiplier;
            chargedSpeedMultiplier = heroClass.chargedSpeedMultiplier;
            chargedSizeMultiplier = heroClass.chargedSizeMultiplier;
            
            // Ammo system
            hasAmmoSystem = heroClass.hasAmmoSystem;
            maxAmmo = heroClass.maxAmmo;
            reloadTime = heroClass.reloadTime;
            if (hasAmmoSystem)
            {
                currentAmmo = maxAmmo;  // Start with full ammo
            }
            
            // Setup LaserSight if showAimLines is enabled
            if (heroClass.showAimLines)
            {
                SetupLaserSight();
            }
            
            Debug.Log($"[RangedCombat] Configured - projectilePrefab={basicAttackProjectilePrefab?.name ?? "NULL"}, speed={projectileSpeed}");
        }

        /// <summary>
        /// Add LineRenderer and LaserSight components dynamically.
        /// Called from ConfigureFromHeroClass when showAimLines is true.
        /// </summary>
        private void SetupLaserSight()
        {
            var entityTransform = entity?.transform ?? transform;
            
            // Add LineRenderer if missing
            LineRenderer lineRenderer = entityTransform.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = entityTransform.gameObject.AddComponent<LineRenderer>();
            }
            
            // Add LaserSight if missing
            LaserSight laserSight = entityTransform.GetComponent<LaserSight>();
            if (laserSight == null)
            {
                laserSight = entityTransform.gameObject.AddComponent<LaserSight>();
            }
            
            Debug.Log("[RangedCombat] LaserSight configured");
        }

        public override void BasicAttack()
        {
            if (entity == null || entity.IsDead) return;
            
            // Check ammo
            if (hasAmmoSystem && currentAmmo <= 0)
            {
                Debug.Log($"[RangedCombat] No ammo! Press R to reload. ({reloadTime}s)");
                return;
            }
            
            // Check reload
            if (isReloading)
            {
                Debug.Log("[RangedCombat] Cannot attack while reloading!");
                return;
            }
            
            // Check cooldown
            float atkSpeed = entity.AttackSpeed;
            float attackInterval = 1f / atkSpeed;

            if (Time.time >= lastAttackTime + attackInterval)
            {
                lastAttackTime = Time.time;
                
                // Consume ammo
                if (hasAmmoSystem)
                {
                    currentAmmo--;
                    OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
                    Debug.Log($"[RangedCombat] Attack! Ammo left: {currentAmmo}/{maxAmmo}");
                }
                
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
            if (entity?.IsDead == true) return;
            
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
        }

        /// <summary>
        /// Get current charge level as a percentage (0-1).
        /// </summary>
        public float GetNormalizedCharge()
        {
            return maxChargeTime > 0f ? ChargeProgress / maxChargeTime : 0f;
        }
        
        // ============================================================
        // Ammo System
        // ============================================================
        
        /// <summary>
        /// Start reloading. Cannot attack while reloading.
        /// </summary>
        public void Reload()
        {
            if (!hasAmmoSystem) return;
            if (isReloading) return;
            if (currentAmmo >= maxAmmo)
            {
                Debug.Log("[RangedCombat] Ammo already full!");
                return;
            }
            if (reloadCooldownTimer > 0)
            {
                Debug.Log($"[RangedCombat] Reload on cooldown! ({reloadCooldownTimer:F1}s remaining)");
                return;
            }
            
            reloadCoroutine = StartCoroutine(ReloadCoroutine());
        }
        
        /// <summary>
        /// Cancel current reload (called when moving or dashing).
        /// </summary>
        public void CancelReload()
        {
            if (!isReloading) return;
            
            if (reloadCoroutine != null)
            {
                StopCoroutine(reloadCoroutine);
                reloadCoroutine = null;
            }
            
            isReloading = false;
            OnReloadCancelled?.Invoke();
            
            Debug.Log("[RangedCombat] Reload cancelled!");
        }
        
        /// <summary>
        /// Check if reload is available.
        /// </summary>
        public bool CanReload()
        {
            if (!hasAmmoSystem) return false;
            if (isReloading) return false;
            if (currentAmmo >= maxAmmo) return false;
            if (reloadCooldownTimer > 0) return false;
            return true;
        }
        
        private System.Collections.IEnumerator ReloadCoroutine()
        {
            isReloading = true;
            OnReloadStart?.Invoke();
            
            Debug.Log($"[RangedCombat] Reloading... ({reloadTime}s)");
            
            yield return new WaitForSeconds(reloadTime);
            
            currentAmmo = maxAmmo;
            isReloading = false;
            reloadCooldownTimer = 0.5f;  // Small cooldown post-reload
            reloadCoroutine = null;
            
            OnReloadComplete?.Invoke(currentAmmo, maxAmmo);
            
            Debug.Log($"[RangedCombat] Reload complete! ({currentAmmo}/{maxAmmo})");
            
            // Tick reload cooldown
            while (reloadCooldownTimer > 0)
            {
                reloadCooldownTimer -= Time.deltaTime;
                yield return null;
            }
            reloadCooldownTimer = 0;
        }
    }
}
