using System.Collections;
using UnityEngine;
using MobaGameplay.Abilities.AreaEffects;
using MobaGameplay.Core;
using MobaGameplay.UI.Targeting;

namespace MobaGameplay.Abilities
{
    /// <summary>
    /// Habilidad 4 — Ground Trail inspirada en la R de Rumble.
    /// Al lanzarse, genera una serie de segmentos de zona de daño a lo largo de la
    /// dirección apuntada con un pequeño delay entre cada uno, creando el efecto
    /// de un camino que "aparece" progresivamente.
    /// </summary>
    public class GroundTrailAbility : BaseAbility
    {
        private const float DIRECTION_THRESHOLD = 0.001f;

        [Header("Trail Settings")]
        [Tooltip("Prefab de la zona de daño que forma cada segmento del camino.")]
        [SerializeField] private GameObject trailZonePrefab;

        [Tooltip("Longitud total del camino en unidades de mundo.")]
        [SerializeField] private float trailLength = 20f;

        [Tooltip("Ancho de cada segmento del camino.")]
        [SerializeField] private float trailWidth = 3f;

        [Tooltip("Daño por segundo que aplica cada zona activa.")]
        [SerializeField] private float damagePerSecond = 40f;

        [Tooltip("Segundos que cada zona permanece activa antes de destruirse.")]
        [SerializeField] private float zoneDuration = 3f;

        [Tooltip("Número de segmentos en que se divide el camino.")]
        [SerializeField] private int zoneCount = 6;

        [Tooltip("Tiempo de espera entre la aparición de cada segmento.")]
        [SerializeField] private float spawnDelay = 0.12f;

        public override void Initialize(BaseEntity owner)
        {
            base.Initialize(owner);

            // Configurar el tipo de targeting y las dimensiones para que el
            // TargetingManager muestre el indicador de trail correcto.
            TargetingType = IndicatorType.Trail;
            Range = trailLength;
            Width = trailWidth;
        }

        protected override void OnExecute(Vector3 targetPosition, BaseEntity targetEntity)
        {
            if (trailZonePrefab == null)
            {
                Debug.LogError("[GroundTrailAbility] trailZonePrefab no está asignado.");
                return;
            }

            Vector3 origin = ownerEntity.transform.position;

            // Dirección plana hacia el objetivo
            Vector3 direction = targetPosition - origin;
            direction.y = 0f;
            if (direction.sqrMagnitude < DIRECTION_THRESHOLD)
                direction = ownerEntity.transform.forward;
            direction.Normalize();

            // Rotar el player hacia el objetivo antes de lanzar
            ownerEntity.Movement.LookAtPoint(targetPosition);

            StartCoroutine(SpawnTrailZones(origin, direction));
        }

        /// <summary>
        /// Genera los segmentos del trail de forma progresiva con un delay entre cada uno.
        /// </summary>
        private IEnumerator SpawnTrailZones(Vector3 origin, Vector3 direction)
        {
            float segmentLength = trailLength / zoneCount;
            Quaternion segmentRotation = Quaternion.LookRotation(direction);

            for (int i = 0; i < zoneCount; i++)
            {
                // Centro de cada segmento a lo largo del camino
                float distanceAlongPath = segmentLength * i + segmentLength * 0.5f;
                Vector3 spawnPosition = origin + direction * distanceAlongPath;
                spawnPosition.y = origin.y; // Mantener en el suelo

                GameObject zoneObj = Instantiate(trailZonePrefab, spawnPosition, segmentRotation);

                if (zoneObj.TryGetComponent(out TrailZone zone))
                {
                    zone.Initialize(damagePerSecond, zoneDuration, ownerEntity,
                                    segmentLength, trailWidth);
                }

                yield return new WaitForSeconds(spawnDelay);
            }
        }
    }
}
