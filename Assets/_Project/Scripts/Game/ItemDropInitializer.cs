using UnityEngine;
using System.Collections.Generic;
using MobaGameplay.Inventory;
using MMORPG.Inventory;

namespace MobaGameplay.Game
{
    /// <summary>
    /// Inicializador del sistema de drops que se ejecuta al inicio del juego.
    /// Asigna manualmente los items disponibles para evitar depender de Resources.
    /// </summary>
    public class ItemDropInitializer : MonoBehaviour
    {
        [Header("Item Database")]
        [Tooltip("Todos los items disponibles para drop. Asignar desde el Inspector.")]
        [SerializeField] private List<ItemData> _availableItems = new List<ItemData>();

        [Header("Settings")]
        [Tooltip("Destruir este componente después de inicializar.")]
        [SerializeField] private bool _destroyAfterInit = true;

        [Tooltip("Log de debug al inicializar.")]
        [SerializeField] private bool _logInitialization = true;

        private void Awake()
        {
            InitializeItemDropSystem();
        }

        /// <summary>
        /// Inicializa el ItemDropSystem con los items asignados.
        /// </summary>
        private void InitializeItemDropSystem()
        {
            if (_availableItems == null || _availableItems.Count == 0)
            {
                Debug.LogWarning("[ItemDropInitializer] No items assigned! Please populate the AvailableItems list in the Inspector.");
                return;
            }

            // Convertir lista a array y asignar al sistema
            ItemData[] itemsArray = _availableItems.ToArray();
            ItemDropSystem.SetAvailableItems(itemsArray);

            if (_logInitialization)
            {
                Debug.Log($"[ItemDropInitializer] Initialized with {itemsArray.Length} items.");
            }

            // Auto-destruir si está configurado
            if (_destroyAfterInit)
            {
                Destroy(this);
            }
        }

        /// <summary>
        /// Agrega un item a la lista de items disponibles (útil para testing).
        /// </summary>
        public void AddItem(ItemData item)
        {
            if (item != null && !_availableItems.Contains(item))
            {
                _availableItems.Add(item);
            }
        }

        /// <summary>
        /// Remueve un item de la lista de items disponibles.
        /// </summary>
        public void RemoveItem(ItemData item)
        {
            if (item != null)
            {
                _availableItems.Remove(item);
            }
        }

        /// <summary>
        /// Limpia todos los items de la lista.
        /// </summary>
        public void ClearItems()
        {
            _availableItems.Clear();
        }

        /// <summary>
        /// Obtiene la cantidad de items configurados.
        /// </summary>
        public int GetItemCount()
        {
            return _availableItems?.Count ?? 0;
        }
    }
}
