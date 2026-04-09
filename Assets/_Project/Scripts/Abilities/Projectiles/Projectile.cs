using UnityEngine;
using MobaGameplay.Core;
using MobaGameplay.Combat;

namespace MobaGameplay.Abilities.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float maxRange = 20f;
        [SerializeField] private GameObject hitEffectPrefab;

        private float damage;
        private BaseEntity owner;
        private Vector3 startPosition;

        public void Initialize(BaseEntity ownerEntity, Vector3 targetDirection, float projSpeed, float maxDistance, float projDamage)
        {
            owner = ownerEntity;
            speed = projSpeed;
            maxRange = maxDistance;
            damage = projDamage;
            startPosition = transform.position;

            // Orient the projectile towards the target direction
            if (targetDirection != Vector3.zero)
            {
                transform.forward = targetDirection;
            }

            // Apply velocity
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = true; // Use Transform translation or manual velocity to avoid physics gravity issues by default
        }

        private void Update()
        {
            // Move forward
            transform.Translate(Vector3.forward * speed * Time.deltaTime);

            // Destroy if exceeded max range
            if (Vector3.Distance(startPosition, transform.position) > maxRange)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Ignore collision with the owner
            BaseEntity hitEntity = other.GetComponentInParent<BaseEntity>();
            if (hitEntity != null && hitEntity == owner)
            {
                return;
            }

            // Apply damage to the hit entity
            if (hitEntity != null && !hitEntity.IsDead)
            {
                hitEntity.TakeDamage(new DamageInfo(damage, DamageType.Physical, owner));
            }

            // Spawn hit effect if available
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            // Destroy the projectile
            Destroy(gameObject);
        }
    }
}