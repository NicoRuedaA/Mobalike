using System;
using System.Collections.Generic;
using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Inventory
{
    public class EquipmentComponent : MonoBehaviour
    {
        // Stat multipliers: how much each stat point adds to the hero's base stats
        private const int HP_PER_STAT_POINT = 10;  // Each HP point = +10 MaxHealth
        private const int AD_PER_STR_POINT = 2;     // Each STR point = +2 AttackDamage

        private Dictionary<EquipSlot, ItemData> equippedItems = new Dictionary<EquipSlot, ItemData>();

        public event Action<EquipSlot, ItemData> OnEquipmentChanged;
        public event Action<int, int, int> OnStatsChanged; // HP, STR, AGI

        public int TotalHP { get; private set; }
        public int TotalSTR { get; private set; }
        public int TotalAGI { get; private set; }

        // Referencia al owner para aplicar stats
        private BaseEntity _owner;

        // Stats base del owner para poder restaurarlos (incluye bonuses de nivel)
        private float _baseMaxHealth;
        private float _baseAttackDamage;

        private void Awake()
        {
            _owner = GetComponent<BaseEntity>();
            
            // Guardar stats base si existe el owner
            RefreshBaseStats();
        }

        private void Start()
        {
            // Suscribirse al level-up si el owner es un HeroEntity
            // (Start ensures HeroEntity.Awake and Start have already run)
            if (_owner is HeroEntity hero)
            {
                hero.OnLevelUp += OnOwnerLevelUp;
            }
        }

        private void OnDestroy()
        {
            // Desuscribirse para evitar memory leaks
            if (_owner is HeroEntity hero)
            {
                hero.OnLevelUp -= OnOwnerLevelUp;
            }
        }

        private void OnEnable()
        {
            OnStatsChanged += ApplyStatsToOwner;
        }

        private void OnDisable()
        {
            OnStatsChanged -= ApplyStatsToOwner;
        }

        /// <summary>
        /// Refreshes cached base stats from the owner, stripping equipment bonuses.
        /// This ensures level-up bonuses are included in the base values.
        /// Call this when: Awake, after level-up, or whenever base stats change externally.
        /// </summary>
        private void RefreshBaseStats()
        {
            if (_owner == null) return;

            // Strip current equipment bonuses to get the "naked" base (which includes level-ups)
            _baseMaxHealth = _owner.MaxHealth - (TotalHP * HP_PER_STAT_POINT);
            _baseAttackDamage = _owner.AttackDamage - (TotalSTR * AD_PER_STR_POINT);
        }

        /// <summary>
        /// Called when the owner levels up. Refreshes base stats and recalculates equipment bonuses
        /// so level-up stat gains are preserved properly.
        /// </summary>
        private void OnOwnerLevelUp(int newLevel)
        {
            RefreshBaseStats();
            RecalculateStats();
        }

        public void EquipItem(ItemData item, out ItemData previousItem)
        {
            previousItem = null;
            if (item == null || item.itemType != ItemType.Equipment || item.equipSlot == EquipSlot.None)
                return;

            if (equippedItems.ContainsKey(item.equipSlot))
            {
                previousItem = equippedItems[item.equipSlot];
            }

            equippedItems[item.equipSlot] = item;
            OnEquipmentChanged?.Invoke(item.equipSlot, item);
            RecalculateStats();
        }

        public void UnequipItem(EquipSlot slot, out ItemData removedItem)
        {
            removedItem = null;
            if (equippedItems.ContainsKey(slot))
            {
                removedItem = equippedItems[slot];
                equippedItems.Remove(slot);
                OnEquipmentChanged?.Invoke(slot, null);
                RecalculateStats();
            }
        }

        private void RecalculateStats()
        {
            // Refresh base stats first (includes level-up bonuses, strips old equipment)
            RefreshBaseStats();

            // Restore base values before computing new bonuses
            _owner.MaxHealth = _baseMaxHealth;
            _owner.AttackDamage = _baseAttackDamage;

            TotalHP = 0;
            TotalSTR = 0;
            TotalAGI = 0;

            foreach (var item in equippedItems.Values)
            {
                if (item != null)
                {
                    TotalHP += item.hpBonus;
                    TotalSTR += item.strBonus;
                    TotalAGI += item.agiBonus;
                }
            }

            OnStatsChanged?.Invoke(TotalHP, TotalSTR, TotalAGI);
        }

        /// <summary>
        /// Aplica los stats calculados al owner.
        /// HP bonus = 10 por punto de HP del item (añadido a MaxHealth)
        /// STR bonus = 2 por punto de STR del item (añadido a AttackDamage)
        /// AGI bonus = reservado para futuro (AttackSpeed)
        /// 
        /// IMPORTANT: Uses assignment (=) not accumulation (+=) because
        /// RecalculateStats() already restores base stats before calling this.
        /// </summary>
        private void ApplyStatsToOwner(int hp, int str, int agi)
        {
            if (_owner == null) return;

            // Aplicar HP como bonus a MaxHealth (assignment, NOT accumulation)
            // RecalculateStats() already restored _baseMaxHealth, so we set the final value
            _owner.MaxHealth = _baseMaxHealth + (hp * HP_PER_STAT_POINT);

            // Clamp current health to new max (prevents CurrentHealth > MaxHealth when swapping to lower HP gear)
            _owner.CurrentHealth = Mathf.Min(_owner.CurrentHealth, _owner.MaxHealth);

            // Aplicar STR como AttackDamage (assignment, NOT accumulation)
            _owner.AttackDamage = _baseAttackDamage + (str * AD_PER_STR_POINT);

            // TODO: AGI bonus reserved for future AttackSpeed implementation
            // When AttackSpeed is a settable property: _owner.AttackSpeed = _baseAttackSpeed + (agi * 0.1f);

            #if UNITY_EDITOR
Debug.Log($"[Equipment] Stats applied to {_owner.gameObject.name}: " +
                      $"HP: {_baseMaxHealth} + {hp * HP_PER_STAT_POINT} = {_owner.MaxHealth}, " +
                      $"AD: {_baseAttackDamage} + {str * AD_PER_STR_POINT} = {_owner.AttackDamage}");
            #endif
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Debug: Muestra todos los items equipados y sus stats.
        /// </summary>
        [ContextMenu("Debug: Show Equipped Items")]
        public void DebugShowEquippedItems()
        {
            string output = $"=== Equipped Items on {gameObject.name} ===\n";
            
            if (equippedItems.Count == 0)
            {
                output += "No items equipped.\n";
            }
            else
            {
                foreach (var kvp in equippedItems)
                {
                    var item = kvp.Value;
                    output += $"[{kvp.Key}] {item.itemName}: " +
                             $"HP+{item.hpBonus}, STR+{item.strBonus}, AGI+{item.agiBonus}\n";
                }
            }
            
            output += $"\nTotal Bonuses: HP+{TotalHP}, STR+{TotalSTR}, AGI+{TotalAGI}\n";
            output += $"Effective Stats: +{TotalHP * HP_PER_STAT_POINT} MaxHealth, +{TotalSTR * AD_PER_STR_POINT} AttackDamage";
            
            Debug.Log(output);
        }

        /// <summary>
        /// Debug: Equipar item de prueba (para testing rápido).
        /// </summary>
        [ContextMenu("Debug: Test Equip Random Item")]
        public void DebugTestEquipRandomItem()
        {
            // Buscar items en el proyecto
            var items = UnityEditor.AssetDatabase.FindAssets("t:ItemData");
            if (items.Length > 0)
            {
                var randomGuid = items[UnityEngine.Random.Range(0, items.Length)];
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(randomGuid);
                var item = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(path);
                
                if (item != null && item.itemType == ItemType.Equipment)
                {
                    EquipItem(item, out var previous);
                    Debug.Log($"[Equipment] Test equipped: {item.itemName}");
                }
            }
            else
            {
                Debug.LogWarning("[Equipment] No ItemData assets found in project.");
            }
        }
        #endif

        /// <summary>
        /// Obtiene el item equipado en un slot específico.
        /// </summary>
        public ItemData GetEquippedItem(EquipSlot slot)
        {
            return equippedItems.ContainsKey(slot) ? equippedItems[slot] : null;
        }

        /// <summary>
        /// Verifica si hay un item equipado en el slot.
        /// </summary>
        public bool HasItemEquipped(EquipSlot slot)
        {
            return equippedItems.ContainsKey(slot) && equippedItems[slot] != null;
        }

        /// <summary>
        /// Obtiene todos los slots que tienen items equipados.
        /// </summary>
        public IEnumerable<EquipSlot> GetEquippedSlots()
        {
            return equippedItems.Keys;
        }
    }
}