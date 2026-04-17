using System.Collections.Generic;
using UnityEngine;
using MobaGameplay.Combat;
using MobaGameplay.Core;

namespace MobaGameplay.Abilities.AreaEffects
{
    /// <summary>
    /// Zona de daño persistente que forma parte del camino de la habilidad 4.
    /// Aplica daño por segundo (DoT) a los enemigos que permanezcan dentro y se destruye al expirar.
    /// Requiere un BoxCollider con isTrigger = true en el mismo GameObject.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class TrailZone : MonoBehaviour
    {
        [Header("Zone Settings")]
        [SerializeField] private float zoneHeight = 2f;

        private float _damagePerSecond;
        private float _duration;
        private BaseEntity _owner;
        private float _elapsed;
        private BoxCollider _boxCollider;

        private readonly HashSet<EnemyEntity> _entitiesInside = new HashSet<EnemyEntity>();

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider>();
            _boxCollider.isTrigger = true;
        }

        /// <summary>Inicializa la zona con sus parámetros de daño, duración y dueño.</summary>
        /// <param name="damagePerSecond">Daño por segundo aplicado a cada enemigo dentro.</param>
        /// <param name="duration">Segundos hasta que la zona desaparece.</param>
        /// <param name="owner">Entidad dueña de la habilidad.</param>
        /// <param name="segmentLength">Longitud del segmento para ajustar el collider.</param>
        /// <param name="segmentWidth">Ancho del segmento para ajustar el collider.</param>
        public void Initialize(float damagePerSecond, float duration, BaseEntity owner,
                               float segmentLength, float segmentWidth)
        {
            _damagePerSecond = damagePerSecond;
            _duration = duration;
            _owner = owner;
            _elapsed = 0f;

            // Ajustar el collider a las dimensiones del segmento
            _boxCollider.size = new Vector3(segmentWidth, zoneHeight, segmentLength);
            _boxCollider.center = Vector3.zero;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;

            if (_elapsed >= _duration)
            {
                Destroy(gameObject);
                return;
            }

            // Limpiar referencias nulas o entidades muertas antes de iterar
            _entitiesInside.RemoveWhere(e => e == null || e.IsDead);

            // Detectar enemigos manualmente con OverlapBox (respaldo para triggers)
            DetectEnemiesWithOverlap();

            float tickDamage = _damagePerSecond * Time.deltaTime;
            foreach (EnemyEntity enemy in _entitiesInside)
            {
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.TakeDamage(new DamageInfo(tickDamage, DamageType.Magical, _owner));
                    #if UNITY_EDITOR
                    Debug.Log($"[TrailZone] Dealing {tickDamage:F1} damage to {enemy.gameObject.name}. Total entities: {_entitiesInside.Count}");
                    #endif
                }
            }
        }

        /// <summary>
        /// Detecta enemigos dentro del área usando Physics.OverlapBox.
        /// Esto funciona como respaldo cuando OnTriggerEnter no detecta correctamente.
        /// </summary>
        private void DetectEnemiesWithOverlap()
        {
            Vector3 center = transform.position + _boxCollider.center;
            Vector3 halfExtents = _boxCollider.size * 0.5f;
            
            Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);
            foreach (Collider hit in hits)
            {
                EnemyEntity enemy = hit.GetComponentInParent<EnemyEntity>();
                if (enemy != null && !enemy.IsDead && !_entitiesInside.Contains(enemy))
                {
                    _entitiesInside.Add(enemy);
                    #if UNITY_EDITOR
                    Debug.Log($"[TrailZone] Detected enemy via OverlapBox: {enemy.gameObject.name}");
                    #endif
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            #if UNITY_EDITOR
            Debug.Log($"[TrailZone] OnTriggerEnter: {other.gameObject.name} (Layer: {other.gameObject.layer})");
            #endif
            
            EnemyEntity enemy = other.GetComponentInParent<EnemyEntity>();
            if (enemy != null && !enemy.IsDead)
            {
                _entitiesInside.Add(enemy);
                #if UNITY_EDITOR
                Debug.Log($"[TrailZone] Added enemy to tracking: {enemy.gameObject.name}");
                #endif
            }
            else
            {
                #if UNITY_EDITOR
                Debug.Log($"[TrailZone] No EnemyEntity found on {other.gameObject.name}");
                #endif
            }
        }

        private void OnTriggerExit(Collider other)
        {
            EnemyEntity enemy = other.GetComponentInParent<EnemyEntity>();
            if (enemy != null)
            {
                _entitiesInside.Remove(enemy);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.4f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, _boxCollider != null ? _boxCollider.size : Vector3.one);
        }
    }
}
