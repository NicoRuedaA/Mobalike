using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Abilities.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class HomingProjectile : MonoBehaviour
    {
        [Header("Homing Settings")]
        [SerializeField] private float speed = 12f;
        [SerializeField] private float turnSpeed = 10f; // Velocidad de rotación para que no sea un giro instantáneo
        [SerializeField] private GameObject hitEffectPrefab;

        private float damage;
        private BaseEntity owner;
        private BaseEntity target;
        private Vector3 currentDirection;

        public void Initialize(BaseEntity ownerEntity, BaseEntity targetEntity, float projSpeed, float turnSpd, float projDamage)
        {
            owner = ownerEntity;
            target = targetEntity;
            speed = projSpeed;
            turnSpeed = turnSpd;
            damage = projDamage;

            // Dirección inicial
            if (target != null)
            {
                currentDirection = (target.transform.position + Vector3.up * 1f - transform.position).normalized;
                transform.forward = currentDirection;
            }

            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        private void Update()
        {
            if (target != null)
            {
                // Si el objetivo existe, calculamos la nueva dirección hacia su centro (asumiendo +1 en Y para el pecho)
                Vector3 targetPos = target.transform.position + Vector3.up * 1f;
                Vector3 desiredDirection = (targetPos - transform.position).normalized;

                // Rotación suave hacia el objetivo
                currentDirection = Vector3.Lerp(currentDirection, desiredDirection, turnSpeed * Time.deltaTime).normalized;
                transform.forward = currentDirection;
            }
            else
            {
                // Si el objetivo murió o desapareció mientras el proyectil volaba, sigue recto
                // Podríamos destruirlo, pero visualmente es mejor que vuele y se estrelle
                if (Vector3.Distance(transform.position, owner.transform.position) > 30f)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            // Movimiento constante
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            BaseEntity hitEntity = other.GetComponentInParent<BaseEntity>();
            
            if (hitEntity != null && hitEntity == owner) return;

            // Si el proyectil impacta algo, explota. 
            // Podríamos hacerlo para que SOLO explote si golpea a 'target', pero usualmente en MOBA estos proyectiles 
            // golpean al primer enemigo que interceptan (o solo explotan si es el 'target').
            if (hitEntity != null)
            {
                // Implementación temporal de daño directo si es un enemigo
                if (hitEntity is EnemyEntity enemy)
                {
                    enemy.TakeDamage(damage);
                }
                else
                {
                    Debug.Log($"Homing Projectile hit: {hitEntity.gameObject.name} for {damage} damage.");
                }
            }
            
            // Efecto
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}