using System;
using System.Collections.Generic;
using UnityEngine;
using MobaGameplay.Core;

namespace MMORPG.Inventory
{
    public class EquipmentComponent : MonoBehaviour
    {
        private Dictionary<EquipSlot, ItemData> equippedItems = new Dictionary<EquipSlot, ItemData>();

        public event Action<EquipSlot, ItemData> OnEquipmentChanged;
        public event Action<int, int, int> OnStatsChanged; // HP, STR, AGI

        public int TotalHP { get; private set; }
        public int TotalSTR { get; private set; }
        public int TotalAGI { get; private set; }

        // Referencia al owner para aplicar stats
        private BaseEntity _owner;

        // Stats base del owner para poder restaurarlos
        private float _baseMaxHealth;
        private float _baseAttackDamage;

        private void Awake()
        {
            _owner = GetComponent<BaseEntity>();
            
            // Guardar stats base si existe el owner
            if (_owner != null)
            {
                _baseMaxHealth = _owner.MaxHealth;
                _baseAttackDamage = _owner.AttackDamage;
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
            // Restaurar stats base antes de recalcular
            if (_owner != null)
            {
                _owner.MaxHealth = _baseMaxHealth;
                _owner.AttackDamage = _baseAttackDamage;
            }

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
        /// </summary>
        private void ApplyStatsToOwner(int hp, int str, int agi)
        {
            if (_owner == null) return;

            // Aplicar HP como bonus a MaxHealth
            // HP bonus = 10 por punto de HP del item
            _owner.MaxHealth += hp * 10f;

            // Aplicar STR como AttackDamage
            // STR bonus = 2 por punto de STR del item
            _owner.AttackDamage += str * 2f;

            // Aplicar AGI como... (dejar comentado para futuro, o como AttackSpeed)
            // _owner.AttackSpeed += agi * 0.1f;
        }
    }
}