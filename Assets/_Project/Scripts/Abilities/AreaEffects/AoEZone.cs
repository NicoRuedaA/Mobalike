using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Abilities.AreaEffects
{
    public class AoEZone : MonoBehaviour
    {
        [Header("Zone Settings")]
        public float delay = 1.0f;
        public float radius = 3.0f;
        public float damage = 100f;
        public GameObject explosionEffectPrefab;
        public LayerMask targetLayer = ~0; // Por defecto golpea todo, ajustable en inspector

        private float timer;
        private BaseEntity owner;
        private bool hasExploded = false;

        public void Initialize(BaseEntity ownerEntity, float zoneDelay, float zoneRadius, float zoneDamage, LayerMask layer)
        {
            owner = ownerEntity;
            delay = zoneDelay;
            radius = zoneRadius;
            damage = zoneDamage;
            targetLayer = layer;
            timer = delay;
        }

        private void Update()
        {
            if (hasExploded) return;

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Explode();
            }
        }

        private void Explode()
        {
            hasExploded = true;

            // Reproducir partículas
            if (explosionEffectPrefab != null)
            {
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            }

            // Buscar objetivos en el área
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, targetLayer);
            foreach (Collider hit in hits)
            {
                BaseEntity hitEntity = hit.GetComponentInParent<BaseEntity>();
                
                // Si golpeó a una entidad y no es el dueño
                if (hitEntity != null && hitEntity != owner)
                {
                    // En el futuro: hitEntity.TakeDamage(damage);
                    Debug.Log($"[AoE] {hitEntity.gameObject.name} hit by explosion for {damage} damage!");
                }
            }

            // Destruir el indicador/zona de efecto
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}