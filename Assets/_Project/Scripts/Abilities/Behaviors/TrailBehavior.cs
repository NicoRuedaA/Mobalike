using System.Collections;
using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Abilities.AreaEffects;

namespace MobaGameplay.Abilities.Behaviors
{
    /// <summary>
    /// Spawns a trail of damage zones along a direction path.
    /// Inspired by Rumble's R (Equalizer) — zones appear progressively.
    /// Used for: Ground Trail, wall of fire, etc.
    /// </summary>
    public class TrailBehavior : IAbilityBehavior
    {
        private const float DIRECTION_THRESHOLD = 0.001f;

        public void Execute(AbilityContext context)
        {
            if (context.Owner == null || context.Data == null) return;

            AbilityData data = context.Data;

            if (data.trailZonePrefab == null)
            {
                Debug.LogError($"[TrailBehavior] No trail zone prefab for '{data.abilityName}'");
                return;
            }

            // Calculate direction (flat on XZ plane)
            Vector3 origin = context.Owner.transform.position;
            Vector3 direction = context.TargetPosition - origin;
            direction.y = 0f;

            if (direction.sqrMagnitude < DIRECTION_THRESHOLD)
            {
                direction = context.Owner.transform.forward;
            }

            direction.Normalize();

            // Face the target instantly (abilities snap rotation, no smooth turn)
            if (direction.sqrMagnitude > DIRECTION_THRESHOLD)
            {
                context.Owner.transform.forward = direction;
            }

            // Spawn trail zones progressively using coroutine
            // We need a MonoBehaviour to start the coroutine, so we use the owner
            TrailCoroutineRunner runner = context.Owner.gameObject.AddComponent<TrailCoroutineRunner>();
            runner.StartTrail(data, origin, direction, context.Owner);
        }

        /// <summary>
        /// Helper MonoBehaviour to run the trail spawning coroutine.
        /// Auto-destroys when the coroutine finishes.
        /// </summary>
        private class TrailCoroutineRunner : MonoBehaviour
        {
            public void StartTrail(AbilityData data, Vector3 origin, Vector3 direction, BaseEntity owner)
            {
                StartCoroutine(SpawnTrailZones(data, origin, direction, owner));
            }

            private IEnumerator SpawnTrailZones(AbilityData data, Vector3 origin, Vector3 direction, BaseEntity owner)
            {
                float segmentLength = data.range / data.trailZoneCount;
                Quaternion segmentRotation = Quaternion.LookRotation(direction);

                for (int i = 0; i < data.trailZoneCount; i++)
                {
                    float distanceAlongPath = segmentLength * i + segmentLength * 0.5f;
                    Vector3 spawnPosition = origin + direction * distanceAlongPath;
                    spawnPosition.y = origin.y;

                    GameObject zoneObj = Instantiate(data.trailZonePrefab, spawnPosition, segmentRotation);

                    if (zoneObj.TryGetComponent(out TrailZone zone))
                    {
                        zone.Initialize(
                            data.trailDamagePerSecond,
                            data.trailZoneDuration,
                            owner,
                            segmentLength,
                            data.trailWidth
                        );
                    }

                    yield return new WaitForSeconds(data.trailSpawnDelay);
                }

                // Self-destruct when done
                Destroy(this);
            }
        }
    }
}