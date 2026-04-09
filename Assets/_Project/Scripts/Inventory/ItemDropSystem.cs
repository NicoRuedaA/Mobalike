using UnityEngine;
using System.Collections.Generic;
using MobaGameplay.Inventory;

namespace MobaGameplay.Inventory
{
    /// <summary>
    /// Sistema estático para generar drops de items en el mundo.
    /// Gestiona la instanciación de GroundItems desde el pool de items disponibles.
    /// </summary>
    public static class ItemDropSystem
    {
        [Header("Drop Settings")]
        private const float DEFAULT_DROP_CHANCE = 0.3f;
        private const float SPAWN_OFFSET_Y = 0.5f;

        // Cache de items disponibles
        private static ItemData[] _cachedItems;
        private static bool _isInitialized = false;

        /// <summary>
        /// Array público de todos los items disponibles para drop.
        /// Asignar desde el inspector o llamar a Initialize() para cargar automáticamente.
        /// </summary>
        public static ItemData[] AvailableItems
        {
            get
            {
                if (!_isInitialized)
                {
                    Initialize();
                }
                return _cachedItems;
            }
            set
            {
                _cachedItems = value;
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Inicializa el sistema cargando todos los items disponibles.
        /// Debe llamarse al inicio del juego.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            // Cargar todos los ItemData desde Resources
            // Nota: Los items deben estar en una carpeta Resources/ScriptableObjects/Items
            _cachedItems = Resources.LoadAll<ItemData>("ScriptableObjects/Items");

            if (_cachedItems == null || _cachedItems.Length == 0)
            {
                Debug.LogWarning("[ItemDropSystem] No items found in Resources/ScriptableObjects/Items. " +
                    "Please assign AvailableItems manually or move items to Resources folder.");
            }
            else
            {
                Debug.Log($"[ItemDropSystem] Initialized with {_cachedItems.Length} items.");
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Genera un drop aleatorio en la posición especificada.
        /// </summary>
        /// <param name="position">Posición donde spawnear el item.</param>
        /// <param name="dropChance">Probabilidad de drop (0.0 a 1.0). Default: 0.3 (30%)</param>
        /// <returns>El GroundItem instanciado, o null si no hubo drop.</returns>
        public static GroundItem DropRandomItem(Vector3 position, float dropChance = DEFAULT_DROP_CHANCE)
        {
            // Verificar probabilidad de drop
            if (Random.value > dropChance)
            {
                return null;
            }

            // Asegurar que tenemos items cargados
            if (!_isInitialized)
            {
                Initialize();
            }

            // Verificar que hay items disponibles
            if (_cachedItems == null || _cachedItems.Length == 0)
            {
                Debug.LogWarning("[ItemDropSystem] Cannot drop item: No items available. " +
                    "Make sure items are in Resources folder or assign AvailableItems manually.");
                return null;
            }

            // Seleccionar item aleatorio
            ItemData randomItem = _cachedItems[Random.Range(0, _cachedItems.Length)];

            // Spawnear el GroundItem
            return SpawnGroundItem(randomItem, position);
        }

        /// <summary>
        /// Genera un drop específico en la posición indicada.
        /// </summary>
        /// <param name="itemData">Item específico a dropear.</param>
        /// <param name="position">Posición donde spawnear.</param>
        /// <returns>El GroundItem instanciado.</returns>
        public static GroundItem DropSpecificItem(ItemData itemData, Vector3 position)
        {
            if (itemData == null)
            {
                Debug.LogWarning("[ItemDropSystem] Cannot drop null item.");
                return null;
            }

            return SpawnGroundItem(itemData, position);
        }

        /// <summary>
        /// Genera múltiples drops desde una lista de items.
        /// </summary>
        /// <param name="position">Posición base donde spawnear.</param>
        /// <param name="items">Lista de items a dropear.</param>
        /// <param name="spreadRadius">Radio de dispersión para múltiples items.</param>
        public static void DropMultipleItems(Vector3 position, List<ItemData> items, float spreadRadius = 1f)
        {
            if (items == null || items.Count == 0) return;

            for (int i = 0; i < items.Count; i++)
            {
                Vector3 offset = Vector3.zero;
                if (items.Count > 1)
                {
                    // Distribuir items en círculo
                    float angle = (i / (float)items.Count) * Mathf.PI * 2f;
                    offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spreadRadius;
                }

                DropSpecificItem(items[i], position + offset);
            }
        }

        /// <summary>
        /// Instancia el prefab de GroundItem y lo configura.
        /// </summary>
        private static GroundItem SpawnGroundItem(ItemData itemData, Vector3 position)
        {
            // Cargar prefab desde Resources
            GameObject prefab = Resources.Load<GameObject>("Prefabs/Items/GroundItem");

            if (prefab == null)
            {
                Debug.LogError("[ItemDropSystem] GroundItem prefab not found at Resources/Prefabs/Items/GroundItem. " +
                    "Please create the prefab and place it in the Resources folder.");
                return null;
            }

            // Calcular posición con offset
            Vector3 spawnPosition = position + new Vector3(0f, SPAWN_OFFSET_Y, 0f);

            // Instanciar
            GameObject instance = Object.Instantiate(prefab, spawnPosition, Quaternion.identity);

            // Configurar GroundItem
            GroundItem groundItem = instance.GetComponent<GroundItem>();
            if (groundItem == null)
            {
                Debug.LogError("[ItemDropSystem] GroundItem prefab is missing GroundItem component.");
                Object.Destroy(instance);
                return null;
            }

            groundItem.Initialize(itemData);

            Debug.Log($"[ItemDropSystem] Dropped {itemData.itemName} at {spawnPosition}");

            return groundItem;
        }

        /// <summary>
        /// Asigna manualmente el array de items disponibles.
        /// Útil cuando los items no están en Resources.
        /// </summary>
        /// <param name="items">Array de items disponibles para drop.</param>
        public static void SetAvailableItems(ItemData[] items)
        {
            _cachedItems = items;
            _isInitialized = true;

            Debug.Log($"[ItemDropSystem] Set {items?.Length ?? 0} available items manually.");
        }
    }
}
