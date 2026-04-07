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

        [Header("Charged Attack")]
        [Tooltip("Multiplicador de daño para ataques cargados (al apuntar)")]
        [SerializeField] private float chargedDamageMultiplier = 1.5f;
        [Tooltip("Multiplicador de velocidad para ataques cargados")]
        [SerializeField] private float chargedSpeedMultiplier = 1.3f;
        [Tooltip("Multiplicador de tamaño para ataques cargados")]
        [SerializeField] private float chargedSizeMultiplier = 1.5f;
        [Tooltip("Tiempo máximo de carga (segundos)")]
        [SerializeField] private float maxChargeTime = 1.0f;
        [Tooltip("Cooldown después de un ataque cargado (evita spam)")]
        [SerializeField] private float chargedCooldown = 2.0f;

        private BaseEntity entity;
        private float lastAttackTime = -999f;
        private float lastChargedTime = -999f;

        public bool IsCharging { get; private set; }
        public float ChargeProgress { get; private set; }
        public float MaxChargeTime => maxChargeTime;
        public bool CanCharge => !IsOnChargedCooldown;
        public bool IsOnChargedCooldown => Time.time < lastChargedTime + chargedCooldown;

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
                bool isCharged = IsCharging && ChargeProgress > 0.1f;
                FireProjectile(isCharged);
                ResetCharge();
                base.BasicAttack(); // Invokes the OnBasicAttack event (e.g. for animations)
            }
        }

        public void StartCharging()
        {
            // No permitir cargar si estamos en cooldown de ataque cargado
            if (IsOnChargedCooldown) return;
            
            IsCharging = true;
            ChargeProgress = 0f;
        }

        public void UpdateCharge()
        {
            if (IsCharging && ChargeProgress < 1f)
            {
                ChargeProgress = Mathf.Clamp01(ChargeProgress + Time.deltaTime / maxChargeTime);
            }
        }

        public void ResetCharge()
        {
            IsCharging = false;
            ChargeProgress = 0f;
        }

        private void FireProjectile(bool isCharged)
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

                // Apply charged multipliers if aiming
                if (isCharged)
                {
                    float t = ChargeProgress; // 0 to 1 based on how long charged
                    float sizeMult = Mathf.Lerp(1f, chargedSizeMultiplier, t);
                    float speedMult = Mathf.Lerp(1f, chargedSpeedMultiplier, t);
                    float damageMult = Mathf.Lerp(1f, chargedDamageMultiplier, t);
                    
                    projectile.ApplyChargeMultiplier(sizeMult, speedMult, damageMult);
                    lastChargedTime = Time.time; // Iniciar cooldown de ataque cargado
                    Debug.Log($"[RangedCombat] Charged attack! Size:{sizeMult:F2}x Speed:{speedMult:F2}x Damage:{damageMult:F2}x");
                }
            }
        }
    }
}
