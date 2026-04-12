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
            _boxCollider.size = new Vector3(segmentWidth, 2f, segmentLength);
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

            float tickDamage = _damagePerSecond * Time.deltaTime;
            foreach (EnemyEntity enemy in _entitiesInside)
            {
                enemy.TakeDamage(new DamageInfo(tickDamage, DamageType.Magical, _owner));
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            EnemyEntity enemy = other.GetComponentInParent<EnemyEntity>();
            if (enemy != null && !enemy.IsDead)
            {
                _entitiesInside.Add(enemy);
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
