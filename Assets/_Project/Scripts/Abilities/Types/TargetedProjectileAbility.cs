using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Abilities.Projectiles;

namespace MobaGameplay.Abilities.Types
{
    public class TargetedProjectileAbility : BaseAbility
    {
        [Header("Targeted Spell Settings")]
        [SerializeField] private HomingProjectile projectilePrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float projectileSpeed = 15f;
        [SerializeField] private float turnSpeed = 10f; // Qué tan rápido dobla el misil (si el enemigo hace un flash/dash, ¿puede esquivarlo?)
        [SerializeField] private float damage = 150f;
        [SerializeField] private float castRange = 8f;

        public override bool CanCast()
        {
            // Podrías chequear maná o silenciamientos aquí
            return base.CanCast();
        }

        public override void ExecuteCast(Vector3 targetPosition, BaseEntity targetEntity)
        {
            if (!CanCast()) return;

            // 1. Verificamos que hayamos hecho click en una Entidad (Enemigo), no en el suelo vacío
            if (targetEntity == null)
            {
                Debug.LogWarning($"[{abilityName}] Requires an entity target! Cast failed, no target selected.");
                return;
            }

            // 2. Opcional: Verificamos si es nuestro propio jugador
            if (targetEntity == ownerEntity)
            {
                Debug.LogWarning($"[{abilityName}] Cannot cast on yourself.");
                return;
            }

            // 3. Verificamos el Rango
            float distanceToTarget = Vector3.Distance(ownerEntity.transform.position, targetEntity.transform.position);
            if (distanceToTarget > castRange)
            {
                Debug.LogWarning($"[{abilityName}] Target out of range ({distanceToTarget} > {castRange}).");
                return;
            }

            // --- ¡Éxito! Lanzamos el misil ---
            
            // Mirar hacia el objetivo
            ownerEntity.Movement.LookAtPoint(targetEntity.transform.position);

            Vector3 startPos = spawnPoint != null ? spawnPoint.position : ownerEntity.transform.position + Vector3.up * 1f;

            HomingProjectile proj = Instantiate(projectilePrefab, startPos, Quaternion.identity);
            proj.Initialize(ownerEntity, targetEntity, projectileSpeed, turnSpeed, damage);

            // Al llamar a la base, el cooldown empieza a correr
            base.ExecuteCast(targetPosition, targetEntity);
        }
    }
}