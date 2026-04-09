using UnityEngine;
using MobaGameplay.Inventory;

namespace MobaGameplay.Inventory
{
    /// <summary>
    /// Componente para detectar y recoger items del suelo.
    /// Debe estar adjunto al jugador (HeroEntity).
    /// </summary>
    [RequireComponent(typeof(InventoryComponent))]
    public class ItemPickupDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [Tooltip("Radio de detección para recoger items.")]
        [SerializeField] private float _detectionRadius = 2.0f;

        [Tooltip("LayerMask para filtrar solo GroundItems.")]
        [SerializeField] private LayerMask _groundItemLayer;

        [Header("Visual Effects")]
        [Tooltip("Prefab de partículas al recoger un item.")]
        [SerializeField] private GameObject _pickupVfxPrefab;

        // Referencias cacheadas
        private InventoryComponent _inventory;
        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
            _inventory = GetComponent<InventoryComponent>();

            if (_inventory == null)
            {
                Debug.LogError("[ItemPickupDetector] InventoryComponent is required but not found.");
                enabled = false;
                return;
            }

            // Configurar LayerMask por defecto si no está asignado
            if (_groundItemLayer == 0)
            {
                _groundItemLayer = LayerMask.GetMask("Default");
            }
        }

        private void Update()
        {
            DetectAndPickupItems();
        }

        /// <summary>
        /// Detecta GroundItems cercanos y los recoge.
        /// </summary>
        private void DetectAndPickupItems()
        {
            // Usar OverlapSphere para detectar items cercanos
            Collider[] hitColliders = Physics.OverlapSphere(_transform.position, _detectionRadius, _groundItemLayer);

            foreach (var hitCollider in hitColliders)
            {
                // Verificar si tiene el tag GroundItem
                if (!hitCollider.CompareTag("GroundItem"))
                {
                    continue;
                }

                // Obtener componente GroundItem
                GroundItem groundItem = hitCollider.GetComponent<GroundItem>();
                if (groundItem == null)
                {
                    groundItem = hitCollider.GetComponentInParent<GroundItem>();
                }

                if (groundItem != null && groundItem.ItemData != null)
                {
                    TryPickupItem(groundItem);
                }
            }
        }

        /// <summary>
        /// Intenta recoger un item del suelo.
        /// </summary>
        /// <param name="groundItem">El GroundItem a recoger.</param>
        private void TryPickupItem(GroundItem groundItem)
        {
            if (groundItem == null || groundItem.ItemData == null) return;

            // Intentar agregar al inventario
            bool added = _inventory.AddItem(groundItem.ItemData);

            if (added)
            {
                // Mostrar efecto visual
                SpawnPickupEffect(groundItem.transform.position);

                Debug.Log($"[ItemPickupDetector] Picked up {groundItem.ItemData.itemName}");

                // Destruir el GroundItem
                Destroy(groundItem.gameObject);
            }
            else
            {
                // Inventario lleno - el item se queda en el suelo
                Debug.Log($"[ItemPickupDetector] Inventory full! Cannot pick up {groundItem.ItemData.itemName}");
            }
        }

        /// <summary>
        /// Instancia el efecto visual de recolección.
        /// </summary>
        /// <param name="position">Posición donde spawnear el efecto.</param>
        private void SpawnPickupEffect(Vector3 position)
        {
            if (_pickupVfxPrefab != null)
            {
                Instantiate(_pickupVfxPrefab, position, Quaternion.identity);
            }
        }

        /// <summary>
        /// Dibuja el radio de detección en el editor.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        }
    }
}
