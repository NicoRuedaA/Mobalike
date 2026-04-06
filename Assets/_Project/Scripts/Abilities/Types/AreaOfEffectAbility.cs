using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Abilities.AreaEffects;

namespace MobaGameplay.Abilities.Types
{
    public class AreaOfEffectAbility : BaseAbility
    {
        [Header("AoE Settings")]
        [SerializeField] private AoEZone aoePrefab;
        [SerializeField] private float damage = 100f;
        [SerializeField] private float radius = 3.0f;
        [SerializeField] private float explosionDelay = 1.0f;
        [SerializeField] private LayerMask enemyLayer = ~0;
        
        [Header("Optional Aim Range")]
        [SerializeField] private float maxCastRange = 10f; // Max distance from player

        public override void ExecuteCast(Vector3 targetPosition, BaseEntity targetEntity)
        {
            if (!CanCast()) return;

            if (aoePrefab == null)
            {
                Debug.LogError($"[AoEAbility] No AoE zone prefab assigned for {abilityName}");
                return;
            }

            // Opcional: Limitar el rango de casteo
            Vector3 currentPos = ownerEntity.transform.position;
            Vector3 direction = targetPosition - currentPos;
            direction.y = 0f;

            if (direction.magnitude > maxCastRange)
            {
                // Limitar al borde máximo del rango
                direction = direction.normalized * maxCastRange;
                targetPosition = currentPos + direction;
            }

            // Mirar hacia la explosión (opcional)
            ownerEntity.Movement.LookAtPoint(targetPosition);

            // Instanciar la zona de advertencia/daño
            AoEZone zone = Instantiate(aoePrefab, targetPosition, Quaternion.identity);
            zone.Initialize(ownerEntity, explosionDelay, radius, damage, enemyLayer);

            // Trigger cooldown en la clase base
            base.ExecuteCast(targetPosition, targetEntity);
        }
    }
}